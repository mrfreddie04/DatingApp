using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using AutoMapper;
using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, 
            IConfiguration config)
        {
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });
            
            //strongly type our config settings (strings=>class)            
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));

            //AddScoped() - scoped to context of the request
            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped<IPhotoService,PhotoService>();

            services.AddScoped<IUserRepository,UserRepository>();

            services.AddScoped<ILikesRepository,LikesRepository>();

            services.AddScoped<IMessageRepository,MessageRepository>();

            services.AddScoped<LogUserActivity>();
                    
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

            return services;
        }
    }
}