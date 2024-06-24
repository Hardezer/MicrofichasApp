using Microsoft.EntityFrameworkCore;
using Microfichas_App.Models;

namespace Microfichas_App.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Folder> Folders { get; set; }
        public DbSet<Microfichas_App.Models.File> Files { get; set; } // Usar namespace completo aquí
    }
}
