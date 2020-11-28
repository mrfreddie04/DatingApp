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

    public void AddMessage(Message message)
    {
      _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
      _context.Messages.Remove(message); //assume message is already brought into the context
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

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
      var query = _context.Messages
          .OrderByDescending(m => m.DateSent)
          .AsQueryable();

      query = messageParams.Container switch
      {
        "Inbox" => query.Where(m => m.Recipient.UserName == messageParams.Username && !m.RecipientDeleted),
        "Outbox" => query.Where(m => m.Sender.UserName == messageParams.Username && !m.SenderDeleted),
        _ => query.Where(m => m.DateRead == null && m.Recipient.UserName == messageParams.Username && !m.RecipientDeleted)
      };
      var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

      return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
    {
        //because we are not projecting we need to eagerly load the photos for the user
        var messages = await _context.Messages
            .Include(m => m.Sender).ThenInclude(u => u.Photos)
            .Include(m => m.Recipient).ThenInclude(u => u.Photos)
            .Where( m => m.Recipient.UserName == currentUsername && m.Sender.UserName == recipientUsername && !m.RecipientDeleted||
                    m.Recipient.UserName == recipientUsername && m.Sender.UserName == currentUsername && !m.SenderDeleted
                    )
            .OrderBy(m=>m.DateSent)
            .ToListAsync();

        var unreadMessages = messages.Where(m=>m.DateRead == null && m.Recipient.UserName == currentUsername)
            .ToList();

        if(unreadMessages.Any())
        {
            foreach(var message in unreadMessages)
            {
                message.DateRead = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
      return (await _context.SaveChangesAsync()) > 0 ? true : false;
    }
  }
}