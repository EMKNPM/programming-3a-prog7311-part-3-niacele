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
        }
    }

