using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAPI.Models.Concrete;

namespace WebAPI.DataAccess.EntityConfiguration;

public class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.ToTable("Districts").HasKey(x => x.Id);
        builder.Property(wp => wp.ProvinceId).IsRequired();
        builder.Property(wp => wp.Name).IsRequired();
        builder.Property(wp => wp.Population).IsRequired();
        builder.Property(wp => wp.Area).IsRequired();
        
        builder.HasOne(d => d.Province)
            .WithMany(w => w.Districts)
            .HasForeignKey(d => d.ProvinceId) 
            .OnDelete(DeleteBehavior.Cascade);
    }
}