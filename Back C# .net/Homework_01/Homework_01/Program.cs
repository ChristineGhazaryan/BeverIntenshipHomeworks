using Homework_01.Model;
using Homework_01.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework_01
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                D365Connector d365Connector = new D365Connector("ChristineGhazaryan@BeverSystem149.onmicrosoft.com",
                                                                "christ1ne25*_",
                                                                "https://org44eb61b2.api.crm4.dynamics.com/api/data/v9.2/");
                Console.WriteLine("Connected!!!");
                Console.WriteLine("Enter Inventory, Product, Quantity and Type of operation");

                string inventory = Console.ReadLine();
                string product = Console.ReadLine();
                int quantity = Int32.Parse(Console.ReadLine());
                string typeOfOperation = Console.ReadLine();

                Guid inventoryId = d365Connector.getInventoryId(inventory);
                Guid productId = d365Connector.getProductId(product);
                InventoryProduct inventoryProduct = d365Connector.getInventoryProduct(inventoryId, productId);

                if (typeOfOperation == "subtraction")
                {
                    if (inventoryProduct.quantity >= quantity)
                    {
                        // call update function
                        d365Connector.updateInventoryProduct(inventoryProduct, -quantity); 
                    }
                    else
                    {
                        Console.WriteLine("It's not posible");
                    }
                }
                else if (typeOfOperation == "addition")
                {
                    if (inventoryProduct.IsEmpty())
                    {
                        // creating new 
                        d365Connector.createInventoryProduct(inventoryId, productId, quantity);  
                    }
                    else
                    {
                        d365Connector.updateInventoryProduct(inventoryProduct, quantity);
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
