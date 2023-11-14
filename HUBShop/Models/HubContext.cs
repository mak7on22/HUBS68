using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HUBShop.Models.Users;
using HUBShop.Models.Task;

namespace HUBShop.Models
{
    public class HubContext : IdentityDbContext<User,IdentityRole<int>, int>
    {
        public DbSet<User> Users {  get; set; }
        public DbSet<Goal> Goals { get; set; }
        public HubContext(DbContextOptions<HubContext> options)
            : base(options)
        {
        }
    }
}
