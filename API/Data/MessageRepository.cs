using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using System;

namespace API.Data
{
  public class MessageRepository : IMessageRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MessageRepository(DataContext context, IMapper mapper)
    {
      _context = context;
      _mapper = mapper;
    }

    public void AddGroup(Group group)
    {
      _context.Groups.Add(group);
    }

    public void RemoveConnection(Connection connection)
    {
      _context.Connections.Remove(connection);
    }    

    public async Task<Connection> GetConnectionAsync(string connectionId)
    {
      return await _context.Connections.FindAsync(connectionId);
    }

    public async Task<Group> GetMessageGroupAsync(string groupName)
    {
      return await _context.Groups
        .Include(g => g.Connections)
        .SingleOrDefaultAsync( g => g.Name == groupName);
    }    

    public async Task<Group> GetGroupForConnectionAsync(string connectionId){
      return await _context.Groups
        .Include(g => g.Connections)
        .Where( g => g.Connections.Any( c => c.ConnectionId == connectionId))
        .FirstOrDefaultAsync();
    }

    public void AddMessage(Message message)
    {
      _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
      _context.Messages.Remove(message);
    }


    public async Task<Message> GetMessageAsync(int id)
    {
      return await _context.Messages.FindAsync(id);

      // return await _context.Messages
      //   .Include( m => m.Sender)
      //   .Include( m => m.Recipient)
      //   .SingleOrDefaultAsync( m => m.Id == id);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams)
    {
        //build query
        var query = _context.Messages
            .OrderByDescending( m => m.DateSent)
            .AsQueryable();

        query = messageParams.Container switch 
        {
            "Inbox" => query.Where(m => m.RecipientUsername == messageParams.Username && !m.RecipientDeleted),
            "Outbox" => query.Where(m => m.SenderUsername == messageParams.Username  && !m.SenderDeleted),
            _ => query.Where(m => m.RecipientUsername == messageParams.Username && !m.RecipientDeleted && m.DateRead == null)    
        };

        //create PagedList
        return await PagedList<MessageDto>.CreateAsync(
            query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider).AsNoTracking(), 
            messageParams.PageNumber,
            messageParams.PageSize
        );      
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUserName, string recipientName)
    {
        //get all messages in-memory
        var messages = await _context.Messages
            .Include( m => m.Sender).ThenInclude( au => au.Photos )
            .Include( m => m.Recipient).ThenInclude( au => au.Photos )
            .Where( m => 
                m.Sender.UserName == currentUserName && !m.SenderDeleted && m.Recipient.UserName == recipientName ||
                m.Sender.UserName == recipientName && !m.RecipientDeleted && m.Recipient.UserName == currentUserName)
            .OrderBy( m => m.DateSent)    
            .ToListAsync();

        var unreadMessages = messages
            .Where( m => m.Recipient.UserName == currentUserName && m.DateRead == null)
            .ToList();

        //update unread
        if(unreadMessages.Any()) 
        {
            foreach(var message in unreadMessages) 
            {
                message.DateRead = DateTime.UtcNow; 
            }

            await _context.SaveChangesAsync();
        }

        //map 
        return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
      if(_context.ChangeTracker.HasChanges()) {
        var updates = await _context.SaveChangesAsync();
        return (updates>0);
      }
      return false;
    }
  }
}