using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSPipeline.DemoAPI.Products.Model
{
    public class ProductModel
    {
        public int ProductModelID { get; set; }
        public string Name { get; set; }

        public Guid? rowguid { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string OtherField { get; set; }
    }
}
