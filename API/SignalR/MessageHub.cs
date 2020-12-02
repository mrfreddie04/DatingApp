using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using API.Interfaces;
using API.Extensions;
using API.DTOs;
using API.Entities;
using System.Linq;

namespace API.SignalR
{
  public class MessageHub : Hub
  {
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHub;
    private readonly PresenceTracker _tracker;

    public MessageHub(IUnitOfWork unitOfWork, IHubContext<PresenceHub> presenceHub, PresenceTracker tracker, IMapper mapper)
    {
      _unitOfWork = unitOfWork;      
      _tracker = tracker;
      _presenceHub = presenceHub;
      _mapper = mapper;
    }

    public override async Task OnConnectedAsync()
    {
      string caller = Context.User.GetUsername();
      var httpContext = Context.GetHttpContext();
      //when we make connection to Message Hyb we will pass the user name in the query string
      var other = httpContext.Request.Query["user"].ToString();
      var groupName = GetGroupName(caller, other);

      //Create a Hub group
      await Groups.AddToGroupAsync(Context.ConnectionId, groupName);  

      var group = await AddToGroup(groupName); //save to DB    

      //send to all group members whoe are still in this group
      await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

      //Get message thread
      var messages = await _unitOfWork.MessageRepository.GetMessageThread(caller, other);

      if(_unitOfWork.HasChanges())
        await _unitOfWork.Complete();

      //whoever is connecting will get the message thread
      await Clients.Caller.SendAsync("ReceiveMessageThread", messages);

      //await base.OnConnectedAsync();
      return;
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      // string caller = Context.User.GetUsername();
      // var httpContext = Context.GetHttpContext();
      // //when we make connection to Message Hyb we will pass the user name in the query string
      // var other = httpContext.Request.Query["user"].ToString();                
      // var groupName = GetGroupName(caller, other);

      // await Groups.RemoveFromGroupAsync(Context.ConnectionId,groupName);
      var group = await RemoveFromMessageGroup();        
      // await Groups.RemoveFromGroupAsync(Context.ConnectionId, group.Name);  //performed automatically
      await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);

      await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
      var username = Context.User.GetUsername();
      var recipientUserame = createMessageDto.RecipientUsername.ToLower();

      if (username == recipientUserame)
        throw new HubException("You cannot send messages to yourself");

      var sender = await _unitOfWork.UserRepository.GetUserByUserNameAsync(username);
      var recipient = await _unitOfWork.UserRepository.GetUserByUserNameAsync(recipientUserame);

      if (recipient == null)
        throw new HubException("Not found user");

      var message = new Message()
      {
        Sender = sender,
        Recipient = recipient,
        SenderUsername = sender.UserName,
        RecipientUsername = recipient.UserName,
        Content = createMessageDto.Content
      };

      var groupName = GetGroupName(username, recipientUserame);

      var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);

      if (group.Connections.Any(c => c.Username == recipientUserame))
      {
        message.DateRead = DateTime.UtcNow;
      }
      else
      {
        var connections = await _tracker.GetConnectionsForUser(recipientUserame);
        if(connections != null)
        {
          await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", 
              new {
                  username = sender.UserName,
                  knownAs = sender.KnownAs
                });
        }

      }

      //send a notification to the recipient if he is online but not in the chat room with the users


      _unitOfWork.MessageRepository.AddMessage(message);

      if (await _unitOfWork.Complete())
      {
        var messageDto = _mapper.Map<MessageDto>(message);
        await Clients.Group(groupName).SendAsync("NewMessage", messageDto);
        return;
      }

      throw new HubException("Failed to send message");
    }

    private string GetGroupName(string caller, string other)
    {
      var stringCompare = String.CompareOrdinal(caller, other) < 0;
      return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private async Task<Group> AddToGroup(string groupName)
    {
      var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
      var username = Context.User.GetUsername();
      var connection = new Connection(Context.ConnectionId, username);

      if (group == null)
      {
        group = new Group(groupName);
        _unitOfWork.MessageRepository.AddGroup(group);
      }
      group.Connections.Add(connection);

      if(await _unitOfWork.Complete())
        return group;

      throw new HubException("Failed to join group");
    }

    private async Task<Group> RemoveFromMessageGroup()
    {      
      var group = await _unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
      var connection = group.Connections.FirstOrDefault(c=>c.ConnectionId == Context.ConnectionId);
      //await _unitOfWork.MessageRepository.GetConnection(Context.ConnectionId);

      _unitOfWork.MessageRepository.RemoveConnection(connection);
      
      if(await _unitOfWork.Complete())
        return group;

      throw new HubException("Failed to remove from group");
    }
  }
}