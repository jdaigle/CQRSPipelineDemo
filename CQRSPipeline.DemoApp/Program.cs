using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CQRSPipeline.DemoAPI;
using CQRSPipeline.DemoAPI.Products;

namespace CQRSPipeline.DemoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // always need to call Init() on startup
            Client.Init();

            // create a new new client
            var client = Client.New();

            // command with return value
            var id = client.Execute(new AddProductModel());
            
            // comand with no return value
            client.Execute(new SetProductModelName());

            // standalone query
            var productModels = client.Query(new ListProductModels());

            return;
        }
    }
}
