using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSPipeline.DemoAPI.Products.Model
{
    public static class ProductsCommandHandlers
    {
        [CommandHandler]
        public static int Handle(AddProductModel command, DbContext dbContext)
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
        }

        [CommandHandler]
        public static void Handle(SetProductModelName command, CommandContext commandContext)
        {
            var product = commandContext.DbContext.Set<ProductModel>().Find(1);
            product.Name = "Classic Vest " + DateTime.UtcNow;
        }

        [CommandHandler]
        public static void Handle(CommandContext commandContext, AddProductReview command)
        {
            var productReview = new ProductReview(command);
            commandContext.DbContext.Set<ProductReview>().Add(productReview);
        }
    }
}
