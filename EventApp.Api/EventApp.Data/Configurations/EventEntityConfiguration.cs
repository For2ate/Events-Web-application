using EventApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventApp.Data.Configurations {

    public class EventConfiguration : IEntityTypeConfiguration<EventEntity> {

        public void Configure(EntityTypeBuilder<EventEntity> builder) {

            builder.ToTable("Events");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(e => e.Name);

            builder.Property(e => e.Description) 
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(e => e.DateOfEvent)
                .IsRequired();

            builder.HasIndex(e => e.DateOfEvent);

            builder.Property(e => e.Place)
                .IsRequired();

            builder.HasIndex(e => e.Place);

            builder.Property(e => e.CurrentNumberOfParticipants);

            builder.Property(e => e.MaxNumberOfParticipants)
                .IsRequired(); 

            builder.Property(e => e.ImageUrl)
                .IsRequired() 
                .HasMaxLength(2048); 

            builder.Property(e => e.CategoryId)
                .IsRequired();

        }

    }

}