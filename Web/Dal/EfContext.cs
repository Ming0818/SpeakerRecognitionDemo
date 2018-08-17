using Microsoft.EntityFrameworkCore;
using Web.Models;

namespace Web.Dal
{
    public class EfContext : DbContext
    {
        public DbSet<UserProfile> Profiles { get; set; }

        public EfContext(DbContextOptions<EfContext> options)
            : base(options)
        {

        }
    }
}