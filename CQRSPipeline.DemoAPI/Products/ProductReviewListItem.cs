using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRSPipeline.DemoAPI.Products
{
    public class ProductReviewListItem
    {
        public int ProductReviewId { get; set; }

        public int ProductId { get; set; }

        public string ReviewerName { get; set; }
        public string EmailAddress { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }

        public DateTime ReviewDate { get; set; }
    }
}
