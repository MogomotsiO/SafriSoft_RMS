var statementsDataTable = $("#statements-table").DataTable({
    "responsive": true,
    "autoWidth": false,
    "success": true,
    lengthMenu: [[20, 40, 60, -1], [20, 40, 60, "All"]],
    ajax: {
        url: '/api/Rental/GetStatements/',
        method: 'GET',
        "dataSrc": ""
    },
    'columns': [
        {
            'data': 'TenantName',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td id="' + data + '" >' + data + '</td>';
            }
        }, {
            'data': 'DateFrom',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td id="' + data + '" >' + data + '</td>';
            }
        },
        {
            'data': 'DateTo',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td id="' + data + '" >' + data + '</td>';
            }
        },
        {
            'data': 'Balance',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td class="text-right py-0 align-middle">' + new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data) + '</td>';
            }
        },
        {
            'data': 'TenantName',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"> <a id="' + 0 + '" class="btn btn-white" href="#" onclick="statementViewModal(' + full.TenantId + ',\'' + full.DateFrom + '\',\'' + full.DateTo + '\')"><i class="fas fa-file-invoice text-success"></i></a></div> </td>';
            }
        }
    ]
});

function sendTenantStatementEmail() {

    var sendStatementEmail = {
        TenantId: $('#tenantId').text(),
        DateFrom: $('#dateFrom').text(),
        DateTo: $('#dateTo').text(),
        OrganisationName: $('#organisation-name').text()
    };

    $.ajax({
        url: '/api/Rental/SendTenantStatementEmail',
        method: 'POST',
        dataType: 'json',
        data: sendStatementEmail,
        contextType: "application/json",
        traditional: true
    }).done(function (data) {
        if (data.Success == true) {
            toastr.success(data.Message);
        } else {
            toastr.error(data.Error);
        }
    });
}

function statementViewModal(tenantId, dateFrom, dateTo) {

    var statementDetails = {
        TenantId: tenantId,
        DateFrom: dateFrom,
        DateTo: dateTo
    }
    console.log(statementDetails);
    $.ajax({
        url: '/api/Rental/GetStatement',
        method: 'POST',
        dataType: 'json',
        data: statementDetails,
        contextType: "application/json",
        traditional: true
    }).done(function (data) {
        console.log(data);
        $('#logo').attr('src', data.organisation.OrganisationLogo);
        $('#organisation-name').text(data.organisation.OrganisationName);
        $('#organisation-email').text(data.organisation.OrganisationEmail);
        $('#organisation-cell').text(data.organisation.OrganisationCell);
        $('#organisation-street').text(data.organisation.OrganisationStreet);
        $('#organisation-suburb').text(data.organisation.OrganisationSuburb);
        $('#organisation-city').text(data.organisation.OrganisationCity);
        $('#organisation-code').text(data.organisation.OrganisationCode);
        $('#customer-name').text(data.tenant.TenantName);
        $('#customer-address').text(data.tenant.TenantAddress);
        $('#customer-email').text(data.tenant.TenantEmail);
        $('#customer-cell').text(data.tenant.TenantCell);
        var dateToday = new Date();
        var yearToday = dateToday.getFullYear();
        var monthToday = ('0' + (dateToday.getMonth() + 1)).slice(-2);
        var dayToday = ('0' + dateToday.getDate()).slice(-2);
        $('#order-date').text(dayToday + '-' + monthToday + '-' + yearToday);
        $('#unit-number').text(data.unit.UnitNumber);
        $('#client-reference').text(data.unit.UnitNumber);
        $('#vat-number').text(data.organisation.VATNumber);
        $('#tenantId').text(data.tenant.Id);
        $('#dateFrom').text(dateFrom);
        $('#dateTo').text(dateTo);
        //$('#table-order-id').text(data.OrderId);
        //$('#num-of-orders').text(data.NumberOfItems);
        //$('#product-name').text(data.ProductName);
        //$('#order-worth').text(new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data.OrderWorth));
        //$('#order-worth-snd').text(new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data.OrderWorth));
        $('#account-name').text(data.organisation.AccountName);
        $('#account-no').text(data.organisation.AccountNo);
        $('#bank-name').text(data.organisation.BankName);
        $('#bank-branch').text(data.organisation.BranchName);
        $('#branch-code').text(data.organisation.BranchCode);
        //$('#client-reference').text(data.ClientReference.toUpperCase() + "/" + data.OrderId.substr(1, data.OrderId.length));
        //$('#vat-amount').text(new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data.organisation.VatAmount));
        //$('#shipping-amount').text(new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data.ShippingCost));
        //$('#order-total').text(new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data.InvoiceTotal));
        $('#statement-transactions').text('');
        for (var i = 0; i < data.transactions.length; i++) {
            var date = new Date(data.transactions[i].TransactionDate);
            var year = date.getFullYear();
            var month = ('0' + (date.getMonth() + 1)).slice(-2);
            var day = ('0' + date.getDate()).slice(-2);
            $('#statement-transactions').append('<tr><td>' + year + '-' + month + '-' + day + '</td><td>' + data.transactions[i].TransactionName + '</td><td>' + new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data.transactions[i].TransactionAmount) + '</td></tr>')
        }

        $('#balance-bf').text(new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data.BalanceBF));
        $('#balance').text(new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data.Balance));
        $('#statement-view-modal').modal('show');
    });

}

// Hide premium Buttons
var packageId = $('#packageId').val();

if (packageId != 7 && packageId != 8) {
    $('#statement-pdf').hide();
    $('#send-pdf').hide();
    $('#statement-pdf-lock').show();
    $('#send-pdf-lock').show();
} else {
    $('#statement-pdf').show();
    $('#send-pdf').show();
    $('#statement-pdf-lock').hide();
    $('#send-pdf-lock').hide();
}