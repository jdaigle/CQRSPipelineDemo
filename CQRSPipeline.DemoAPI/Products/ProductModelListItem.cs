using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRSPipeline.DemoAPI.Products
{
    public class ProductModelListItem
    {
        public int ProductModelID { get; set; }
        public string Name { get; set; }

        public Guid? rowguid { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
