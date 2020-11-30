using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json; //MS version of JSON
using API.Entities;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            if(await userManager.Users.AnyAsync())
                return;
            
            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
            if(users == null) return;

            // using FileStream fs = File.OpenRead("Data/UserSeedData.json");            
            // var users = await JsonSerializer.DeserializeAsync<List<AppUser>>(fs);
            
            var roles = new List<AppRole>(){
                new AppRole(){ Name="Member" },
                new AppRole(){ Name="Admin" },
                new AppRole(){ Name="Moderator" }
            };

            foreach(var role in roles)
            {
                await roleManager.CreateAsync(role);           
            }    

            foreach(var user in users)
            {
                //context.Users.Add(user);
                //using var hmac= new HMACSHA512(); //will dispose when var goes our os scope

                user.UserName = user.UserName.ToLower();
                //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
                //user.PasswordSalt = hmac.Key; //it is randomly generated when hmac calss is instantiated

                await userManager.CreateAsync(user,"Pa$$w0rd");
                await userManager.AddToRoleAsync(user,"Member");
            }

            var admin = new AppUser(){
                UserName = "admin"
            };
            await userManager.CreateAsync(admin,"Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, new string[]{"Admin","Moderator"});            
            
            //context.Users.AddRange(users);
            //await context.SaveChangesAsync();           
        }
    }
}