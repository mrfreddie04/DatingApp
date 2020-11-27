using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using API.Extensions;
using API.Interfaces;

namespace API.Helpers
{
  public class LogUserActivity : IAsyncActionFilter
  {
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      //after an action is completed, get user info and update LastActive Date
      //then add this method as a service 
      var resultContext = await next(); //context after action has been executed.

      if(!resultContext.HttpContext.User.Identity.IsAuthenticated)
        return;

      var userId =  resultContext.HttpContext.User.GetUserId();

      var repo = resultContext.HttpContext.RequestServices.GetService<IUserRepository>();

      var user = await repo.GetUserByIdAsync(userId);

      user.LastActive = DateTime.Now;

      await repo.SaveAllAsync();
    }
  }
}