using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.EntityConfiguration;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries").HasKey(x => x.Id);

        builder.Property(c => c.CommonName).IsRequired();
        builder.Property(c => c.OfficialName).IsRequired();
        builder.Property(c => c.Alpha2Code).IsRequired();
        builder.Property(c => c.Alpha3Code).IsRequired();
        builder.Property(c => c.NumericCode).IsRequired(false);
        builder.Property(c => c.IsIndependent).IsRequired();
        builder.Property(c => c.Status).IsRequired();
        builder.Property(c => c.IsUnMember).IsRequired();
        builder.Property(c => c.Region);
        builder.Property(c => c.Subregion).IsRequired(false);
        builder.Property(c => c.Latitude).IsRequired(false);
        builder.Property(c => c.Longitude).IsRequired(false);
        builder.Property(c => c.Area).IsRequired(false);
        builder.Property(c => c.Population).IsRequired(false);
        builder.Property(c => c.FlagEmoji).IsRequired(false);
        builder.Property(c => c.GoogleMaps).IsRequired(false);
        builder.Property(c => c.OpenStreetMap).IsRequired(false);
        builder.Property(c => c.Timezone).IsRequired(false);
        builder.Property(c => c.StartOfWeek).IsRequired(false);

        builder.HasMany(c => c.Currencies)
            .WithOne(cur => cur.Country)
            .HasForeignKey(cur => cur.CountryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Languages)
            .WithOne(l => l.Country)
            .HasForeignKey(l => l.CountryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Translations)
            .WithOne(t => t.Country)
            .HasForeignKey(t => t.CountryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}