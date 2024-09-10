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
function filterContact(executionContext) {
    let formContext = executionContext.getFormContext()
    let customer = formContext.getAttribute('new_fk_customer').getValue()
    // console.log('customer', customer);


    if (contactLookupPointer != null) {
        formContext.getControl('new_fk_contact').removePreSearch(contactLookupPointer)
    }
    // customer = customer[0]?.id.replace('{', '').replace("}", '')
    contactLookupPointer = filterFunction.bind({ "customer": customer[0]?.id })

    formContext.getControl('new_fk_contact').addPreSearch(contactLookupPointer)
}

async function filterFunction(executionContext) {
    let formContext = executionContext.getFormContext()
    console.log(this.customer);


    let fetchXmlPosition = `
        <fetch version="1.0" mapping="logical" savedqueryid="43ab67d7-f840-4284-bfaf-cc1da35db8ef"
            no-lock="false" distinct="true">
            <entity name="new_my_positions">
                <attribute name="new_my_positionsid" />
                <filter type="and">
                    <condition attribute="new_fk_account" operator="eq"
                        value="${this.customer}" />
                </filter>
            </entity>
        </fetch>
    `
    fetchXmlPosition = "?fetchXml=" + encodeURIComponent(fetchXmlPosition)
    const assetPositions = await Xrm.WebApi.retrieveMultipleRecords('new_my_positions', fetchXmlPosition)
    const positions = assetPositions?.entities
    console.log(positions);
    for (let i = 0; i < positions.length; i++) {
        const el = positions[i];
        // I got positionIds but can't use
        let fetchXmlContacts = `
        
        `

    }
    formContext.getControl('new_fk_contact').addCustomFilter(fetchXmlContactFilter, 'new_my_contact')

}

// Task 3
// In the work order product entity, change places "Inventory" and "Product" fields.
// First should be "Inventory" and then "Product" in the forms. Write a JS to filter "Product"
// field based on the "Inventory" and to get only products where "type"="Product"

let productLookupPointer = null
function filterContact(executionContext) {
    let formContext = executionContext.getFormContext()
    let inventory = formContext.getAttribute('new_fk_inventory').getValue()
    // console.log('customer', customer);


    if (productLookupPointer != null) {
        formContext.getControl('new_fk_product').removePreSearch(productLookupPointer)
    }
    productLookupPointer = filterFunction.bind({ "inventory": inventory[0]?.id })

    formContext.getControl('new_fk_product').addPreSearch(productLookupPointer)
}

function filterFunction(executionContext) {
    let formContext = executionContext.getFormContext()
    // also with link-entity
    let fetchXml = `
                <fetch version="1.0" mapping="logical" savedqueryid="977564c7-43b4-4deb-a9f5-7e3c64b4a8d3"
                    no-lock="false" distinct="true">
                    <entity name="new_product">
                        <attribute name="statecode" />
                        <attribute name="new_name" />
                        <order attribute="new_name" descending="false" />
                        <attribute name="new_type" />
                        <attribute name="new_price_per_unit" />
                        <attribute name="new_productid" />
                        <filter type="and">
                            <condition attribute="new_type" operator="eq" value="100000000" />
                        </filter>
                        <link-entity name="new_inventory_product" alias="aa" link-type="inner" from="new_fk_product"
                            to="new_productid">
                            <filter type="and">
                                <condition attribute="new_fk_inventory" operator="eq"
                                    value="{028885cd-f45f-ef11-bfe2-000d3a48afd3}" uiname="Inventory 2"
                                    uitype="new_inventory" />
                            </filter>
                        </link-entity>
                    </entity>
                </fetch>
            `

    formContext.getControl('new_fk_product').addCustomFilter(fetchXml, 'new_product')

}



// Task 4
// In the work order service entity, filter "Service" lookup to get only those products
// where "type"="service"
// done
