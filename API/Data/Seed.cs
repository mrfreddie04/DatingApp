using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager
        ) {

            if(await userManager.Users.AnyAsync()) {
                return;
            }

            //read json file into a string variable
            var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
            //parse json into a List of AppUser objects
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
            if( users == null) return;

            var roles = new List<AppRole>(){
                new AppRole(){ Name = "Member"},
                new AppRole(){ Name = "Moderator"},
                new AppRole(){ Name = "Admin"}
            };

            foreach(var role in roles) {
                await roleManager.CreateAsync(role);
            }

            //add users into context
            //await context.Users.AddRangeAsync(users.ToArray<AppUser>());
            foreach(var user in users) 
            {
                user.UserName = user.UserName.ToLower();
                
                // using var hmac = new HMACSHA512();                
                // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Apteka123"));
                // user.PasswordSalt = hmac.Key;         
                await userManager.CreateAsync(user, "Apteka$1");
                await userManager.AddToRoleAsync(user, "Member");
            };                
            
            var admin = new AppUser() {
                UserName = "admin"
            };
            await userManager.CreateAsync(admin, "Apteka$1");
            await userManager.AddToRolesAsync(admin, new []{"Admin","Moderator"});
            //await context.Users.AddRangeAsync(users.ToArray<AppUser>());

            //await context.SaveChangesAsync();
        }
    }
}