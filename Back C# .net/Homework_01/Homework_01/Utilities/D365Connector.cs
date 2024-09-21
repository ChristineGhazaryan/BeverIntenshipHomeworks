using Homework_01.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework_01.Utilities
{
    internal class D365Connector
    {
        private string D365username;
        private string D365Password;
        private string D365URL;

        private CrmServiceClient service;

        public D365Connector(string D365username, string D365Password, string D365URL)
        {
            this.D365username = D365username;
            this.D365Password = D365Password;
            this.D365URL = D365URL;

            string authType = "OAuth";
            string appId = "51f81489-12ee-4a9e-aaae-a2591f45987d";
            string reDirectURI = "app://58145B91-0C36-4500-8554-080854F2AC97";
            string loginPrompt = "Auto";

            string ConnectingString = string.Format("AuthType = {0};Username = {1};Password = {2}; Url = {3}; AppId={4}; RedirectUri={5};LoginPrompt={6}",
                authType, D365username, D365Password, D365URL, appId, reDirectURI, loginPrompt);

            this.service = new CrmServiceClient(ConnectingString);

        }
        // get Inventory Product 
        public InventoryProduct getInventoryProduct(Guid inventoryId, Guid productId)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = "new_inventory_product",
                ColumnSet = new ColumnSet("new_fk_inventory", "new_fk_product", "new_int_quantity", "new_price_per_unit"),
                Criteria =
                {
                    FilterOperator=LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("new_fk_inventory", ConditionOperator.Equal, inventoryId),
                        new ConditionExpression("new_fk_product", ConditionOperator.Equal, productId)
                    }
                }
            };
            //Console.WriteLine(query);
            EntityCollection data = this.service.RetrieveMultiple(query);
            //Console.WriteLine(data.Entities.Count);
            try
            {
                if (data.Entities.Count > 0)
                {
                    Entity inventoryProduct = data.Entities[0];
                    InventoryProduct inventoryProductObj = new InventoryProduct();
                    inventoryProductObj.invevnetoryProductId = inventoryProduct.Id;
                    Money pricePerUnit = inventoryProduct.GetAttributeValue<Money>("new_price_per_unit");
                    inventoryProductObj.pricePerUnit = pricePerUnit.Value;
                    inventoryProductObj.quantity = inventoryProduct.GetAttributeValue<int>("new_int_quantity");
                    EntityReference invId = inventoryProduct.GetAttributeValue<EntityReference>("new_fk_inventory");
                    EntityReference prodId = inventoryProduct.GetAttributeValue<EntityReference>("new_fk_product");
                    if (invId != null && prodId != null)
                    {
                        Console.WriteLine("invevnetoryId");
                        inventoryProductObj.invevnetoryId = invId.Id;
                        inventoryProductObj.productId = prodId.Id;
                    }

                    return inventoryProductObj;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return new InventoryProduct();
        }

        // get Inventory Id By name
        public Guid getInventoryId(string inventoryName)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = "new_inventory",
                ColumnSet = new ColumnSet("new_name"),
                Criteria =
                {
                    FilterOperator=LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("new_name", ConditionOperator.Equal, inventoryName)
                    }
                }
            };

            try
            {
                EntityCollection data = this.service.RetrieveMultiple(query);
                if (data.Entities.Count > 0)
                {
                    Guid recordId = data.Entities[0].Id;
                    return recordId;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return Guid.Empty;
        }

        // get Product Id By name
        public Guid getProductId(string productName)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = "new_product",
                ColumnSet = new ColumnSet("new_name"),
                Criteria =
                {
                    FilterOperator=LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("new_name", ConditionOperator.Equal, productName)
                    }
                }
            };

            try
            {
                EntityCollection data = this.service.RetrieveMultiple(query);
                if (data.Entities.Count > 0)
                {
                    Guid recordId = data.Entities[0].Id;
                    return recordId;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return Guid.Empty;
        }

        public void updateInventoryProduct(InventoryProduct inventoryProduct, int quantity)
        {
            try
            {
                Guid inventoryProductId = new Guid($"{inventoryProduct.invevnetoryProductId}");
                Entity inventoryProductEntity = service.Retrieve("new_inventory_product", inventoryProductId, new ColumnSet("new_int_quantity"));

                int currentQuantity = inventoryProductEntity.GetAttributeValue<int>("new_int_quantity");
                int newQuantity = currentQuantity + quantity;
                inventoryProductEntity["new_int_quantity"] = newQuantity;


                service.Update(inventoryProductEntity);
                Console.WriteLine("Inventory product updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating inventory product: {ex.Message}");
            }


        }


        //create Inventory Product
        public void createInventoryProduct(Guid inventoryId, Guid productId, int quantity)
        {
            try
            {
                Entity inventoryProductObj = new Entity("new_inventory_product");
                inventoryProductObj["new_int_quantity"] = quantity;

                Entity productEntity = service.Retrieve("new_product", productId, new ColumnSet("transactioncurrencyid", "new_price_per_unit"));
                EntityReference transactioncurrencyid = productEntity.GetAttributeValue<EntityReference>("transactioncurrencyid");

                QueryExpression PriceListQuery = new QueryExpression
                {
                    EntityName = "new_price_list",
                    Criteria =
                {
                    FilterOperator=LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("transactioncurrencyid", ConditionOperator.Equal, transactioncurrencyid.Id),
                    }
                }
                };
                EntityCollection priceList = this.service.RetrieveMultiple(PriceListQuery);
                Guid priceListId = priceList.Entities[0].Id;

                QueryExpression priceListItemQuery = new QueryExpression
                {
                    EntityName = "new_price_list_item",
                    ColumnSet = new ColumnSet("new_mon_price"),
                    Criteria =
                {
                    FilterOperator=LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("new_fk_price_list", ConditionOperator.Equal, priceListId),
                        new ConditionExpression("new_fk_product", ConditionOperator.Equal, productId)
                    }
                }
                };
                EntityCollection priceListItem = this.service.RetrieveMultiple(priceListItemQuery);

                if ( priceListItem.Entities.Count > 0)
                {
                    Money pricePerUnit = priceListItem.Entities[0].GetAttributeValue<Money>("new_mon_price");
                    Console.WriteLine($"pricePerUnit if {pricePerUnit}");
                    inventoryProductObj["new_price_per_unit"] = pricePerUnit;
                    inventoryProductObj["new_total_amount"] = new Money(pricePerUnit.Value * quantity);
                } else
                {
                    Money pricePerUnit = productEntity.GetAttributeValue<Money>("new_mon_price");
                    Console.WriteLine($"pricePerUnit else {pricePerUnit}");
                    inventoryProductObj["new_price_per_unit"] = pricePerUnit;
                    inventoryProductObj["new_total_amount"] = new Money(pricePerUnit.Value * quantity);
                }

                inventoryProductObj["new_fk_inventory"] = new EntityReference("new_inventory", inventoryId);
                inventoryProductObj["new_fk_product"] = new EntityReference("new_product", productId);
                inventoryProductObj["transactioncurrencyid"] = new EntityReference("transactioncurrency", transactioncurrencyid.Id);

                Guid newRecordId = service.Create(inventoryProductObj);

                Console.WriteLine($"New inventory product record created with ID: {newRecordId}");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

        }

    }
}
