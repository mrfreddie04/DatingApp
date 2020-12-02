using System;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {        
        public static IServiceCollection AddIdentityServices(this IServiceCollection services,
            IConfiguration config)
        {
            //configure identity
            services.AddIdentityCore<AppUser>(opt=>{
                opt.Password.RequireNonAlphanumeric = false; //example - turn off complex pws policy
            })
            .AddRoles<AppRole>()
            .AddRoleManager<RoleManager<AppRole>>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddRoleValidator<RoleValidator<AppRole>>()
            .AddEntityFrameworkStores<DataContext>(); //to add identity table to the db

            //add authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => 
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                        ValidateIssuer = false, //API Serve
                        ValidateAudience = false //Angular App
                    };

                    options.Events = new JwtBearerEvents()
                    {
                        OnMessageReceived = context => 
                        {
                            //SignalR by default will send up a token with the key of access_token
                            var accessToken = context.Request.Query["access_token"];
                            //Check the path of this request
                            var path = context.HttpContext.Request.Path;

                            if(!String.IsNullOrEmpty(accessToken) || path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;    
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            //add authorization
            services.AddAuthorization(opt=>{
                opt.AddPolicy("RequireAdminRole", policy=>policy.RequireRole("Admin"));
                opt.AddPolicy("ModeratePhotoRole", policy=>policy.RequireRole("Admin","Moderator"));
            });

            return services;
        }
    }
}