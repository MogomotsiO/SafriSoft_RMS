var projectionDataTable = $("#projection-table").DataTable({
    "responsive": true,
    "autoWidth": false,
    "success": true,
    lengthMenu: [[20, 40, 60, -1], [20, 40, 60, "All"]],
    ajax: {
        url: '/api/Rental/GetProjections/',
        method: 'GET',
        "dataSrc": ""
    },
    'columns': [
        {
            'data': 'TransDate',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td id="' + data + '" >' + data + '</td>';
            }
        }, {
            'data': 'TenantName',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td id="' + data + '" >' + data + '</td>';
            }
        },
        {
            'data': 'TransName',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td id="' + data + '" >' + data + '</td>';
            }
        },
        {
            'className': 'bg-warning text-right',
            'data': 'TransProposed',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td class="text-right py-0 align-middle">' + new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data) + '</td>';
            }
        },
        {
            'className': 'bg-info text-right',
            'data': 'TransActual',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td class="text-right py-0 align-middle">' + new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data) + '</td>';
            }
        },
        {
            'className': 'bg-success text-right',
            'data': 'Balance',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td class="text-right py-0 align-middle">' + new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data) + '</td>';
            }
        }
    ]
});

$('#RaiseTransactions').on('click', function () {
    $.ajax({
        url: '/api/Rental/GetTenants',
        method: 'GET',
        dataType: 'json'
    }).done(function (data) {
        console.log(data);
        var select = document.getElementById("transaction-for");
        select.innerHTML = "";

        var initialOption = document.createElement("option");
        initialOption.value = "0";
        initialOption.text = "All Tenants";
        select.add(initialOption);

        for (var i = 0; i < data.length; i++) {
            var option = document.createElement("option");
            option.value = data[i].Id;
            option.text = data[i].TenantName;
            select.add(option);
        }
    });
    $('#create-transaction-modal').modal('show');
});

$('#payment-final-wait').hide();
$('#RaisePayments').on('click', function () {
    $.ajax({
        url: '/api/Rental/GetTenants',
        method: 'GET',
        dataType: 'json'
    }).done(function (data) {
        console.log(data);
        var select = document.getElementById("payment-for");
        select.innerHTML = "";

        var initialOption = document.createElement("option");
        initialOption.value = "0";
        initialOption.text = "Select Tenant";
        select.add(initialOption);

        for (var i = 0; i < data.length; i++) {
            var option = document.createElement("option");
            option.value = data[i].Id;
            option.text = data[i].TenantName;
            select.add(option);
        }
    });
    $('#create-payment-modal').modal('show');
});

$('#transaction-final-wait').hide();
$('#transaction-final-create').on('click', function () {
    var transactionFor = $('#transaction-for').val();
    var dateTransactionRaised = $('#date-transaction-raised').val();

    var transaction = {
        'TransactionFor': transactionFor,
        'DateTransactionRaised': dateTransactionRaised
    };
    $('#transaction-final-create').hide();
    $('#transaction-final-wait').show();
    $.ajax({
        url: '/api/Rental/TransactionCreate',
        method: 'POST',
        dataType: 'json',
        data: transaction,
        contextType: "application/json",
        traditional: true
    }).done(function (data) {
        if (data.Success) {
            console.log(data);
            $('#create-transaction-modal').modal('hide');
            toastr.success('Successfully created transaction search transaction table for confirmation!');
            projectionDataTable.ajax.reload();
            
        } else {
            console.log(data);
            toastr.error('An error occured while trying to save tenant contact administrator! ' + data.Message);
        }

        $('#transaction-final-create').show();
        $('#transaction-final-wait').hide();

    });
    
});

$('#payment-final-create').on('click', function () {
    var paymentFor = $('#payment-for').val();
    var paymentAmount = $('#payment-amount').val();
    var datePaymentRaised = $('#date-payment-raised').val();

    if (paymentFor == 0) {
        toastr.error('Please select a Tenant');
        return;
    }

    var transaction = {
        'TransactionFor': paymentFor,
        'TransactionAmount': paymentAmount,
        'DateTransactionRaised': datePaymentRaised
    };
    $('#payment-final-create').hide();
    $('#payment-final-wait').show();
    $.ajax({
        url: '/api/Rental/PaymentTransactionCreate',
        method: 'POST',
        dataType: 'json',
        data: transaction,
        contextType: "application/json",
        traditional: true
    }).done(function (data) {
        if (data.Success) {
            console.log(data);
            $('#create-payment-modal').modal('hide');
            toastr.success('Successfully created transaction search transaction table for confirmation!');
            projectionDataTable.ajax.reload();

        } else {
            console.log(data);
            toastr.error('An error occured while trying to create a transaction contact administrator! ' + data.Message);
        }

        $('#payment-final-create').show();
        $('#payment-final-wait').hide();

    });

});