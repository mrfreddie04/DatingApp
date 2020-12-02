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
    private readonly IUnitOfWork  _unitOfWork;
    private readonly IMapper _mapper;
    public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
    {
      _mapper = mapper;
      _unitOfWork = unitOfWork;
    }

    [HttpPost] //the user to be liked by the current user
    public async Task<ActionResult<MessageDto>> SendMessage(CreateMessageDto createMessageDto)
    {
      var username = User.GetUsername();
      var recipientUserame = createMessageDto.RecipientUsername.ToLower();

      if (username == recipientUserame)
        return BadRequest("You cannot send messages to yourself");

      var sender = await _unitOfWork.UserRepository.GetUserByUserNameAsync(username);
      var recipient = await _unitOfWork.UserRepository.GetUserByUserNameAsync(recipientUserame);

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

      _unitOfWork.MessageRepository.AddMessage(message);

      if(await _unitOfWork.Complete())  
      {
        return Ok(_mapper.Map<MessageDto>(message));
      }

      return BadRequest("Failed to send message");    
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id) 
    {
      var username = User.GetUsername();

      var message = await _unitOfWork.MessageRepository.GetMessage(id);
      if(message == null)
        return NotFound();

      if (username != message.Sender.UserName && username != message.Recipient.UserName)
        return Unauthorized();        

      if (username == message.Sender.UserName)
        message.SenderDeleted = true;

      if (username == message.Recipient.UserName)
        message.RecipientDeleted = true;

      if (message.RecipientDeleted && message.SenderDeleted)
        _unitOfWork.MessageRepository.DeleteMessage(message);

      if (await _unitOfWork.Complete())
        return Ok();

      return BadRequest("Problem deleting the message");  
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();    

        var messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

        Response.AddPaginationHeader(messages.CurrentPage,messages.PageSize,messages.TotalCount,messages.TotalPages);
        
        return Ok(messages);
    }    

    // Not used anymore - replaced by SignalR sockets
    // [HttpGet("thread/{username}")]
    // public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    // {
    //     var currentUsername = User.GetUsername();    

    //     var messages = await _messageRepository.GetMessageThread(currentUsername, username);
        
    //     return Ok(messages);
    // }        

  }
}