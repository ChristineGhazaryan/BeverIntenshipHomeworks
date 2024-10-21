using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;

namespace ResourcesManagment
{
    public class AutoFillAge : CodeActivity
    {
        // Fields
        [Input("dateOfBirth")]
        public InArgument<DateTime> DateOfBirth { get; set; }

       
        [Output("age")]
        public OutArgument<int> Age { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            
            try
            {
                int currentYear = DateTime.Now.Year;
                DateTime dateOfBirth = DateOfBirth.Get(executionContext);  
                int age = 0;
                if (dateOfBirth != null)
                {
                    age = currentYear - dateOfBirth.Year;
                }
                Age.Set(executionContext, age);
                
                Entity entity = service.Retrieve(context.PrimaryEntityName, context.PrimaryEntityId, new ColumnSet(true));
                entity["new_int_age"] = age;
                service.Update(entity);

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
