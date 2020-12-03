using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using System.Linq;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class PhotoRepository : IPhotoRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    public PhotoRepository(DataContext context, IMapper mapper)
    {
      _mapper = mapper;
      _context = context;
    }

    public async Task<Photo> GetPhotoById(int photoId)
    {
      return await _context.Photos
          .IgnoreQueryFilters()
          .SingleOrDefaultAsync(p => p.Id == photoId);
    }

    public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
    {
      return await _context.Photos
          .IgnoreQueryFilters()
          .Where(p => p.isApproved == false)
          .ProjectTo<PhotoForApprovalDto>(_mapper.ConfigurationProvider)
          .ToListAsync();
    }

    public void RemovePhoto(Photo photo)
    {
      _context.Photos.Remove(photo);
    }
  }
}