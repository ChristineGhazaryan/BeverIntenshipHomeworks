

// ------------------------------
async function calculateChildsQuantity(executionContext) {
    const formContext = executionContext.getFormContext();
    let recordId = formContext.data.entity.getId()

    let fetchXml = `
        <fetch version="1.0" mapping="logical" savedqueryid="d7a6c03f-d23e-4885-ad48-2d63d085440e" no-lock="false" distinct="true">
            <entity name="new_inventory_product">
                <attribute name="new_name"/>
                <attribute name="statecode"/>
                <!-- <order attribute="new_fk_inventory" descending="false"/> -->
                <!-- <attribute name="new_total_amount"/> -->
                <attribute name="new_int_quantity"/>
                <attribute name="new_fk_product"/>
                <!-- <attribute name="new_price_per_unit"/> -->
                <attribute name="new_fk_inventory"/>
                <!-- <attribute name="new_inventory_productid"/> -->
                <filter type="and">
                    <condition attribute="new_fk_inventory" operator="eq" value="${recordId}" />
                </filter>
            </entity>
        </fetch>
    `
    fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml)

    let result = await Xrm.WebApi.retrieveMultipleRecords('new_inventory_product', fetchXml)
    let itemsQuantity = result.entities.length
    formContext.getAttribute("new_int_items_quantity").setValue(itemsQuantity)
}


// ------------------ fetch -----------------------
async function calculateTotalPriceInInventory(executionContext) {
    const formContext = executionContext.getFormContext();
    let recordId = formContext.data.entity.getId()

    let fetchXml = `
        <fetch version="1.0" mapping="logical" savedqueryid="d7a6c03f-d23e-4885-ad48-2d63d085440e" no-lock="false" distinct="true">
            <entity name="new_inventory_product">
                <attribute name="new_name"/>
                <attribute name="statecode"/>
                <!-- <order attribute="new_fk_inventory" descending="false"/> -->
                <!-- <attribute name="new_total_amount"/> -->
                <attribute name="new_int_quantity"/>
                <attribute name="new_fk_product"/>
                <!-- <attribute name="new_price_per_unit"/> -->
                <attribute name="new_fk_inventory"/>
                <!-- <attribute name="new_inventory_productid"/> -->
                <filter type="and">
                    <condition attribute="new_fk_inventory" operator="eq" value="${recordId}" />
                </filter>
            </entity>
        </fetch>
    `
    fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml)

    let recordsArray = await Xrm.WebApi.retrieveMultipleRecords('new_inventory_product', fetchXml)
    let itemsQuantity = recordsArray.entities.length
    for (let i = 0; i < itemsQuantity; i++) {
        const inventoryLine = recordsArray.entities[i]
        alert(inventoryLine['new_int_quantity'])
        alert(inventoryLine['_new_fk_product_value'])
    }
}

// --------------------------link-entity------------------------------------
async function calculateTotalPriceInInventory(executionContext) {
    const formContext = executionContext.getFormContext();
    let recordId = formContext.data.entity.getId()

    let fetchXml = `
<fetch version="1.0" mapping="logical" savedqueryid="d7a6c03f-d23e-4885-ad48-2d63d085440e"
    no-lock="false" distinct="true">
    <entity name="new_inventory_product">
        <attribute name="new_inventory_productid" />
        <attribute name="new_name" />
        <attribute name="new_int_quantity" />
        <attribute name="new_fk_product" />
        <attribute name="new_fk_inventory" />
        <order attribute="new_name" descending="false" />
        <filter type="and">
            <condition attribute="new_fk_inventory" operator="eq" value="${recordId}" />
        </filter>
        <link-entity name="new_product" alias="al" link-type="inner" from="new_productid"
            to="new_fk_product">
            <link-entity name="new_price_list_item" alias="ao" link-type="inner"
                from="new_fk_product" to="new_productid" >
                <attribute name="new_mon_price" />
                <attribute name="new_fk_price_list" />

            </link-entity>
        </link-entity>

    </entity>
</fetch>
    `
    fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml)

    let inventoryLinesArray = await Xrm.WebApi.retrieveMultipleRecords('new_inventory_product', fetchXml)
    let itemsLinesQuantity = inventoryLinesArray.entities.length

    let priceList = formContext.getAttribute('new_fk_price_list').getValue()
    console.log('price List =>', priceList)
    let priceListId = priceList[0].id
    console.log('price lost id', priceListId)
    let totaPrice = 0

    for (let i = 0; i < itemsLinesQuantity; i++) {
        const line = inventoryLinesArray.entities[i]
        if (line['ao.new_fk_price_list'] == priceListId.replace('{', '').replace('}', '').toLowerCase()) {
            let quantity = line['new_int_quantity']
            let price = line['ao.new_mon_price']
            totaPrice += price * quantity
        }

    }
    formContext.getAttribute('new_mon_total_amount').setValue(totaPrice)
}
