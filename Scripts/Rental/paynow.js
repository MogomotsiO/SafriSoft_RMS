var transactionData = {
    'TransactionFor': $('#transaction-for').text(),
    'DateTransactionRaised': $('#transaction-date').text()
}
$.ajax({
    url: '/api/Rental/GetPayNowInvoiceData',
    method: 'POST',
    dataType: 'json',
    data: transactionData
}).done(function (data) {
    console.log(data);
    $('#logo').attr('src', data[0].OrganisationLogo);
    $('#organisation-name').text(data[0].OrganisationName);
    $('#organisation-email').text(data[0].OrganisationEmail);
    $('#organisation-cell').text(data[0].OrganisationCell);
    $('#organisation-street').text(data[0].OrganisationStreet);
    $('#organisation-suburb').text(data[0].OrganisationSuburb);
    $('#organisation-city').text(data[0].OrganisationCity);
    $('#organisation-code').text(data[0].OrganisationCode);
    $('#customer-name').text(data[0].CustomerName);
    $('#customer-address').text(data[0].CustomerAddress);
    $('#customer-email').text(data[0].CustomerEmail);
    $('#customer-cell').text(data[0].CustomerCell);
    $('#order-date').text(data[0].DateOrderCreated);
    $('#order-id').text(data[0].OrderId);
    $('#vat-number').text(data[0].VATNumber);
    $('#table-order-id').text(data[0].OrderId);
    $('#num-of-orders').text(data[0].NumberOfItems);
    $('#product-name').text(data[0].ProductName);
    $('#order-worth').text(new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data[0].OrderWorth));
    $('#order-worth-snd').text(new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data[0].OrderWorth));
    $('#account-name').text(data[0].AccountName);
    $('#account-no').text(data[0].AccountNo);
    $('#bank-name').text(data[0].BankName);
    $('#bank-branch').text(data[0].BranchName);
    $('#branch-code').text(data[0].BranchCode);
    $('#client-reference').text(data[0].ClientReference.toUpperCase() + "/" + data[0].OrderId.substr(1, data[0].OrderId.length));
    $('#vat-amount').text(new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data[0].VatAmount));
    $('#shipping-amount').text(new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data[0].ShippingCost));
    $('#order-total').text(new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data[0].InvoiceTotal));
    $('#invoice-view-modal').modal('show');
});