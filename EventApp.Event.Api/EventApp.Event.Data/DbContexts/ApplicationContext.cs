using EventApp.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventApp.Data.DbContexts {

    public class ApplicationContext : DbContext {

        public DbSet<UserEntity> Users { get; set; }
 
        public DbSet<EventEntity> Events { get; set; }

        public DbSet<EventRegistrationEntity> EventsRegistrations { get; set; }

        public DbSet<EventCategoryEntity> EventsCategories { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> contextOptions) : base(contextOptions) {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppContext).Assembly);

            base.OnModelCreating(modelBuilder);

        }



    }

}
