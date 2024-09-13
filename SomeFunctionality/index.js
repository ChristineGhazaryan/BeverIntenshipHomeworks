function fullname(executionContext) {
    let formContext = executionContext.getFormContext()
    let firstname = formContext.getAttribute('new_slot_firstname').getValue()
    let lastname = formContext.getAttribute('new_slot_lastname').getValue()
    if (!firstname) {
        firstname = ''
    } else if (!lastname) {
        lastname = ''
    }
    let fullname = `${firstname} ${lastname}`
    formContext.getAttribute('new_name').setValue(fullname)
}

async function totalAmount(executionContext) {
    let formContext = executionContext.getFormContext()
    let price_per_unit = formContext.getAttribute('new_fk_price_per_unit').getValue()
    let quantity = formContext.getAttribute('new_int_quantity').getValue()
    if (!price_per_unit) {
        let productId = formContext.getAttribute('new_fk_product').getValue()

        let fetchXml = `
                <fetch version="1.0" mapping="logical" savedqueryid="977564c7-43b4-4deb-a9f5-7e3c64b4a8d3"
                    no-lock="false" distinct="true">
                    <entity name="new_product">
                        <attribute name="new_price_per_unit" />
                        <attribute name="new_productid" />
                        <filter type="and">
                            <condition attribute="new_productid" operator="eq" value="${productId[0]?.id}"/>
                        </filter>
                    </entity>
                </fetch>
                `
        fetchXml = "?fetchXml=" + encodeURIComponent(fetchXml)
        const asset = await Xrm.WebApi.retrieveMultipleRecords('new_product', fetchXml)
        if (asset.entities?.length) {
            price_per_unit = asset.entities[0]['new_price_per_unit']
            formContext.getAttribute('new_fk_price_per_unit').setValue(price_per_unit)
        }

    }
    let total = price_per_unit * quantity
    formContext.getAttribute('new_mon_total_amount').setValue(total)
}


function totalAmountForService(executionContext) {
    let formContext = executionContext.getFormContext()
    let price_per_unit = formContext.getAttribute('new_mon_price_per_unit').getValue()
    let duration = formContext.getAttribute('new_whole_duration').getValue()
    let total = (price_per_unit * duration) / 60
    formContext.getAttribute('new_mon_total_amount').setValue(total)
}