using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.EntityConfiguration;

public class CountryLanguageConfiguration : IEntityTypeConfiguration<CountryLanguage>
{
    public void Configure(EntityTypeBuilder<CountryLanguage> builder)
    {
        builder.ToTable("CountryLanguages").HasKey(x => x.Id);

        builder.Property(l => l.Code).IsRequired();
        builder.Property(l => l.Name).IsRequired();

        builder.HasOne(l => l.Country)
            .WithMany(c => c.Languages)
            .HasForeignKey(l => l.CountryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}