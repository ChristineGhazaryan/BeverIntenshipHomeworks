// Task 1
async function changeCurrency(executionContext) {
    const formContext = executionContext.getFormContext();
    formContext.getControl('transactioncurrencyid').setDisabled(true)

    let priceList = formContext.getAttribute('new_fk_price_list').getValue()

    if (priceList) {
        const priceListEntityType = priceList[0].entityType;
        const priceListId = priceList[0]?.id.replace('{', '').replace('}', '')

        const asset = await Xrm.WebApi.retrieveRecord(priceListEntityType, priceListId);

        formContext.getAttribute('transactioncurrencyid').setValue([{
            id: `{${asset['_transactioncurrencyid_value']}}`,
            name: asset['_transactioncurrencyid_value@OData.Community.Display.V1.FormattedValue'],
            entityType: "transactioncurrency"
        }])

    }
}



// Task 2 +
// ------------------------------------------------------
// Make "name" field in the "price list item" hidden and autofill from "Product",
// Make hidden owner
function changeName(executionContext) {
    const formContext = executionContext.getFormContext();

    let product = formContext.getAttribute("new_fk_product").getValue();

    if (product) {
        let product_name = product[0]?.name
        formContext.getAttribute('new_name').setValue(product_name)
    }
    formContext.getControl("new_name").setVisible(false);
}



// Task 3
// ------------------------------------------------------
// Change "price Per Unit" subgrig under "price List" to show "product", "price per unit"
// done, i dont use js code

// Task 4
// -------------------------------------------------------
// In the "price list item" chnage 'Active Price Lists' view, to show
// the following columns "product", "price per unit", "price List" , "Currency"
// done

// Task 5
// Rename 'price per unit' to be "default price per unit"
// And show "currency" on top of it
// done

// Task 6
// Add a lookup to the "price list" in the inventory form
// done


// Task 7
// Add currency under "Inventory Product"
// done

// Task 8
// Make currency disabled in the "inventory Product" and autofill from the currency
// in the price list in "Inventory"
async function changeCurrencyForInventoryProduct(executionContext) {
    const formContext = executionContext.getFormContext();
    let inventory = formContext.getAttribute('new_fk_inventory').getValue()
    formContext.getControl('transactioncurrencyid').setDisabled(true)

    if (inventory) {

        let fetchXml = `
                <fetch version="1.0" mapping="logical" savedqueryid="c4972ce9-a9dd-4244-9d44-62eb6ca3fd88"
                    no-lock="false" distinct="true">
                    <entity name="new_inventory">
                        <attribute name="statecode" />
                        <attribute name="new_name" />
                        <attribute name="createdon" />
                        <order attribute="new_name" descending="false" />
                        <attribute name="new_os_type" />
                        <attribute name="new_inventoryid" />
                        <filter type="and">
                            <condition attribute="new_inventoryid" operator="eq"
                                value="${inventory[0]?.id}" />
                        </filter>
                        <link-entity name="new_price_list" alias="ab" link-type="inner" from="new_price_listid"
                            to="new_fk_price_list">
                            <link-entity name="transactioncurrency" alias="ac" link-type="inner"
                                from="transactioncurrencyid" to="transactioncurrencyid" >
                                    <attribute name="currencyname" />
                                    <attribute name="transactioncurrencyid" />
                                </link-entity>
                        </link-entity>
                    </entity>
                </fetch>
            `

        fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml)
        const asset = await Xrm.WebApi.retrieveMultipleRecords('new_inventory', fetchXml)

        if (asset.entities) {
            const cur = asset.entities[0]

            formContext.getAttribute('transactioncurrencyid').setValue([{
                name: `${cur['ac.currencyname']}`,
                id: `{${cur['ac.transactioncurrencyid']}}`,
                entityType: "transactioncurrency"
            }])

        }

    }

}



// Task 9
// Make "price Per unit" disabled in the "inventory Product" form and
// bring a price from "price List" in the "Inventory" if exist,
// otherwise get default price from the product
async function changePricePerUnit(executionContext) {
    const formContext = executionContext.getFormContext();
    formContext.getControl('new_price_per_unit').setDisabled(true)

    let product = formContext.getAttribute('new_fk_product').getValue()
    let inventory = formContext.getAttribute('new_fk_inventory').getValue()
    console.log(product);
    console.log(inventory);



    if (product && inventory) {


        let fetchXml = `
            <fetch version="1.0" mapping="logical" savedqueryid="c4972ce9-a9dd-4244-9d44-62eb6ca3fd88"
                no-lock="false" distinct="true">
                <entity name="new_inventory">
                    <attribute name="statecode" />
                    <attribute name="new_name" />
                    <attribute name="createdon" />
                    <order attribute="new_name" descending="false" />
                    <attribute name="new_os_type" />
                    <attribute name="new_inventoryid" />
                    <filter type="and">
                        <condition attribute="new_inventoryid" operator="eq"
                            value="${inventory[0]?.id}" />
                    </filter>
                    <link-entity name="new_price_list" alias="ac" link-type="inner" from="new_price_listid"
                        to="new_fk_price_list">
                        <link-entity name="new_price_list_item" alias="af" link-type="inner"
                            from="new_fk_price_list" to="new_price_listid">
                            <filter type="and">
                                <condition attribute="new_fk_product" operator="eq"
                                    value="${product[0]?.id}" uiname="Fanta"
                                    uitype="new_product" />
                            </filter>
                            <attribute name="new_mon_price" />
                        </link-entity>
                    </link-entity>
                </entity>
            </fetch>

        `
        fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml)
        // console.log(fetchXml);

        const asset = await Xrm.WebApi.retrieveMultipleRecords('new_inventory', fetchXml)
        console.log(asset);
        if (asset?.entities?.length) {
            const price_per_unit = asset.entities[0]['af.new_mon_price']
            console.log(price_per_unit);
            formContext.getAttribute('new_price_per_unit').setValue(price_per_unit)

        } else {
            let fetchXml = `
                <fetch version="1.0" mapping="logical" savedqueryid="977564c7-43b4-4deb-a9f5-7e3c64b4a8d3"
                    no-lock="false" distinct="true">
                    <entity name="new_product">
                        <attribute name="new_price_per_unit" />
                        <filter type="and">
                            <condition attribute="new_productid" operator="eq"
                                value="${product[0]?.id}"  />
                        </filter>
                    </entity>
                </fetch>
            `
            fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml)
            const asset = await Xrm.WebApi.retrieveMultipleRecords('new_product', fetchXml)
            console.log(asset);

            const price_per_unit = asset.entities[0]['new_price_per_unit']
            console.log(price_per_unit);
            formContext.getAttribute('new_price_per_unit').setValue(price_per_unit)
        }
    }
}



