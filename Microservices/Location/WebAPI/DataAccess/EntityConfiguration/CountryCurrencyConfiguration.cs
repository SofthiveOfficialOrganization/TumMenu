using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.EntityConfiguration;

public class CountryCurrencyConfiguration : IEntityTypeConfiguration<CountryCurrency>
{
    public void Configure(EntityTypeBuilder<CountryCurrency> builder)
    {
        builder.ToTable("CountryCurrencies").HasKey(x => x.Id);

        builder.Property(c => c.Code).IsRequired();
        builder.Property(c => c.Name).IsRequired();
        builder.Property(c => c.Symbol).IsRequired(false);

        builder.HasOne(c => c.Country)
            .WithMany(co => co.Currencies)
            .HasForeignKey(c => c.CountryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}