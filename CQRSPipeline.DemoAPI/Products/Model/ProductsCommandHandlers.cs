using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSPipeline.DemoAPI.Products.Model
{
    public static class ProductsCommandHandlers
    {
        public static int Handle(AddProductModel command, CommandContext commandContext)
        {
            var product = commandContext.DbContext.Set<ProductModel>().Add(new ProductModel()
            {
                ProductModelID = 123,
                Name = "New Product Name - " + DateTime.Now.Ticks,
                rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow,
            });
            commandContext.DbContext.SaveChanges(); // to get the new ProductModelId
            return product.ProductModelID;
        }

        public static void Handle(SetProductModelName command, CommandContext commandContext)
        {
            var product = commandContext.DbContext.Set<ProductModel>().Find(1);
            product.Name = "Classic Vest " + DateTime.UtcNow;
        }
    }
}
