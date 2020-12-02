using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class MessageRepository : IMessageRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    public MessageRepository(DataContext context, IMapper mapper)
    {
      _mapper = mapper;
      _context = context;
    }

    public void AddGroup(Group group)
    {
      _context.Groups.Add(group);
    }

    public void AddMessage(Message message)
    {
      _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
      _context.Messages.Remove(message); //assume message is already brought into the context
    }

    public async Task<Connection> GetConnection(string connectionId)
    {
      return await _context.Connections.FindAsync(connectionId);  
    }

    public async Task<Message> GetMessage(int id)
    {
      //that is not eager loading
      //return await _context.Messages.FindAsync(id);
      return await _context.Messages
        .Include(s=>s.Sender)
        .Include(r=>r.Recipient)
        .SingleOrDefaultAsync(m=>m.Id == id);
    }

    public async Task<Group> GetMessageGroup(string groupName)
    {
      return await _context.Groups
        .Include(g => g.Connections)
        .FirstOrDefaultAsync(g=>g.Name == groupName);  
    }

    public async Task<Group> GetGroupForConnection(string connectionId)
    {
      // var query = _context.Groups.AsQueryable();
      //   query = query.Where(g => g.Connections.Any(c=>c.ConnectionId==connectionId));
      //   query = query.Include( g => g.Connections);
      // return await query.FirstOrDefaultAsync();

      return await  _context.Groups.Include( g => g.Connections)
              .Where(g => g.Connections.Any(c=>c.ConnectionId==connectionId))
              .FirstOrDefaultAsync();
    }    

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
      var query = _context.Messages
          .OrderByDescending(m => m.DateSent)
          .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
          .AsQueryable();

      query = messageParams.Container switch
      {
        "Inbox" => query.Where(m => m.RecipientUsername == messageParams.Username && !m.RecipientDeleted),
        "Outbox" => query.Where(m => m.SenderUsername == messageParams.Username && !m.SenderDeleted),
        _ => query.Where(m => m.DateRead == null && m.RecipientUsername == messageParams.Username && !m.RecipientDeleted)
      };

      return await PagedList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
    {
        //because we are not projecting we need to eagerly load the photos for the user
        var messages = await _context.Messages
            //not needed retrieved by projection
            //.Include(m => m.Sender).ThenInclude(u => u.Photos)
            //.Include(m => m.Recipient).ThenInclude(u => u.Photos)
            .Where( m => m.Recipient.UserName == currentUsername && m.Sender.UserName == recipientUsername && !m.RecipientDeleted||
                    m.Recipient.UserName == recipientUsername && m.Sender.UserName == currentUsername && !m.SenderDeleted
                    )
            .OrderBy(m=>m.DateSent)
            .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var unreadMessages = messages.Where(m=>m.DateRead == null && m.RecipientUsername == currentUsername)
            .ToList();

        if(unreadMessages.Any())
        {
            foreach(var message in unreadMessages)
            {
                message.DateRead = DateTime.UtcNow;
            }

            //await _context.SaveChangesAsync();
        }

        return messages;
    }

    public void RemoveConnection(Connection connection)
    {
      _context.Connections.Remove(connection);
    }

  }
}