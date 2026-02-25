using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.EntityConfiguration;

public class CountryTranslationConfiguration : IEntityTypeConfiguration<CountryTranslation>
{
    public void Configure(EntityTypeBuilder<CountryTranslation> builder)
    {
        builder.ToTable("CountryTranslations").HasKey(x => x.Id);

        builder.Property(t => t.LanguageCode).IsRequired();
        builder.Property(t => t.OfficialName).IsRequired(false);
        builder.Property(t => t.CommonName).IsRequired(false);
        
        builder.HasOne(t => t.Country)
            .WithMany(c => c.Translations)
            .HasForeignKey(t => t.CountryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}