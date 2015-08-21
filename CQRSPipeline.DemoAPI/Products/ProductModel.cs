using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSPipeline.DemoAPI.Products
{
    public class ProductModel
    {
        public int ProductModelID { get; set; }
        public string Name { get; set; }

        public Guid? rowguid { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string OtherField { get; set; }
    }

    public class ProductModelMap : EntityTypeConfiguration<ProductModel>
    {
        public ProductModelMap()
        {
            this.ToTable("ProductModel", "Production");

            // Primary Key
            this.HasKey(t => t.ProductModelID);
            this.Property(t => t.ProductModelID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(true);

            this.Property(t => t.rowguid)
                .IsOptional();

            this.Property(t => t.ModifiedDate)
                .IsOptional()
                .HasColumnType("datetime2");

            this.Ignore(t => t.OtherField);
        }
    }
}
