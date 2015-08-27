using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSPipeline.DemoAPI.Products.Model
{
    public static class ProductsQueryHandler
    {
        [QueryHandler]
        public static List<ProductModelListItem> Handle(ListProductModels query, QueryContext queryContext)
        {
            return queryContext.SqlQuery<ProductModelListItem>("SELECT * FROM Production.ProductModel", param: null).ToList();
        }

        [QueryHandler]
        public static List<ProductReviewListItem> Handle(ListProductReviews query, QueryContext queryContext)
        {
            return queryContext.SqlQuery<ProductReviewListItem>("SELECT * FROM Production.[ProductReview] WHERE ProductId = @ProductId", param: query).ToList();
        }
    }
}
