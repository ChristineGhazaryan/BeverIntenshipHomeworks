using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow.Activities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoriesManagment
{
    public class CalculateTotalAmount : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            string messageType = context.MessageName;

            if (messageType == "Create")
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity inventoryProduct = (Entity)context.InputParameters["Target"];
                    EntityReference inventoryRef = (EntityReference)inventoryProduct["new_fk_inventory"];
                    Guid inventoryId = inventoryRef.Id;

                    EntityCollection inventoryProducts = getInventoryProducts(inventoryId, service);
                    decimal totalAmount = colculateTotalAmount(inventoryProducts);
                    updateInventoryTotalAmount(service, inventoryId, totalAmount);
                }
            }
            else if (messageType == "Update")
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity entity)
                {
                    Guid inventoryId = getInventoryId(service, entity.Id);
                    EntityCollection inventoryProducts = getInventoryProducts(inventoryId, service);
                    decimal totalAmount = colculateTotalAmount(inventoryProducts);
                    updateInventoryTotalAmount(service, inventoryId, totalAmount);
                }
            }
            else if (messageType == "Delete")
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference entityReference)
                {
                    Guid inventoryId = getInventoryId(service, entityReference.Id);
                    EntityCollection inventoryProducts = getInventoryProducts(inventoryId, service);
                    inventoryProducts = filterInventoryProductsForDelete(inventoryProducts, entityReference.Id);
                    decimal totalAmount = colculateTotalAmount(inventoryProducts);
                    updateInventoryTotalAmount(service, inventoryId, totalAmount);
                }

            }
        }
        // filter for delete record
        public EntityCollection filterInventoryProductsForDelete(EntityCollection inventoryProducts, Guid entityReferenceId)
        {
            EntityCollection filteredCollection = new EntityCollection();

            foreach (Entity entity in inventoryProducts.Entities)
            {
                if (entity.Contains("new_total_amount") && entity.Id != entityReferenceId)
                {
                    filteredCollection.Entities.Add(entity);
                }
            }
            return filteredCollection;
        }
        // updateing
        public void updateInventoryTotalAmount(IOrganizationService service, Guid inventoryId, decimal totalAmount)
        {
            Entity inventoryToUpdate = new Entity("new_inventory", inventoryId);
            inventoryToUpdate["new_mon_total_amount"] = new Money(totalAmount);
            service.Update(inventoryToUpdate);
        }
        //colculation
        public decimal colculateTotalAmount(EntityCollection inventoryProducts)
        {
            decimal totalAmountSum = 0m;

            foreach (Entity entity in inventoryProducts.Entities)
            {
                if (entity.Contains("new_total_amount"))
                {
                    Money totalAmount = (Money)entity["new_total_amount"];
                    totalAmountSum += totalAmount.Value;
                }
            }

            return totalAmountSum;
        }

        // get all inventory Products by inv 
        public EntityCollection getInventoryProducts(Guid inventoryId, IOrganizationService service)
        {
            QueryExpression queryInventoryProducts = new QueryExpression
            {

                EntityName = "new_inventory_product",
                ColumnSet = new ColumnSet("new_total_amount"),
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
        //get Inventory id 
        public Guid getInventoryId(IOrganizationService service, Guid entityId)
        {
            Entity inventoryProductdEntity = service.Retrieve("new_inventory_product", entityId, new ColumnSet("new_fk_inventory"));
            EntityReference primaryContact = (EntityReference)inventoryProductdEntity["new_fk_inventory"];
            Guid inventoryId = primaryContact.Id;
            return inventoryId;
        }

    }
}
