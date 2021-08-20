using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using API.Extensions;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub: Hub    
    {
        private readonly PresenceTracker _tracker;

        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var currentUser = Context.User.GetUserName();
            var isOnline = await _tracker.UserConnected(currentUser, Context.ConnectionId);            
            
            if(isOnline) //first login for this user, logging in to additional devices won't raise this flag
                await Clients.Others.SendAsync("UserIsOnline",currentUser);

            var currentUsers = await _tracker.GetConnectedUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);  
            //await Clients.All.SendAsync("GetOnlineUsers", currentUsers);          
        }

        #nullable enable    
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var currentUser = Context.User.GetUserName();
            var isOffline = await _tracker.UserDisconnected(currentUser, Context.ConnectionId);

            if(isOffline) //logout from the last device, user is completely offline now
                await Clients.Others.SendAsync("UserIsOffline", currentUser);

            //var currentUsers = await _tracker.GetConnectedUsers();
            //await Clients.All.SendAsync("GetOnlineUsers", currentUsers);

            await base.OnDisconnectedAsync(exception);
        }      
        #nullable disable
    }    
}