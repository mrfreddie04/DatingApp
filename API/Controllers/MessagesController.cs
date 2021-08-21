using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController: BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto messageDto) 
        {
            if(String.IsNullOrEmpty(messageDto.Content)) {
                return BadRequest("Message cannot be empty");
            }

            //var senderId = User.GetUserId();
            var senderUserName = User.GetUserName();

            if(senderUserName == messageDto.RecipientUsername) {
                return BadRequest("You cannot send messages to yourself");
            }

            var sender= await _unitOfWork.UserRepository.GetUserByUserNameAsync(senderUserName);

            var recipient = await _unitOfWork.UserRepository.GetUserByUserNameAsync(messageDto.RecipientUsername);
            if(recipient == null)
                return NotFound("Recipient not found");
            
            var message = new Message()
            {
                Content = messageDto.Content,
                Sender = sender,
                SenderUsername = senderUserName,
                Recipient = recipient,
                RecipientUsername = recipient.UserName  
            };
            
            _unitOfWork.MessageRepository.AddMessage(message);

            if(await _unitOfWork.CompleteAsync())
            {
                return Ok(_mapper.Map<MessageDto>(message));
            }   
                
            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUserName();

            var pagedList = await _unitOfWork.MessageRepository.GetMessagesForUserAsync(messageParams);

            Response.AddPaginationHeader(pagedList.CurrentPage, pagedList.PageSize, pagedList.TotalCount,pagedList.TotalPages);

            return Ok(pagedList);
        }

        // [HttpGet("thread/{username}")]
        // public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        // {
        //     var currentUserName = User.GetUserName();

        //     var messageThread = await _unitOfWork.MessageRepository.GetMessageThreadAsync(currentUserName, username);

        //     return Ok(messageThread);
        // }      

        [HttpDelete("{messageId}")]
        public async Task<ActionResult> DeleteMessage(int messageId) {
            var userName = User.GetUserName();     

            var message = await _unitOfWork.MessageRepository.GetMessageAsync(messageId);
            if(message == null) {
                return NotFound("Message Not Found");
            }            

            if( message.SenderUsername != userName && message.RecipientUsername != userName)
                return Unauthorized("You cannot delete this message");

            if( message.SenderUsername == userName)
                message.SenderDeleted = true;

            if( message.RecipientUsername == userName)
                message.RecipientDeleted = true;

            if(message.SenderDeleted && message.RecipientDeleted)
                _unitOfWork.MessageRepository.DeleteMessage(message);

            if(await _unitOfWork.CompleteAsync())
            {
                return Ok();
            }   
                
            return BadRequest("Problem deleting the message");
        }
    }    
}