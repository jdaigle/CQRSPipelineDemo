using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace CQRSPipeline.DemoAPI.Products
{
    public static class ProductsQueryHandler
    {
        public static List<ProductModelListItem> Handle(ListProductModels query, QueryContext queryContext)
        {
            return queryContext.CurrentConnection.Query<ProductModelListItem>("SELECT * FROM Production.ProductModel", param: null, transaction: queryContext.CurrentTransaction).ToList();
        }
    }
}
