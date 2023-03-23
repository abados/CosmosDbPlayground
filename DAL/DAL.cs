using System.Configuration;
using Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;



namespace DAL
{

    public static class DAL
    {
        // Delegate - Function pointer
        public delegate object SetDataReader_delegate(SqlDataReader reader);
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndpointUri"];     // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];
        // Connction string
        private static readonly string ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
        // Getting data from Sql
        public static object GetDataFromDB(string SqlQuery, SetDataReader_delegate func)
        {
            try
            {
                object Data = null;
                using (SqlConnection connection = new SqlConnection(ConnectionString)) 
                {

                    // Adapter
                    using (SqlCommand command = new SqlCommand(SqlQuery, connection))
                    {
                        connection.Open();
                        //Reader
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            Data = func(reader);
                        }

                    }
                }
                return Data;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // Getting an array of Newest Articles (1 per category)
        public static List<Product> GetProducts(string SqlQuery)
        {
            try
            {
                List <Product> Products = new List<Product>();
                return (List<Product>)GetDataFromDB(SqlQuery, _GetProducts);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // Delegate function - Getting newest articles
        private static List<Product> _GetProducts(SqlDataReader reader)
        {
            try
            {
                List<Product> Products = new List<Product>();
                while (reader.Read())
                {
                    Product Product = new Product
                    {
                        ProductID = reader.GetInt32(0).ToString(),
                        ProductName= reader.GetString(1),
                        SupplierID = reader.GetInt32(2),
                        CategoryID= reader.GetInt32(3),
                        QuantityPerUnit= reader.GetString(4),
                        UnitPrice= reader.GetDecimal(5),
                        UnitsInStock= reader.GetInt16(6),
                        UnitsOnOrder= reader.GetInt16(7),
                        ReorderLevel= reader.GetInt16(8),
                        Discontinued= reader.GetBoolean(9),                  
                    };

                    Products.Add(Product);

                }
                return Products;

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
