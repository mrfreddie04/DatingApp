using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json; //MS version of JSON
using API.Entities;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(DataContext context)
        {
            if(await context.Users.AnyAsync())
                return;

            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            // using FileStream fs = File.OpenRead("Data/UserSeedData.json");            
            // var users = await JsonSerializer.DeserializeAsync<List<AppUser>>(fs);
            
            foreach(var user in users)
            {
                //context.Users.Add(user);
                using var hmac= new HMACSHA512(); //will dispose when var goes our os scope

                user.UserName = user.UserName.ToLower();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
                user.PasswordSalt = hmac.Key; //it is randomly generated when hmac calss is instantiated

                context.Users.Add(user);
            }

            //context.Users.AddRange(users);

            await context.SaveChangesAsync();           
        }
    }
}