using Entities;
using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cosmos_Playground
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private async void Import_Click(object sender, EventArgs e)
        {
            List<Product> Products = new List<Product>();
            Products = await MainManager.Instance.ImportProducts();
            TxtResult.Clear();
            foreach (Product product in Products)
            {
                TxtResult.Text += product.ProductName + "\n";
            }
        }

        private async void Cheaper_Click(object sender, EventArgs e)
        {
            List<Product> Products = new List<Product>();
            Products = await CosmosManager.Instance.GetProductsCheaperThen(Convert.ToInt16(TxtCheaper.Text));
            TxtResult.Clear();
            foreach (Product product in Products)
            {
                TxtResult.Text += product.ProductName + "\n";
            }
        }

		private async void OrderID_Click(object sender, EventArgs e)
        {
			List<Product> Products = new List<Product>();
			Products = await CosmosManager.Instance.GetProductsBySupplierID(TxtOrderID.Text.ToString());
			TxtResult.Clear();
			foreach (Product product in Products)
			{
				TxtResult.Text += product.ProductName + "\n";
			}
		}

		private async void ByName_Click(object sender, EventArgs e)
		{
			List<Product> Products = new List<Product>();
			Products = await CosmosManager.Instance.GetProductsByName(TxtName.Text.ToString());
			TxtResult.Clear();
			foreach (Product product in Products)
			{
				TxtResult.Text += product.ProductName + "\n";
			}
		}
	}
}
