using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Models;
using ImmatureBackend.Infrastructure.Persistence.ValueGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImmatureBackend.Infrastructure.Persistence.Configurations;

public class ReplicateEntityConfiguration : IEntityTypeConfiguration<ReplicateEntity>
{
    public void Configure(EntityTypeBuilder<ReplicateEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasValueGenerator<GuidV7ValueGenerator>();

        builder.Property(entity => entity.TechnicianName)
            .HasMaxLength(70);

        builder.Property(entity => entity.CreatedAt)
            .HasDefaultValueSql("now()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(entity => entity.ImmatureWeight)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(entity => entity.Percentage)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(entity => entity.ReviewStatus)
            .HasConversion(status => status.ToString().ToLowerInvariant(),
                s => Enum.Parse<ReviewStatus>(s, true))
            .IsRequired();
    }
}