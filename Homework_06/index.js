// Task 1
async function calculateTotalAmounts(formContext) {
    let workOrderId = formContext.data.entity.getId().replace('{', '').replace('}', '').toLowerCase()
    let fetchXmlProducts = `
    <fetch version="1.0" mapping="logical" distinct="false" aggregate='true'>
        <entity name="new_work_order_product">
            <attribute name="new_fk_work_order" alias='new_fk_work_order_groupby' groupby='true' />
            <attribute name="new_mon_total_amount" alias='new_mon_total_amount_sum' aggregate='sum' />
        </entity>
    </fetch>
    `

    fetchXmlProducts = '?fetchXml=' + encodeURIComponent(fetchXmlProducts);
    let products = await Xrm.WebApi.retrieveMultipleRecords('new_work_order_product', fetchXmlProducts)
    let totalProductAmount = products.entities.find((a) => a.new_fk_work_order_groupby == workOrderId)

    console.log(totalProductAmount['new_mon_total_amount_sum']);
    formContext.getAttribute('new_total_products_amount').setValue(totalProductAmount['new_mon_total_amount_sum'])

    let fetchXmlService = `
    <fetch version="1.0" mapping="logical" distinct="false" aggregate='true'>
        <entity name="new_work_order_service">
            <attribute name="new_fk_work_order" alias='new_fk_work_order_groupby' groupby='true' />
            <attribute name="new_mon_total_amount" alias='new_mon_total_amount_sum' aggregate='sum' />
        </entity>
    </fetch>
    `

    fetchXmlService = '?fetchXml=' + encodeURIComponent(fetchXmlService);
    let services = await Xrm.WebApi.retrieveMultipleRecords('new_work_order_service', fetchXmlService)
    let totalServiceAmount = services.entities.find((a) => a.new_fk_work_order_groupby == workOrderId)

    console.log(totalServiceAmount['new_mon_total_amount_sum']);
    formContext.getAttribute('new_mon_total_service_amount').setValue(totalServiceAmount['new_mon_total_amount_sum'])

}


// Task 2
// In the Work Order Product Form, if user selects "Product" and if "Inventory" is blank, 
// wrrite a JS to autofill "Inventory" with the Max quantity of selected product 
async function maxQuantity(executionContext) {
    const formContext = executionContext.getFormContext();
    const product = formContext.getAttribute('new_fk_product').getValue()
    const inventory = formContext.getAttribute('new_fk_inventory').getValue()
    const workOrder = formContext.getAttribute('new_fk_work_order').getValue()

    if (product && !inventory) {

        let fetchXmlMaxQuantity = `
            <fetch aggregate="true">
                <entity name="new_work_order_product">
                    <attribute name="new_int_quantity" aggregate="max" alias="new_int_quantity_max" />
                    <filter type="and">
                        <condition attribute="new_fk_work_order" operator="eq" value="${workOrder[0]?.id}" />
                    </filter>
                </entity>
            </fetch>
        `

        fetchXmlMaxQuantity = "?fetchXml=" + encodeURIComponent(fetchXmlMaxQuantity)
        const asset = await Xrm.WebApi.retrieveMultipleRecords('new_work_order_product', fetchXmlMaxQuantity)
        console.log(asset);
        const getMaxQuantity = asset.entities[0]['new_int_quantity_max']

        let fetchXmlInvenory = `
        <fetch version="1.0" mapping="logical" savedqueryid="4ca1f36a-7e9c-476d-83cd-22307121c764"
            no-lock="false" distinct="true">
            <entity name="new_work_order_product">
                <attribute name="new_fk_inventory" />
                <filter type="and">
                    <condition attribute="new_int_quantity" operator="eq" value="${getMaxQuantity}" />
                    <condition attribute="new_fk_work_order" operator="eq"
                        value="${workOrder[0]?.id}" />
                </filter>
            </entity>
        </fetch>
        `
        fetchXmlInvenory = "?fetchXml=" + encodeURIComponent(fetchXmlInvenory)
        let data = await Xrm.WebApi.retrieveMultipleRecords('new_work_order_product', fetchXmlInvenory)
        console.log(data);
        data = data.entities
        // didn't work
        if (data) {
            formContext.getAttribute('new_fk_inventory').setValue([{
                id: `{${data[0]['_new_fk_inventory_value']}}`,
                name: data[0]['_new_fk_inventory_value@OData.Community.Display.V1.FormattedValue'],
                entityType: data[0]['_new_fk_inventory_value@Microsoft.Dynamics.CRM.lookuplogicalname']
            }])
            console.log('ooo');

        }

    }
}