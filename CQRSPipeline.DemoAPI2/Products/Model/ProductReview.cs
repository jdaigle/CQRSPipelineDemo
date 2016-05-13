using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSPipeline.DemoAPI.Products.Model
{
    public class ProductReview
    {
        public ProductReview(AddProductReview command)
        {
            this.ProductId = command.ProductId;
            this.ReviewerName = command.ReviewerName;
            this.EmailAddress = command.EmailAddress;
            this.Rating = command.Rating;
            this.Comments = command.Comments;

            this.ReviewDate = DateTime.UtcNow;
            this.ModifiedDate = DateTime.UtcNow;
        }

        public int ProductReviewID { get; set; }

        public int ProductId { get; set; }

        public string ReviewerName { get; set; }
        public string EmailAddress { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }

        public DateTime ReviewDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
