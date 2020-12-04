using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using API.Extensions;
using API.Middleware;
using API.SignalR;

namespace API
{
  public class Startup
  {

    private readonly IConfiguration _config;

    public Startup(IConfiguration config)
    {
      _config = config;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddApplicationServices(_config);
      services.AddControllers();
      services.AddCors();
      services.AddIdentityServices(_config);
      services.AddSignalR();

      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
      });
    
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseMiddleware<ExceptionMiddleware>();

      if (env.IsDevelopment())
      {
        //app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
      }

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseCors( x => x.AllowAnyHeader()
        .AllowAnyMethod() //POST,GET,PUT, etc
        .AllowCredentials() //needed for the tokens being sent in the Query String
        .WithOrigins("https://localhost:4200"));

      app.UseAuthentication();

      app.UseAuthorization();

      //for prod deployment - tell API server to use static files
      app.UseDefaultFiles(); //to serve index.html, if it is inside there
      app.UseStaticFiles();

      //routing
      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapHub<PresenceHub>("hubs/presence"); //add SignalR to routing
        endpoints.MapHub<MessageHub>("hubs/message"); //add SignalR to routing
        endpoints.MapFallbackToController("Index","Fallback");
      });
    }
  }
}
