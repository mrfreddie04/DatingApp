using System.Threading.Tasks;
using AutoMapper;
using API.Interfaces;

namespace API.Data
{
  public class UnitOfWork : IUnitOfWork
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private IUserRepository _userRepository;
    public UnitOfWork(DataContext context, IMapper mapper)
    {
      _mapper = mapper;
      _context = context;
      _userRepository = new UserRepository(_context,_mapper);

    }

    public IUserRepository UserRepository {//=> new UserRepository(_context,_mapper);
      get {return _userRepository;}
    }  

    public IMessageRepository MessageRepository => new MessageRepository(_context,_mapper);

    public ILikesRepository LikesRepository => new LikesRepository(_context,_mapper);

    public IPhotoRepository PhotoRepository => new PhotoRepository(_context,_mapper);

    public async Task<bool> Complete()
    {
      return (await _context.SaveChangesAsync()) > 0;
    }

    public bool HasChanges()
    {
      return _context.ChangeTracker.HasChanges();
    }
  }
}