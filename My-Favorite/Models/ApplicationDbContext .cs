using Microsoft.EntityFrameworkCore;
using WebApplicationUser.Models.WebApplicationUser.Models;

namespace WebApplicationUser.Models
{ 
public class WebApplicationUserDbContext : DbContext
    {
        public WebApplicationUserDbContext(DbContextOptions<WebApplicationUserDbContext> options) : base(options) { }

        public DbSet<DocumentMeta> DocumentMeta { get; set; }
    }

}

