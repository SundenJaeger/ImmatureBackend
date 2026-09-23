using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Models;
using ImmatureBackend.Infrastructure.Persistence.ValueConverters;
using ImmatureBackend.Infrastructure.Persistence.ValueGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImmatureBackend.Infrastructure.Persistence.Configurations;

public class ReplicateEntityConfiguration : IEntityTypeConfiguration<ReplicateEntity>
{
    public void Configure(EntityTypeBuilder<ReplicateEntity> builder)
    {
        builder.ToTable("replicates");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .HasValueGenerator<GuidV7ValueGenerator>();

        builder.Property(entity => entity.TechnicianName)
            .HasColumnName("technician_name")
            .HasMaxLength(70);

        builder.Property(entity => entity.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(entity => entity.SampleId)
            .HasColumnName("sample_id");

        builder.Property(entity => entity.AiPredictedGrains)
            .HasColumnName("ai_predicted_grains");

        builder.Property(entity => entity.ConfirmedGrains)
            .HasColumnName("confirmed_grains");

        builder.Property(entity => entity.ImmatureWeight)
            .HasColumnName("immature_weight")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(entity => entity.Percentage)
            .HasColumnName("percentage")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(entity => entity.Grade)
            .HasColumnName("grade")
            .HasConversion<GradeConverter>()
            .IsRequired();

        builder.Property(entity => entity.OriginalImage)
            .HasColumnName("original_image");

        builder.Property(entity => entity.ReviewStatus)
            .HasColumnName("review_status")
            .HasConversion(status => status.ToString().ToLowerInvariant(),
                s => Enum.Parse<ReviewStatus>(s, true))
            .IsRequired();
    }
}