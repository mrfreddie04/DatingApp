using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class PresenceTracker
    {
        //key - username
        //value - list of connection ids for this user (may be connected from multiple devices at the same time)
        //this in-memory dictionary (being static) will be shared among different users, but Dictionary is not a thread safe reource
        private static readonly Dictionary<string,List<string>> OnlineUsers = new Dictionary<string,List<string>>();

        public Task<bool> UserConnected(string username, string connectionId) 
        {
            bool isOnline = false;
            lock(OnlineUsers)
            {
                if(OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                }
                else 
                {                    
                    OnlineUsers.Add(username, new List<string>(){connectionId});
                    isOnline = true; //opened the first connection
                }
            }
            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(string username, string connectionId) 
        {
            bool isOffline = false;
            lock(OnlineUsers)
            {
                if(OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Remove(connectionId);
                    if(OnlineUsers[username].Count==0)
                    {                        
                        OnlineUsers.Remove(username);    
                        isOffline = true; //closed the lasr connection
                    }    
                }
            }
            return Task.FromResult(isOffline);            
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers;
            lock(OnlineUsers)                
            {
                onlineUsers = OnlineUsers.OrderBy(d=>d.Key).Select(d=>d.Key).ToArray<string>();            
            }
            return Task.FromResult(onlineUsers);
        }

        public Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;
            lock(OnlineUsers)
            {   
                connectionIds = OnlineUsers.GetValueOrDefault(username);    
                //if(OnlineUsers.ContainsKey(username)) connectionIds = OnlineUsers[username];       
            }
            return Task.FromResult(connectionIds);
        }
    }
}