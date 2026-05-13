using Microsoft.EntityFrameworkCore;
using BanHang.Models;

namespace BanHang.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Account> Users { get; set; }
    }
}