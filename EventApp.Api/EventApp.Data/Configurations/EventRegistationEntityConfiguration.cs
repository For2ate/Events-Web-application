using EventApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventApp.Data.Configurations {
    public class EventRegistrationConfiguration : IEntityTypeConfiguration<EventRegistrationEntity> {
        public void Configure(EntityTypeBuilder<EventRegistrationEntity> builder) {
            builder.ToTable("EventRegistrations");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.RegistrationDate)
                .IsRequired();


            builder.Property(r => r.UserId)
                .IsRequired();

            builder.Property(r => r.EventId)
                .IsRequired();

            builder.HasIndex(r => new { r.UserId, r.EventId })
                .IsUnique();

            // Связь: Много регистраций к одному пользователю
            builder.HasOne(r => r.User)
                   .WithMany(u => u.Registrations) 
                   .HasForeignKey(r => r.UserId) 
                   .OnDelete(DeleteBehavior.Cascade);

            // Связь: Много регистраций к одному событию
            builder.HasOne(r => r.Event) 
                   .WithMany(e => e.Registrations) 
                   .HasForeignKey(r => r.EventId) 
                   .OnDelete(DeleteBehavior.Cascade); 
        
        }

    }

}