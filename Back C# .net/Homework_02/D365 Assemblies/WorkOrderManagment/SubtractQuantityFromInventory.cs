using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkOrderManagment
{
    public class SubtractQuantityFromInventory : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            string messageType = context.MessageName;
            
            if (messageType == "Update")
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity workOrderAsset = (Entity)context.InputParameters["Target"];
                    if (workOrderAsset.Contains("new_os_status"))
                    {
                        OptionSetValue statusOptionSet = (OptionSetValue)workOrderAsset["new_os_status"];
                        int status = statusOptionSet.Value;

                        if (status == 100000001)
                        {
                            EntityCollection workOrderProducts = getWorkOrderProductsByWorkOrderId(workOrderAsset.Id, service);
                            checkAndUpdateInventoryProduct(workOrderProducts, service);
                        }
                    }
                }
            }

        }

        public EntityCollection getWorkOrderProductsByWorkOrderId(Guid workOrderId, IOrganizationService service)
        {
            QueryExpression queryInventoryProducts = new QueryExpression
            {

                EntityName = "new_work_order_product",
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("new_fk_work_order", ConditionOperator.Equal, workOrderId)
                    }
                }

            };
            return service.RetrieveMultiple(queryInventoryProducts);
        }

        public Entity getInventoryProduct(Guid productId, Guid inventoryId, IOrganizationService service)
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
                        new ConditionExpression("new_fk_inventory", ConditionOperator.Equal, inventoryId),
                        new ConditionExpression("new_fk_product", ConditionOperator.Equal, productId),
                    }
                }

            };
            EntityCollection data = service.RetrieveMultiple(queryInventoryProducts);
            if (data == null || data.Entities.Count == 0)
            {
                return null;
            }
            return data.Entities[0];
        }

        public void checkAndUpdateInventoryProduct(EntityCollection workOrderProducts, IOrganizationService service)
        {

            bool temp = false;
            EntityCollection entityCollection = new EntityCollection();
            foreach (Entity entity in workOrderProducts.Entities)
            {
                if (entity.Contains("new_fk_inventory") && entity.Contains("new_fk_product"))
                {
                    EntityReference inventoryRef = (EntityReference)entity["new_fk_inventory"];
                    Guid inventoryId = inventoryRef.Id;

                    EntityReference productRef = (EntityReference)entity["new_fk_product"];
                    Guid productId = productRef.Id;

                    Entity inventoryProduct = getInventoryProduct(productId, inventoryId, service);

                    if (inventoryProduct != null)
                    {
                        int quantity = (int)inventoryProduct["new_int_quantity"];
                        int workOrderProductQuantity = (int)entity["new_int_quantity"];
                        if (quantity >= workOrderProductQuantity)
                        {
                            inventoryProduct["new_int_quantity"] = quantity - workOrderProductQuantity;
                            entityCollection.Entities.Add(inventoryProduct);
                        }
                        else
                        {
                            temp = true;
                        }
                    }
                }

            }

            if (temp)
            {
                throw new InvalidPluginExecutionException("Problem with quantity");
            }
            else
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    service.Update(entity);
                }

            }
        }

    }
}
