using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;
using System;
using System.Linq;

namespace API.Helpers
{
  public class AutoMapperProfiles : Profile
  {
    public AutoMapperProfiles()
    {
        CreateMap<AppUser,MemberDto>()
            .ForMember(dest => dest.PhotoUrl, 
                opt => opt.MapFrom(src=>src.Photos.FirstOrDefault(p=>p.IsMain==true).Url))
            .ForMember(dest => dest.Age,
                opt => opt.MapFrom(src=>src.DateOfBirth.CalculateAge()));
        
        CreateMap<Photo,PhotoDto>();

        CreateMap<MemberUpdateDto,AppUser>();

        CreateMap<RegisterDto,AppUser>();

        CreateMap<Message,MessageDto>()
          .ForMember( dest => dest.RecipientPhotoUrl,
            opt => opt.MapFrom(src=>src.Recipient.Photos.FirstOrDefault(p=>p.IsMain==true).Url))
          .ForMember( dest => dest.SenderPhotoUrl,
            opt => opt.MapFrom(src=>src.Sender.Photos.FirstOrDefault(p=>p.IsMain==true).Url));

        CreateMap<Photo,PhotoForApprovalDto>()
          .ForMember(dest => dest.Username,
                opt => opt.MapFrom(src=>src.AppUser.UserName));

        //done in DataContext.OnModelCreating() method
        //CreateMap<DateTime,DateTime>().ConvertUsing(d => (DateTime)DateTime.SpecifyKind(d, DateTimeKind.Utc));  

        // CreateMap<AppUser,LikeDto>()
        //     .ForMember(dest => dest.PhotoUrl, 
        //         opt => opt.MapFrom(src=>src.Photos.FirstOrDefault(p=>p.IsMain==true).Url))
        //     .ForMember(dest => dest.Age,
        //         opt => opt.MapFrom(src=>src.DateOfBirth.CalculateAge()));
        // CreateMap<MemberDto,AppUser>();
        // CreateMap<PhotoDto,Photo>();
    }
  }
}