using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class MainManager
    {
        private MainManager() { }

        private static readonly MainManager _Instance = new MainManager();
        public static MainManager Instance { get { return _Instance;} }
        public static List<Product> Products;
        

        public async Task<List<Product>> ImportProducts()
        {
            string SqlQuery = "Select * From Products";
            Products =  DAL.DAL.GetProducts(SqlQuery);
            await CosmosManager.Instance.GetStartedDemoAsync(Products);
            return Products;
        }

    }
}
