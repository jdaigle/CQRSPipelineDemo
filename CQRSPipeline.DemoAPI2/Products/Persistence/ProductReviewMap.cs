using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRSPipeline.DemoAPI.Products.Model;

namespace CQRSPipeline.DemoAPI.Products.Persistence
{
    public class ProductReviewMap : EntityTypeConfiguration<ProductReview>
    {
        public ProductReviewMap()
        {
            this.ToTable("ProductReview", "Production");

            // Primary Key
            this.HasKey(t => t.ProductReviewID);
            this.Property(t => t.ProductReviewID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            // Properties
            this.Property(t => t.ReviewerName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(true);

            this.Property(t => t.EmailAddress)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(true);

            this.Property(t => t.Comments)
                .IsOptional()
                .HasMaxLength(3850)
                .IsUnicode(true);

            this.Property(t => t.Rating)
                .IsRequired();

            this.Property(t => t.ReviewDate)
                .IsRequired()
                .HasColumnType("datetime");

            this.Property(t => t.ModifiedDate)
                .IsRequired()
                .HasColumnType("datetime");
        }
    }
}
