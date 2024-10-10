using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WorkOrderManagment
{
    public class ConflicingBookingCheck : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            string messageType = context.MessageName;
            //tracingService.Trace($"messageType {messageType}");

            if (messageType == "Create")
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity bookingAsset = (Entity)context.InputParameters["Target"];
                    if (bookingAsset.Contains("new_fk_resource"))
                    {
                        EntityReference resourceRef = (EntityReference)bookingAsset["new_fk_resource"];
                        Guid resourceId = resourceRef.Id;
                        EntityCollection bookings = getAllBookingByResourceId(resourceId, service);
                        checking(bookings, bookingAsset, tracingService);
                    }

                }
            }
            else if (messageType == "Update")
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity entityReference)
                {
                    // with trace check this part
                    //tracingService.Trace($"entityReference- {entityReference.Id}");
                    Entity bookingAsset = getBooking(entityReference.Id, service);

                    EntityReference resource = (EntityReference)bookingAsset["new_fk_resource"];
                    EntityCollection bookings = getAllBookingByResourceId(resource.Id, service);
                    checking(bookings, bookingAsset, tracingService);
                    service.Update(bookingAsset); // didn't work
                }
            }
        }

        public Entity getBooking(Guid id, IOrganizationService service)
        {
            string entityName = "new_booking";
            ColumnSet columns = new ColumnSet(true);
            Entity bookingEntity = service.Retrieve(entityName, id, columns);
            return bookingEntity;
        }

        public EntityCollection getAllBookingByResourceId(Guid resourceId, IOrganizationService service)
        {
            QueryExpression queryBookings = new QueryExpression
            {

                EntityName = "new_booking",
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("new_fk_resource", ConditionOperator.Equal, resourceId)
                    }
                }

            };
            return service.RetrieveMultiple(queryBookings);
        }

        public static bool IsOverlappingOrContained(DateTime targetStart, DateTime targetEnd, DateTime rangeStart, DateTime rangeEnd)
        {
            return (targetStart >= rangeStart && targetEnd <= rangeEnd)
                   || (targetStart < rangeEnd && targetEnd > rangeStart);
        }
        public void checking(EntityCollection bookings, Entity bookingAsset, ITracingService tracingService)
        {
            DateTime bookingStartDate = (DateTime)bookingAsset["new_dt_start_date"];
            DateTime bookingEndDate = (DateTime)bookingAsset["new_dt_end_date"];
            EntityReference resource = (EntityReference)bookingAsset["new_fk_resource"];
            Guid resourceId = resource.Id;
            foreach (Entity entity in bookings.Entities)
            {
                DateTime bookingStDt = (DateTime)entity["new_dt_start_date"];
                DateTime bookingEndDt = (DateTime)entity["new_dt_end_date"];
                EntityReference entityResource = (EntityReference)entity["new_fk_resource"];
                Guid entityResourceId = entityResource.Id;
                bool match = IsOverlappingOrContained(bookingStartDate, bookingEndDate, bookingStDt, bookingEndDt);
                if (match)
                {
                    //tracingService.Trace("match");
                    var fault = new OrganizationServiceFault
                    {
                        Message = "Hours coincide with another booking",
                        ErrorCode = -2147220891
                    };

                    throw new FaultException<OrganizationServiceFault>(fault, new FaultReason(fault.Message));
                }
            }
        }
    }
}
