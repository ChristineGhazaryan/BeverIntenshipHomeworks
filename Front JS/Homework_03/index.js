// Task 1
async function initializePriceList(formContext) {
    let priceListId = formContext.data.entity.getId();
    let currency = formContext.getAttribute('transactioncurrencyid').getValue()


    if (priceListId) {
        let fetchXml = `
                <fetch version="1.0" mapping="logical" savedqueryid="8b4b4699-ceac-4d2d-ad42-9e2450dda168"
                    no-lock="false" distinct="true">
                    <entity name="new_price_list_item">
                        <attribute name="new_price_list_itemid" />
                        <filter type="and">
                            <condition attribute="new_fk_price_list" operator="eq"
                                value="${priceListId}" uiname="PriceList - USD"
                                uitype="new_price_list" />
                        </filter>
                    </entity>
                </fetch>
            `
        fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml)
        const asset = await Xrm.WebApi.retrieveMultipleRecords('new_price_list_item', fetchXml)
        const priceListItems = asset?.entities

        if (priceListItems?.length) {
            for (let i = 0; i < priceListItems.length; i++) {
                const item = priceListItems[i];
                let result = await Xrm.WebApi.deleteRecord('new_price_list_item', item['new_price_list_itemid'])
            }
        }
        formContext.data.refresh(true);

        // create products
        let fetchXmlForProducts = `
                <fetch version="1.0" mapping="logical" savedqueryid="977564c7-43b4-4deb-a9f5-7e3c64b4a8d3"
                    no-lock="false" distinct="true">
                    <entity name="new_product">
                        <attribute name="statecode" />
                        <attribute name="new_name" />
                        <attribute name="new_type" />
                        <attribute name="new_price_per_unit" />
                        <attribute name="new_productid" />
                        <filter type="and">
                            <condition attribute="new_productid" operator="not-null" />
                        </filter>
                    </entity>
                </fetch>
            `
        fetchXmlForProducts = "?fetchXml=" + encodeURIComponent(fetchXmlForProducts)
        const assetProducts = await Xrm.WebApi.retrieveMultipleRecords('new_product', fetchXmlForProducts)
        const products = assetProducts?.entities

        if (products?.length) {
            for (let i = 0; i < products.length; i++) {

                const prod = products[i];
                const obj = {}
                obj['new_mon_price'] = 1
                obj['new_fk_price_list@odata.bind'] = `/new_price_lists(${priceListId.replace('{', '').replace('}', '')})`
                obj['new_fk_product@odata.bind'] = `/new_products(${prod['new_productid']})`
                obj["transactioncurrencyid@odata.bind"] = `/transactioncurrencies(${currency[0]?.id.replace('{', '').replace('}', '')})`
                let result = await Xrm.WebApi.createRecord('new_price_list_item', obj)
                formContext.data.refresh(true);
            }
        }
    }
}




// Task 2
async function chackProductInInventory(executionContext) {
    const formContext = executionContext.getFormContext();
    const product = formContext.getAttribute('new_fk_product').getValue()
    const inventory = formContext.getAttribute('new_fk_inventory').getValue()
    if (product && inventory) {

        let fetchXml = `
                    <fetch version="1.0" mapping="logical" savedqueryid="977564c7-43b4-4deb-a9f5-7e3c64b4a8d3"
                        no-lock="false" distinct="true">
                        <entity name="new_product">
                            <attribute name="new_name" />
                            <attribute name="new_type" />
                            <attribute name="new_price_per_unit" />
                            <attribute name="new_productid" />
                            <filter type="and">
                                <condition attribute="new_productid" operator="eq"
                                    value="${product[0]?.id}" />
                            </filter>
                            <link-entity name="new_inventory_product" alias="aa" link-type="inner" from="new_fk_product"
                                to="new_productid">
                                <filter type="and">
                                    <condition attribute="new_fk_inventory" operator="eq"
                                        value="${inventory[0]?.id}"  />
                                </filter>
                            </link-entity>
                        </entity>
                    </fetch>
                `
        fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml)
        const asset = await Xrm.WebApi.retrieveMultipleRecords('new_product', fetchXml)

        if (asset?.entities.length) {
            formContext.getControl('new_fk_product').setNotification('Product is already added', 1)
        } else {
            formContext.getControl('new_fk_product').clearNotification(1)
        }
    }

}