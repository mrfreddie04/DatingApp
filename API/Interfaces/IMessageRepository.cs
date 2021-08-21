using System.Collections.Generic;
using System.Threading.Tasks;
using API.Entities;
using API.Helpers;
using API.DTOs;

namespace API.Interfaces
{
    public interface IMessageRepository
    {
        void AddGroup(Group group);
        void RemoveConnection(Connection connection);
        Task<Connection> GetConnectionAsync(string connectionId);
        Task<Group> GetMessageGroupAsync(string groupName);
        Task<Group> GetGroupForConnectionAsync(string connectionId);

        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessageAsync(int id);  
        Task<PagedList<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams);
        Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUserName, string recipientName);
    }
}