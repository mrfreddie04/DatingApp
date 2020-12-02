using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using API.Extensions;

namespace API.SignalR
{
  [Authorize] //only authorized users will be able to access
  public class PresenceHub : Hub
  {
    private readonly PresenceTracker _tracker;
    public PresenceHub(PresenceTracker tracker)
    {
      _tracker = tracker;

    }

    public override async Task OnConnectedAsync()
    {
        string username = Context.User.GetUsername();

        var isOnline = await _tracker.UserConnected(username,Context.ConnectionId);
        if(isOnline)
          await Clients.Others.SendAsync("UserIsOnline", username);
        
        var currentUsers = await _tracker.GetOnlineUsers();
        await Clients.Caller.SendAsync("GetOnlineUsers",currentUsers);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        string username = Context.User.GetUsername();
        var isOffline = await _tracker.UserDisconnected(username,Context.ConnectionId);        
        if(isOffline)
          await Clients.Others.SendAsync("UserIsOffline", username);

        // var currentUsers = await _tracker.GetOnlineUsers();
        // await Clients.All.SendAsync("GetOnlineUsers",currentUsers);

        await base.OnDisconnectedAsync(exception);
    }
  }
}