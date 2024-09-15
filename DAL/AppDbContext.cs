using BOL.DataModel;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options) 
        {

        }


        public DbSet<ApiConfiguration> ApiConfigurations { get; set; }
        public DbSet<ApiField> ApiFields { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApiConfiguration>()
                .HasMany(x => x.ApiFields)
                .WithOne(x => x.ApiConfiguration)
                .HasForeignKey(x => x.ApiConfigurationId)
                .IsRequired();
        }
    }
}
