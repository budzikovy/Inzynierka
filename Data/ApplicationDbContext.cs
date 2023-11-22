using Inz_Fn.Areas.Identity.Data;
using Inz_Fn.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inz_Fn.Data
{
    public class ApplicationDbContext:IdentityDbContext<Inz_FnUser>

    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { 
        }
        public DbSet<Stock> Stock { get; set; }
    }
}
