using Microsoft.Azure.Cosmos;
using Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace Entities
{
    public class CosmosManager
    {
        private CosmosManager() { }
        public static CosmosManager Instance { get { return _Instance; } }
        private static readonly CosmosManager _Instance = new CosmosManager();

        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];

        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Microsoft.Azure.Cosmos.Container container;

        // The name of the database and container we will create
        private string databaseId = "Northwind";
        private string containerId = "Products";

        // </Main>

        // <GetStartedDemoAsync>
        /// <summary>
        /// Entry point to call methods that operate on Azure Cosmos DB resources in this sample
        /// </summary>
        public async Task GetStartedDemoAsync(List<Product> Products)
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
           // await this.ScaleContainerAsync();
            await this.AddItemsToContainerAsync(Products);
            //await this.QueryItemsAsync();
            //await this.DeleteProductItemAsync();
            //await this.DeleteDatabaseAndCleanupAsync();
        }
        // </GetStartedDemoAsync>

        // <CreateDatabaseAsync>
        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        private async Task CreateDatabaseAsync()
        {
            // Create a new database
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }
        // </CreateDatabaseAsync>

        // <CreateContainerAsync>
        /// <summary>
        /// Create the container if it does not exist. 
        /// Specifiy "/LastName" as the partition key since we're storing Product information, to ensure good distribution of requests and storage.
        /// </summary>
        /// <returns></returns>
        private async Task CreateContainerAsync()
        {
            // Create a new container
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/ProductName", 400);
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }
        // </CreateContainerAsync>

        // <ScaleContainerAsync>
        /// <summary>
        /// Scale the throughput provisioned on an existing Container.
        /// You can scale the throughput (RU/s) of your container up and down to meet the needs of the workload. Learn more: https://aka.ms/cosmos-request-units
        /// </summary>
        /// <returns></returns>
        private async Task ScaleContainerAsync()
        {
            // Read the current throughput
            int? throughput = await this.container.ReadThroughputAsync();
            if (throughput.HasValue)
            {
                Console.WriteLine("Current provisioned throughput : {0}\n", throughput.Value);
                int newThroughput = throughput.Value + 100;
                // Update throughput
                await this.container.ReplaceThroughputAsync(newThroughput);
                Console.WriteLine("New provisioned throughput : {0}\n", newThroughput);
            }

        }
        // </ScaleContainerAsync>

        // <AddItemsToContainerAsync>
        /// <summary>
        /// Add Product items to the container
        /// </summary>
        private async Task AddItemsToContainerAsync(List<Product> Products)
        {
            foreach (var Product in Products)
            {
       /*         // Create a Product object
                Product product = new Product
                {
                    ProductID = Product.ProductID,
                    ProductName = Product.ProductName,
                    SupplierID = Product.SupplierID,
                    CategoryID = Product.CategoryID,
                    QuantityPerUnit = Product.QuantityPerUnit,
                    UnitPrice = Product.UnitPrice,
                    UnitsInStock = Product.UnitsInStock,
                    ReorderLevel = Product.ReorderLevel,
                    Discontinued = Product.Discontinued,
                };*/

                try
                {
                    // Read the item to see if it exists.  
                    ItemResponse<Product> productResponse = await this.container.ReadItemAsync<Product>(Product.ProductID.ToString(), new PartitionKey(Product.ProductName));
                    Console.WriteLine("Item in database with id: {0} already exists\n", productResponse.Resource.ProductID);
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    // Create an item in the container representing the Andersen Product. Note we provide the value of the partition key for this item, which is "Andersen"
                    ItemResponse<Product> productResponse = await this.container.CreateItemAsync<Product>(Product, new PartitionKey(Product.ProductName));

                    // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                    Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", productResponse.Resource.ProductID, productResponse.RequestCharge);
                }
            }   
        }
        // </AddItemsToContainerAsync>

        // <QueryItemsAsync>
        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// Including the partition key value of lastName in the WHERE filter results in a more efficient query
        /// </summary>
        private async Task<List<Product>> QueryItemsAsync(string SqlQuery)
        {
            if (container == null)
            {
                await DBConnect();

			}
				Console.WriteLine("Running query: {0}\n", SqlQuery);

				QueryDefinition queryDefinition = new QueryDefinition(SqlQuery);

				FeedIterator<Product> queryResultSetIterator = this.container.GetItemQueryIterator<Product>(queryDefinition);

				List<Product> Products = new List<Product>();

				while (queryResultSetIterator.HasMoreResults)
				{
					FeedResponse<Product> currentResultSet = await queryResultSetIterator.ReadNextAsync();
					foreach (Product Product in currentResultSet)
					{
						Products.Add(Product);
						Console.WriteLine("\tRead {0}\n", Product);
					}
				}
				return Products;
			

		}
        // </QueryItemsAsync>


        // <DeleteProductItemAsync>
        /// <summary>
        /// Delete an item in the container
        /// </summary>
        private async Task DeleteProductItemAsync()
        {
            var partitionKeyValue = "Wakefield";
            var ProductId = "Wakefield.7";

            // Delete an item. Note we must provide the partition key value and id of the item to delete
            ItemResponse<Product> wakefieldProductResponse = await this.container.DeleteItemAsync<Product>(ProductId, new PartitionKey(partitionKeyValue));
            Console.WriteLine("Deleted Product [{0},{1}]\n", partitionKeyValue, ProductId);
        }
        // </DeleteProductItemAsync>

        // <DeleteDatabaseAndCleanupAsync>
        /// <summary>
        /// Delete the database and dispose of the Cosmos Client instance
        /// </summary>
        private async Task DeleteDatabaseAndCleanupAsync()
        {
            DatabaseResponse databaseResourceResponse = await this.database.DeleteAsync();
            // Also valid: await this.cosmosClient.Databases["ProductDatabase"].DeleteAsync();

            Console.WriteLine("Deleted Database: {0}\n", this.databaseId);

            //Dispose of CosmosClient
            this.cosmosClient.Dispose();
        }
        // </DeleteDatabaseAndCleanupAsync>

        public async Task<List<Product>> GetProductsCheaperThen(int price)
        {
            string SqlQuery = "SELECT* FROM c WHERE c.UnitPrice < " + price + "";
            return  await QueryItemsAsync(SqlQuery);
        }

		public async Task<List<Product>> GetProductsBySupplierID(string sid)
		{
			string SqlQuery = "SELECT * from c WHERE c.SupplierID = " + sid + "";
			return await QueryItemsAsync(SqlQuery);
		}

		public async Task<List<Product>> GetProductsByName(string name)
		{
			string SqlQuery = "SELECT * FROM c WHERE STARTSWITH(c.ProductName, '" + name + "')";
			return await QueryItemsAsync(SqlQuery);
		}

		private async Task DBConnect()
		{
			this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "Cosmos-Example" });
			await this.CreateDatabaseAsync();
			await this.CreateContainerAsync();
		}

	}
}

