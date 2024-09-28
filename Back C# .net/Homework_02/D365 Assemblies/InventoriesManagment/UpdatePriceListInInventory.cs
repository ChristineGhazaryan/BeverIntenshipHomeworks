using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoriesManagment
{
    public class UpdatePriceListInInventory : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            string messageType = context.MessageName;

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity inventoryEntity = (Entity)context.InputParameters["Target"];

                if (inventoryEntity.Contains("new_fk_price_list"))
                {
                    EntityReference priceListLookup = (EntityReference)inventoryEntity["new_fk_price_list"];
                    Guid priceListId = priceListLookup.Id;
                    EntityCollection inventoryProducts = getInventoryProducts(inventoryEntity.Id, service);

                    foreach (var inventoryProduct in inventoryProducts.Entities)
                    {
                        if (inventoryProduct.Contains("new_fk_product"))
                        {
                            EntityReference productLookup = (EntityReference)inventoryProduct["new_fk_product"];
                            Entity priceListItem = getProductsFromPriceListItems(priceListId, productLookup.Id, service);
                            if (priceListItem != null)
                            {
                                updateInventoryProduct(inventoryProduct, priceListItem, service);
                            }
                        }
                    }
                }
            }

        }

        public EntityCollection getInventoryProducts(Guid inventoryId, IOrganizationService service)
        {
            QueryExpression queryInventoryProducts = new QueryExpression
            {

                EntityName = "new_inventory_product",
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("new_fk_inventory", ConditionOperator.Equal, inventoryId)
                    }
                }

            };
            return service.RetrieveMultiple(queryInventoryProducts);
        }

        public Entity getProductsFromPriceListItems(Guid priceListId, Guid productId, IOrganizationService service)
        {
            QueryExpression queryInventoryProducts = new QueryExpression
            {

                EntityName = "new_price_list_item",
                ColumnSet = new ColumnSet("new_mon_price"),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("new_fk_price_list", ConditionOperator.Equal, priceListId),
                        new ConditionExpression("new_fk_product", ConditionOperator.Equal, productId)
                    }
                }
            };
            EntityCollection data = service.RetrieveMultiple(queryInventoryProducts);
            if (data != null && data.Entities.Count > 0)
            {
                return data.Entities[0];
            }
            return null;

        }

        public void updateInventoryProduct(Entity inventoryProduct, Entity priceListItem, IOrganizationService service)
        {
            if (inventoryProduct.Contains("new_price_per_unit") && priceListItem.Contains("new_mon_price"))
            {
                inventoryProduct["new_price_per_unit"] = priceListItem["new_mon_price"];

                int quantity = (int)inventoryProduct["new_int_quantity"];
                Money pricePerUnit = (Money)inventoryProduct["new_price_per_unit"];
                decimal totalAmountValue = quantity * pricePerUnit.Value;
                inventoryProduct["new_total_amount"] = totalAmountValue;

                service.Update(inventoryProduct);
            }
        }
    }
}
