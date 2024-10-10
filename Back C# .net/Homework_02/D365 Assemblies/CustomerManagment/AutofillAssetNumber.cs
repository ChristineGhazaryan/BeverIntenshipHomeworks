using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManagment
{
    public class AutofillAssetNumber : IPlugin
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
                    Entity customerAsset = (Entity)context.InputParameters["Target"];

                    if (customerAsset.Contains("new_fk_my_account"))
                    {
                        EntityReference accountRef = (EntityReference)customerAsset["new_fk_my_account"];
                        Guid accountId = accountRef.Id;

                        Entity accountEntity= getAccountName(accountId, service);
                        updateCustomerAssetNumber(customerAsset, accountEntity, service);
                    }
                }
            }
        }
        public Entity getAccountName(Guid accountId, IOrganizationService service)
        {
            string entityName = "new_my_account";
            ColumnSet columns = new ColumnSet(true);
            Entity accountEntity = service.Retrieve(entityName, accountId, columns);

            return accountEntity;

        }
        public void updateCustomerAssetNumber(Entity customerAsset, Entity accountEntity, IOrganizationService service)
        {
            int assetNumber = (int)accountEntity["new_int_asset_number"];
            string accountName = accountEntity.Contains("new_name") ? accountEntity["new_name"].ToString() : string.Empty;
            string customerAssetName = $"{accountName} - 00{assetNumber + 1}";
            
            customerAsset["new_name"] = customerAssetName;
            accountEntity["new_int_asset_number"] = assetNumber + 1;
            service.Update(accountEntity);
        }
    }
}
