using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.EntityConfiguration;

public class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> builder)
    {
        builder.ToTable("Provinces").HasKey(x => x.Id);
        builder.Property(wp => wp.Name).IsRequired();
        builder.Property(wp => wp.Latitude).IsRequired();
        builder.Property(wp => wp.Longitude).IsRequired();
        builder.Property(wp => wp.GoogleMaps).IsRequired();
        
        builder.HasMany(d => d.Districts)
            .WithOne(w => w.Province)
            .HasForeignKey(d => d.ProvinceId) 
            .OnDelete(DeleteBehavior.Cascade);
    }
}