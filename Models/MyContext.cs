using Microsoft.EntityFrameworkCore;

namespace MindYourInjections.Models
{ 
    public class MyContext : DbContext 
    { 
        public MyContext(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Language> Languages {get;set;}
        public DbSet<Objective> Objectives {get;set;}
        public DbSet<Step> Steps {get;set;}
    }
}