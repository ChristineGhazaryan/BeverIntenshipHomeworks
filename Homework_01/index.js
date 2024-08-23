// Task 1
// using onLoad event
function hideField(executionContext) {
    let formContext = executionContext.getFormContext();
    let fieldControl = formContext.getControl("new_name");

    if (fieldControl != null) {
        // non-mandatory
        formContext.getAttribute('new_name').setRequiredLevel('none')
        // hide field
        fieldControl.setVisible(false);
    }
}

// ---------------------------------------------------------------------------
// Task 2
function changeProductName(executionContext) {
    const formContext = executionContext.getFormContext();
    const product_name = formContext.getAttribute('new_fk_product').getValue()

    formContext.getAttribute('new_name').setValue(product_name?.[0]?.name)
}

// ---------------------------------------------------------------------------
// Task 3
function hidePricePerUnit(executionContext) {
    const formContext = executionContext.getFormContext();
    const type = formContext.getAttribute('new_type').getSelectedOption()
    const price_per_unit_control = formContext.getControl("new_price_per_unit");

    if (type?.text == 'Product') {
        price_per_unit_control.setVisible(true)

    } else if (type?.text == 'Service') {
        price_per_unit_control.setVisible(false)
    }
}

// ---------------------------------------------------------------------------
// Task 4
function calculateTotalAmount(executionContext) {
    const formContext = executionContext.getFormContext();
    formContext.getControl("new_total_amount").setDisabled(true)

    const quantity = formContext.getAttribute('new_int_quantity').getValue()
    const price_per_unit = formContext.getAttribute('new_price_per_unit').getValue()

    const result = quantity * price_per_unit
    formContext.getAttribute('new_total_amount').setValue(result)
}

// ---------------------------------------------------------------------------
// Task 5
function hideAllFieldsDependsOnType(executionContext) {
    const formContext = executionContext.getFormContext();
    const attributes = formContext.data.entity.attributes
    const type = formContext.ui.getFormType()
    let temp = true

    // 2 - update
    // 1 - create
    if (type == 1) {
        temp = false
    } else if (type == 2) {
        temp = true
    }

    attributes.forEach((attr) => {
        const field = formContext.getControl(attr.getName())
        field.setDisabled(temp)
    })

}
