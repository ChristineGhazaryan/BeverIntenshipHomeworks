function openInventoryProductPopup(formContext) {
    let inventoryId = formContext.data.entity.getId()
    // or primaryitemId - in XrmToolBox


    let pageInput = {
        pageType: 'webresource',
        webresourceName: 'new_html_inventory_products_popup',
        data: JSON.stringify({ "inventoryId": inventoryId })
    };
    let navigationOptions = {
        target: 2,
        width: 400,
        height: 500,
        position: 1
    }
    Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(
        function success() {
            // code

        },
        function error() {
            Xrm.Navigation.openAlertDialog({ text: error.message })
        }
    )
}