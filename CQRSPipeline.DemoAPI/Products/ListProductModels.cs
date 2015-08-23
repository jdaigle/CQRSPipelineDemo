using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSPipeline.DemoAPI.Products
{
    public class ListProductModels : IAPIQuery<List<ProductModelListItem>>
    {
    }

    public class ListProductReviews : IAPIQuery<List<ProductReviewListItem>>
    {
        public int ProductId { get; set; }
    }
}
