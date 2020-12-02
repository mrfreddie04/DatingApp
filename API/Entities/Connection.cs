namespace API.Entities
{
  public class Connection
  {
    public Connection()
    {
        //EF requires a default ctor
    }
    
    public Connection(string connectionId, string userName)
    {
      ConnectionId = connectionId;
      Username = userName;
    }
    
    public string ConnectionId { get; set; }
    public string Username { get; set; }
  }
}