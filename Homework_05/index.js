// autofill full name in resourses
// in work order product price per unit type is in correct maybe fk to mon in name
// MyPosition view chnage
// MyContact view changes

// ------------------------------------------------
// Task 1
// In the Work Order, filter Customer Assets lookup to get only those,
// which is related to the customer
// done

// Task 2
// In the Work Orer Create a contact lookup. Write a JS code to filter Contacts, based on
// the Customer. Filter contact, based on the position that's relates Contact to the
// Account (Customer).
let contactLookupPointer = null

async function filterContact(executionContext) {
    let formContext = executionContext.getFormContext()
    let customer = formContext.getAttribute('new_fk_customer').getValue()

    if (contactLookupPointer != null) {
        formContext.getControl('new_fk_contact').removePreSearch(contactLookupPointer)
    }

    let fetchXml = `
                <fetch version="1.0" mapping="logical" savedqueryid="00184a6f-fe84-4e77-9776-be05df9ccbec"
                    no-lock="false" distinct="true">
                    <entity name="new_my_contact">
                        <attribute name="new_my_contactid" />
                        <filter type="and">
                            <condition attribute="new_my_contactid" operator="not-null" />
                        </filter>
                        <link-entity name="new_my_positions" alias="aa" link-type="inner" from="new_fk_contact"
                            to="new_my_contactid">
                            <filter type="and">
                                <condition attribute="new_fk_account" operator="eq"
                                    value="${customer[0]?.id}"  />
                            </filter>
                        </link-entity>
                    </entity>
                </fetch>`

    fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml)
    const assets = await Xrm.WebApi.retrieveMultipleRecords('new_my_contact', fetchXml)
    const contacts = assets?.entities

    contactLookupPointer = filterFunction.bind({ "contacts": contacts })
    formContext.getControl('new_fk_contact').addPreSearch(contactLookupPointer)
}

function filterFunction(executionContext) {
    let formContext = executionContext.getFormContext()
    let fetchXmlContacts = `<filter type="or">`
    for (let i = 0; i < this.contacts.length; i++) {
        const contactId = this.contacts[i]['new_my_contactid']
        fetchXmlContacts += `<condition attribute="new_my_contactid" operator="eq" value="{${contactId}}" />`
    }
    fetchXmlContacts += `</filter>`
    formContext.getControl('new_fk_contact').addCustomFilter(fetchXmlContacts, 'new_my_contact')

}


// Task 3
// In the work order product entity, change places "Inventory" and "Product" fields.
// First should be "Inventory" and then "Product" in the forms. Write a JS to filter "Product"
// field based on the "Inventory" and to get only products where "type"="Product"

let productLookupPointer = null
async function filterProducts(executionContext) {
    let formContext = executionContext.getFormContext()
    let inventory = formContext.getAttribute('new_fk_inventory').getValue()

    if (productLookupPointer != null) {
        formContext.getControl('new_fk_product').removePreSearch(productLookupPointer)
    }

    let fetchXml = `
                <fetch version="1.0" mapping="logical" distinct="true">
                    <entity name="new_product">
                        <attribute name="new_productid" />
                        <filter type="and">
                            <condition attribute="new_type" operator="eq" value="100000000" />
                        </filter>
                        <link-entity name="new_inventory_product" alias="aa" link-type="inner" from="new_fk_product"
                            to="new_productid">
                            <filter type="and">
                                <condition attribute="new_fk_inventory" operator="eq"
                                    value="${inventory[0]?.id}"/>
                            </filter>
                        </link-entity>
                    </entity>
                </fetch>`

    fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml)
    const result = await Xrm.WebApi.retrieveMultipleRecords('new_product', fetchXml)
    const products = result?.entities

    productLookupPointer = filterFunctionForProducts.bind({ "products": products })
    formContext.getControl('new_fk_product').addPreSearch(productLookupPointer)
}

function filterFunctionForProducts(executionContext) {
    let formContext = executionContext.getFormContext()
    console.log('prod=>', this.products);



    let fetchXmlProducts = `<filter type="or">`
    for (let i = 0; i < this.products.length; i++) {
        const productId = this.products[i]['new_productid']
        fetchXmlProducts += `<condition attribute="new_productid" operator="eq" value="{${productId}}"  />`
    }
    fetchXmlProducts += `</filter>`
    formContext.getControl('new_fk_product').addCustomFilter(fetchXmlProducts, 'new_product')

}



// // Task 4
// // In the work order service entity, filter "Service" lookup to get only those products
// // where "type"="service"
// // done
