using ImmatureBackend.Application.Constants;
using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Models;
using ImmatureBackend.Infrastructure.Persistence.ValueConverters;
using ImmatureBackend.Infrastructure.Persistence.ValueGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImmatureBackend.Infrastructure.Persistence.Configurations;

public class ReplicateEntityConfiguration : IEntityTypeConfiguration<ReplicateEntity>
{
    public const string UniqueSampleIndexName = "ix_replicates_season_index_number_sample_no";

    public void Configure(EntityTypeBuilder<ReplicateEntity> builder)
    {
        builder.ToTable("replicates");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .HasValueGenerator<GuidV7ValueGenerator>();

        builder.Property(entity => entity.TechnicianName)
            .HasColumnName("technician_name")
            .HasMaxLength(100);

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

        builder.Property(entity => entity.ImagePublicId)
            .HasColumnName("image_public_id")
            .IsRequired();

        builder.Property(entity => entity.ImageSecureUrl)
            .HasColumnName("image_secure_url")
            .IsRequired();

        builder.Property(entity => entity.ReviewStatus)
            .HasColumnName("review_status")
            .HasConversion(status => status.ToString().ToLowerInvariant(),
                s => Enum.Parse<ReviewStatus>(s, true))
            .IsRequired();

        builder.Property(entity => entity.Season)
            .HasColumnName("season")
            .HasMaxLength(ReplicateRules.SeasonMaxLength)
            .IsRequired();

        builder.Property(entity => entity.IndexNumber)
            .HasColumnName("index_number")
            .HasMaxLength(ReplicateRules.IndexNumberMaxLength)
            .IsRequired();

        builder.Property(entity => entity.SelectionName)
            .HasColumnName("selection_name")
            .HasMaxLength(ReplicateRules.SelectionNameMaxLength)
            .IsRequired();

        builder.Property(entity => entity.SampleNo)
            .HasColumnName("sample_no")
            .IsRequired();

        builder.Property(entity => entity.CapturedAt)
            .HasColumnName("captured_at")
            .IsRequired();

        builder.Property(entity => entity.SubmittedAt)
            .HasColumnName("submitted_at")
            .IsRequired();

        builder.Property(entity => entity.MeetsRecommendedValue)
            .HasColumnName("meets_recommended_value")
            .IsRequired();

        builder.Property(entity => entity.ProcessingSeconds)
            .HasColumnName("processing_seconds")
            .HasPrecision(10, 1)
            .IsRequired();

        builder.HasIndex(entity => new { entity.Season, entity.IndexNumber, entity.SampleNo })
            .IsUnique()
            .HasDatabaseName(UniqueSampleIndexName);
    }
}