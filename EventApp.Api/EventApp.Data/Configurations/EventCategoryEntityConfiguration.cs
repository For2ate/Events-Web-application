using EventApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventApp.Data.Configurations {

    public class EventCategoryConfiguration : IEntityTypeConfiguration<EventCategoryEntity> {

        public void Configure(EntityTypeBuilder<EventCategoryEntity> builder) {

            builder.ToTable("EventCategories");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(c => c.Name)
                .IsUnique();

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            // Связь: Одна категория имеет много событий
            builder.HasMany(c => c.Events)
                   .WithOne(e => e.Category)
                   .HasForeignKey(e => e.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

        }
  
    }

}