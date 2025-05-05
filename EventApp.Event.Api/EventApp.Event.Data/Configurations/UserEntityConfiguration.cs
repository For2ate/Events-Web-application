using EventApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventApp.Data.Configurations {

    public class UserConfiguration : IEntityTypeConfiguration<UserEntity> {

        public void Configure(EntityTypeBuilder<UserEntity> builder) {
 
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.FirstName)
                .IsRequired() 
                .HasMaxLength(100); 

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.Password)
                .IsRequired();

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.BirthdayDate)
                .IsRequired();

        }

    }

}