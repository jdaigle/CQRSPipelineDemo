using System;
using System.Data.Entity;
using CQRSPipeline.DemoAPI.Products.Model;
using CQRSPipeline.Framework;

namespace CQRSPipeline.DemoAPI.Products
{
    public class ProductsModule : CommandModule
    {
        public ProductsModule(DbContext dbContext)
        {
            Handle<AddProductModel, int>(command =>
            {
                var product = dbContext.Set<ProductModel>().Add(new ProductModel()
                {
                    ProductModelID = 123,
                    Name = "New Product Name - " + DateTime.Now.Ticks,
                    rowguid = Guid.NewGuid(),
                    ModifiedDate = DateTime.UtcNow,
                });
                dbContext.SaveChanges(); // to get the new ProductModelId
                return product.ProductModelID;
            });

            Handle<SetProductModelName>(command =>
            {
                var product = dbContext.Set<ProductModel>().Find(1);
                product.Name = "Classic Vest " + DateTime.UtcNow;
            });

            Handle<AddProductReview>(command =>
            {
                var productReview = new ProductReview(command);
                dbContext.Set<ProductReview>().Add(productReview);
            });
        }
    }
}
