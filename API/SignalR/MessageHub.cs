using System.Threading.Tasks;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using API.Interfaces;
using API.Extensions;
using System;
using API.DTOs;
using API.Entities;

namespace API.SignalR
{
    public class MessageHub: Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _tracker;
        private readonly IMapper _mapper;

        public MessageHub(IMessageRepository messageRepository, IUserRepository userRepository, 
            IHubContext<PresenceHub> presenceHub, PresenceTracker tracker, IMapper mapper)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _presenceHub = presenceHub;
            _tracker = tracker;
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync() 
        {
            var currentUser = Context.User.GetUserName();
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();

            var groupName = GetGroupName(currentUser, otherUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _messageRepository.GetMessageThreadAsync(currentUser, otherUser);

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        #nullable enable    
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);

            await base.OnDisconnectedAsync(exception);
        }      
        #nullable disable

        public async Task SendMessage(CreateMessageDto createMessageDto) {
            var senderUserName = Context.User.GetUserName();

            if(senderUserName == createMessageDto.RecipientUsername) {
                throw new HubException("You cannot send messages to yourself");
            }

            var sender= await _userRepository.GetUserByUserNameAsync(senderUserName);

            var recipient = await _userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);
            if(recipient == null)
                throw new HubException("Recipient not found");      

            var message = new Message()
            {
                Content = createMessageDto.Content,
                Sender = sender,
                SenderUsername = senderUserName,
                Recipient = recipient,
                RecipientUsername = recipient.UserName  
            };
            
            var groupName = GetGroupName(sender.UserName, recipient.UserName); 
            var group = await _messageRepository.GetMessageGroupAsync(groupName);
            if(group.Connections.Any(c => c.UserName == recipient.UserName)) {
                message.DateRead = DateTime.UtcNow;
            } 
            else 
            {
                var connections = await _tracker.GetConnectionsForUser(recipient.UserName);
                if(connections != null) {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new {username=sender.UserName, knownAs = sender.KnownAs});
                }
            }

            _messageRepository.AddMessage(message);

            if(await _messageRepository.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));                
            }   
            else 
            {
                throw new HubException("Failed to send message");                      
            }                
        }

        private string GetGroupName(string caller, string other) {
            return string.Join("-",(new [] {caller, other}).OrderBy( x => x).ToArray());
        }

        private async Task<Group> AddToGroup(string groupName) 
        {
            var group = await _messageRepository.GetMessageGroupAsync(groupName);
            if(group == null)
            {
                group = new Group(groupName);
                _messageRepository.AddGroup(group);
            }   

            var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());

            group.Connections.Add(connection);

            if(await _messageRepository.SaveAllAsync())
                return group;

            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup() 
        {
            var connectionId = Context.ConnectionId;

            var group = await _messageRepository.GetGroupForConnectionAsync(connectionId);
            if( group == null) 
                throw new HubException("Connection not found");                

            //var connection = await _messageRepository.GetConnectionAsync(connectionId);
            var connection = group.Connections.FirstOrDefault( c => c.ConnectionId == connectionId);         

            _messageRepository.RemoveConnection(connection);

            if(await _messageRepository.SaveAllAsync())
                return group;

            throw new HubException("Failed to remove from group");                
        }        
    }
}