using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRSPipeline.Framework;

namespace CQRSPipeline.DemoAPI.Products
{
    public class AddProductReview : ICommand<VoidResult>
    {
        public int ProductId { get; set; }

        public string ReviewerName { get; set; }
        public string EmailAddress { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }
    }
}
