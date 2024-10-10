using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourcesManagment
{
    public class AutoFillAge : CodeActivity
    {
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
            // IExecutionContext crmContext = executionContext.GetExtension<IExecutionContext>();
            try
            {
                int currentYear = DateTime.Now.Year;
                DateTime dateOfBirth = DateOfBirth.Get(executionContext);  
                int age = 0;
                if (dateOfBirth != null)
                {
                    age = currentYear - dateOfBirth.Year;
                    tracingService.Trace($"{currentYear} - {dateOfBirth}");
                }
                Age.Set(executionContext, age);

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
