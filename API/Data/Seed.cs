using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(DataContext context) {

            if(await context.Users.AnyAsync()) {
                return;
            }

            //read json file into a string variable
            var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");

            //parse json into a List of AppUser objects
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            //add users into context
            //await context.Users.AddRangeAsync(users.ToArray<AppUser>());
            foreach(var user in users) 
            {
                user.UserName = user.UserName.ToLower();
                
                using var hmac = new HMACSHA512();                
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Apteka123"));
                user.PasswordSalt = hmac.Key;         
                context.Users.Add(user);
            };                
            
            //await context.Users.AddRangeAsync(users.ToArray<AppUser>());

            await context.SaveChangesAsync();
        }
    }
}