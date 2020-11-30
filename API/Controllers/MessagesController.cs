using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using API.Interfaces;
using API.Extensions;
using API.Entities;
using AutoMapper;
using System.Collections.Generic;
using API.Helpers;

namespace API.Controllers
{
  [Authorize]
  public class MessagesController : BaseApiController
  {
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
    {
      _mapper = mapper;
      _userRepository = userRepository;
      _messageRepository = messageRepository;

    }

    [HttpPost] //the user to be liked by the current user
    public async Task<ActionResult<MessageDto>> SendMessage(CreateMessageDto createMessageDto)
    {
      var username = User.GetUsername();
      var recipientUserame = createMessageDto.RecipientUsername.ToLower();

      if (username == recipientUserame)
        return BadRequest("You cannot send messages to yourself");

      var sender = await _userRepository.GetUserByUserNameAsync(username);
      var recipient = await _userRepository.GetUserByUserNameAsync(recipientUserame);

      if (recipient == null)
        return NotFound();

      var message = new Message()
      {
        Sender = sender,
        Recipient = recipient,
        SenderUsername = sender.UserName,
        RecipientUsername = recipient.UserName,
        Content = createMessageDto.Content
      };

      _messageRepository.AddMessage(message);

      if(await _messageRepository.SaveAllAsync())  
      {
        return Ok(_mapper.Map<MessageDto>(message));
      }

      return BadRequest("Failed to send message");    
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id) 
    {
      var username = User.GetUsername();

      var message = await _messageRepository.GetMessage(id);
      if(message == null)
        return NotFound();

      if (username != message.Sender.UserName && username != message.Recipient.UserName)
        return Unauthorized();        

      if (username == message.Sender.UserName)
        message.SenderDeleted = true;

      if (username == message.Recipient.UserName)
        message.RecipientDeleted = true;

      if (message.RecipientDeleted && message.SenderDeleted)
        _messageRepository.DeleteMessage(message);

      if (await _messageRepository.SaveAllAsync())
        return Ok();

      return BadRequest("Problem deleting the message");  
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();    

        var messages = await _messageRepository.GetMessagesForUser(messageParams);

        Response.AddPaginationHeader(messages.CurrentPage,messages.PageSize,messages.TotalCount,messages.TotalPages);
        
        return Ok(messages);
    }    

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        var currentUsername = User.GetUsername();    

        var messages = await _messageRepository.GetMessageThread(currentUsername, username);
        
        return Ok(messages);
    }        

  }
}