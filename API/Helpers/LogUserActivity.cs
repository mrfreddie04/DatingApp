using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using API.Interfaces;
using API.Extensions;
using System;

namespace API.Helpers
{
  public class LogUserActivity : IAsyncActionFilter
  {
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        if(!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

        //get user name from Claim Principal
        var userid = resultContext.HttpContext.User.GetUserId();
        //get repo from DI container
        var repo = resultContext.HttpContext.RequestServices.GetService<IUserRepository>();

        var user = await repo.GetUserByIdAsync(userid);
        user.LastActive = DateTime.Now;
        repo.Update(user);
        await repo.SaveAllAsync();
    }
  }
}