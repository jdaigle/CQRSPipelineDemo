using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CQRSPipeline.DemoAPI;
using CQRSPipeline.DemoAPI.Products;
using CQRSPipeline.Framework;

namespace CQRSPipeline.DemoApp
{
    public static class Program
    {
        public static void Main()
        {
            var processor = new CommandProcessor(DemoAPICommandProcessorConfiguration.Instance);

            // command with return value - 1 transaction per command
            int id = processor.Execute(new AddProductModel()
            {
                // ... properties ...
            });

            // command with no return value - 1 transaction per command
            processor.Execute(new SetProductModelName()
            {
                // ... properties ...
            });

            processor.Execute(new AddProductReview()
            {
                ProductId = id,

                ReviewerName = "Joseph Daigle",
                EmailAddress = "joseph.daigle@gmail.com",
                Rating = 4,
                Comments = "Meh",
            });
        }

        //static void Main(string[] args)
        //{
        //    // always need to call Init() on startup
        //    Client.Init();

        //    // create a new client
        //    var client = Client.New();

        //    // command with return value - 1 transaction per command
        //    int id = client.Execute(new AddProductModel()
        //    {
        //        // ... properties ...
        //    });

        //    // command with no return value - 1 transaction per command
        //    client.Execute(new SetProductModelName()
        //    {
        //        // ... properties ...
        //    });

        //    // standalone query - single transaction
        //    List<ProductModelListItem> productModels = client.Query(new ListProductModels()
        //    {
        //        // ... criteria ...
        //    });

        //    client.Execute(new AddProductReview()
        //    {
        //        ProductId = 798,

        //        ReviewerName = "Joseph Daigle",
        //        EmailAddress = "joseph.daigle@gmail.com",
        //        Rating = 4,
        //        Comments = "Meh",
        //    });

        //    using (var queryScope = client.QueryScope())
        //    {
        //        // queries in the same scope (i.e. database transaction)
        //        // transaction is kept open until disposed so don't do much work!
        //        // make sure you dispose otherwise the transaction might get stuck!

        //        productModels = client.Query(new ListProductModels());

        //        List<ProductReviewListItem> productReviews = client.Query(new ListProductReviews()
        //        {
        //            ProductId = 798,
        //        });

        //        // transaction in scope is automatically committed (or rolled back if an exception is thrown) when disposed
        //    }

        //    return;
        //}
    }
}
