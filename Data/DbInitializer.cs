
using ATS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ATS.API.Data;
public class DbInitializer
{
    public static void DbInit(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        SeedData(scope.ServiceProvider.GetService<AtsDbContext>());
    }

    private static void SeedData(AtsDbContext context)
    {
        context.Database.Migrate();
        if(!context.Users.Any())
        {
            Console.WriteLine("Seeding Users data initiated...");
            var _users = new List<User>
            {
                new() {FirstName ="Mariam", LastName="Kasupe", Email ="mariekasupe@gmail.com", 
                IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy ="system", Role = UserRole.HRAdmin}
            };
            context.AddRange(_users);
            context.SaveChanges();
            Console.WriteLine("Seeding users data completed successfully!");
        }

    }
}