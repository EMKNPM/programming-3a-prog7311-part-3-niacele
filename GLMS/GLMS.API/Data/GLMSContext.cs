using GLMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Data
{
    public class GLMSContext : DbContext
    {
        public GLMSContext(DbContextOptions<GLMSContext> options) : base(options)
        {
        }

        //tables that'll be created in the Db
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // seed the database with staff info - no sign up 
            modelBuilder.Entity<User>().HasData(
                new User { UserID = 1, Username = "admin@techmove.com", Password = "Admin123!", FullName = "System Administrator", Role = "Admin" },
                new User { UserID = 2, Username = "operator1", Password = "Password123", FullName = "Logistics Dispatcher", Role = "Operator" }
            );
        }
    }
    }

