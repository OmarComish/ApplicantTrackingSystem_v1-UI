
using System.Runtime.InteropServices;
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
                new() {Id = 1, FirstName ="Isabel", LastName="Nyirenda", Email ="isabelnyirenda@gmail.com", 
                IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy ="system", Role = UserRole.HRAdmin}
            };
            context.AddRange(_users);
            context.SaveChanges();
            Console.WriteLine("Seeding users data completed successfully!");
        }
        if(!context.Industries.Any())
        {
            Console.WriteLine("Seeding Industries data initiated...");
            var _industries = new List<Industry>
            {
                new() {Id = 1, Name = "Technology",  CreatedAt = DateTime.UtcNow, CreatedBy ="system"},
                new() {Id = 2, Name = "Healthcare",  CreatedAt = DateTime.UtcNow, CreatedBy ="system"},
                new() {Id = 3, Name = "Finance",  CreatedAt = DateTime.UtcNow, CreatedBy ="system"},
                new() {Id = 4, Name = "Construction",  CreatedAt = DateTime.UtcNow, CreatedBy ="system"},
                new() {Id = 5, Name = "Food & Beverage",  CreatedAt = DateTime.UtcNow, CreatedBy ="system"},
                new() {Id = 6, Name = "Agriculture",  CreatedAt = DateTime.UtcNow, CreatedBy ="system"},
            };
            context.AddRange(_industries);
            context.SaveChanges();
            Console.WriteLine("Seeding industries data completed successfully!");
        }

        if(!context.Companies.Any())
        {
            Console.WriteLine("Seeding Companies data initiated...");
            var _companies = new List<Company>
            {
               new() {Id = 1, 
                    Name = "DataStream Analytics",
                    IndustryId = 6, 
                    Description = "Big data and analytics company helping businesses unlock insights from their data.",
                    Location = "Lilongwe",
                    Logo ="DS"
               },
               new () {
                    Id= 2,
                    Name = "BuildRight Construction",
                    IndustryId= 4,
                    Location = "Mangochi, Mw",
                    Description = "Sustainable construction and architecture firm focused on smart building technologies.",
                    Logo = "BR",
                },
                new() {
                    Id= 3,
                    Name= "NovaTech Solutions",
                    IndustryId = 1,
                    Location = "Lilongwe, Mw",
                    Description= "A leading software company specializing in cloud infrastructure and enterprise solutions.",
                    Logo= "NT",
                },
                new() {
                    Id= 4,
                    Name= "GreenLeaf Health",
                    IndustryId= 2,
                    Location= "Kasungu, Mw",
                    Description= "Innovative healthcare technology company transforming patient care through digital solutions.",
                    Logo= "GL",
                },
                new() {
                    Id= 5,
                    Name= "FinEdge Capital",
                    IndustryId= 3,
                    Location= "Lilongwe, Mw",
                    Description= "Modern fintech firm providing next-generation banking and investment platforms.",
                    Logo= "FE",
                }
                
            };
            context.AddRange(_companies);
            context.SaveChanges();
            Console.WriteLine("Seeding companies data completed successfully!");
        }

        if(!context.JobPostings.Any())
        {
            Console.WriteLine("Seeding Jobs data initiated...");
            var _jobs = new List<JobPosting>
            {
               
                new() {Id = 1, 
                Title="Senior Frontend Developer", 
                CompanyId = 1,
                Type =JobType.FullTime,
                Description="Join our engineering team to build next-generation user interfaces for enterprise cloud products. You'll work with React, TypeScript, and modern frontend tooling to deliver exceptional user experiences.",
                Responsibilities= "-Design and implement responsive web applications using React and TypeScript,-Collaborate with UX designers to translate wireframes into polished interfaces,-Write clean, maintainable, and well-tested code,-Mentor junior developers and conduct code reviews, -Participate in architectural decisions and technical planning",
                Requirements ="5+ years of experience with React and modern JavaScript/TypeScript,Strong understanding of CSS, responsive design, and accessibility,Experience with state management solutions (Redux, Context API),Familiarity with CI/CD pipelines and testing frameworks,Excellent communication and collaboration skills",
                SalaryMin =130000,
                SalaryMax =170000,
                CreatedAt = DateTime.UtcNow,
                CreatedBy ="system",
                Department = "ICT",
                Location ="Lilongwe, Mw"            
                },
            };
            context.AddRange(_jobs);
            context.SaveChanges();
            Console.WriteLine("Seeding jobs data completed successfully!");
        }

    }
}