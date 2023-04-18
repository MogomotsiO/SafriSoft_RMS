$("#tenant-reports-table").DataTable({
    "responsive": true, "lengthChange": false, "autoWidth": false,
    dom: '<"top"<"left-col"B><"center-col"l><"right-col"f>>rtip',
    "buttons": ["csv", "excel"],
    ajax: {
        url: '/api/Report/GetTenants/',
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
        },
        {
            'data': 'TenantEmail',
            'searchable': true
        },
        {
            'data': 'TenantCell',
            'searchable': true
        },
        {
            'data': 'TenantAddress',
            'searchable': true
        },
        {
            'data': 'TenantWorkAddress',
            'searchable': true
        },
        {
            'data': 'TenantWorkCell',
            'searchable': true
        },
        {
            'data': 'DateTenantCreated',
            'searchable': true
        },
        {
            'data': 'DateLeaseStart',
            'searchable': true
        },
        {
            'data': 'DateLeaseEnd',
            'searchable': true
        }
    ]
});

$("#unit-reports-table").DataTable({
    "responsive": true, "lengthChange": false, "autoWidth": false,
    dom: '<"top"<"left-col"B><"center-col"l><"right-col"f>>rtip',
    "buttons": ["csv", "excel"],
    ajax: {
        url: '/api/Report/GetUnits/',
        method: 'GET',
        "dataSrc": ""
    },
    'columns': [
        {
            'data': 'UnitNumber',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td id="' + data + '" >' + data + '</td>';
            }
        },
        {
            'data': 'UnitName',
            'searchable': true
        },
        {
            'data': 'UnitDescription',
            'searchable': true
        },
        {
            'data': 'UnitRooms',
            'searchable': true
        },
        {
            'data': 'UnitSharing',
            'searchable': true
        },
        {
            'data': 'UnitPrice',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data);
            }
        },
        {
            'data': 'TenantName',
            'searchable': true
        }
    ]
});

$("#expected-reports-table").DataTable({
    "responsive": true, "lengthChange": false, "autoWidth": false,
    dom: '<"top"<"left-col"B><"center-col"l><"right-col"f>>rtip',
    "buttons": ["csv", "excel"],
    ajax: {
        url: '/api/Report/GetExpectedTransactions/',
        method: 'GET',
        "dataSrc": ""
    },
    'columns': [
        {
            'data': 'TransactionDate',
            'searchable': true
        },
        {
            'data': 'Tenant',
            'searchable': true
        },
        {
            'data': 'TransactionCode',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td id="' + data + '" >' + data + '</td>';
            }
        },
        {
            'data': 'TransactionName',
            'searchable': true
        },
        {
            'data': 'TransactionAmount',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data);
            }
        }

    ]
});


$("#received-reports-table").DataTable({
    "responsive": true, "lengthChange": false, "autoWidth": false,
    dom: '<"top"<"left-col"B><"center-col"l><"right-col"f>>rtip',
    "buttons": ["csv", "excel"],
    ajax: {
        url: '/api/Report/GetReceivedTransactions/',
        method: 'GET',
        "dataSrc": ""
    },
    'columns': [
        {
            'data': 'TransactionDate',
            'searchable': true
        },
        {
            'data': 'Tenant',
            'searchable': true
        },
        {
            'data': 'TransactionCode',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td id="' + data + '" >' + data + '</td>';
            }
        },
        {
            'data': 'TransactionName',
            'searchable': true
        },
        {
            'data': 'TransactionAmount',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data);
            }
        }

    ]
});

// Hide premium Buttons
var packageId = $('#packageId').val();

if (packageId != 7 && packageId != 8) {
    $('.dt-buttons').hide();
    $('#excel-download-lock').show();
    $('#excel-download-lock-2').show();
    $('#excel-download-lock-3').show();
    $('#excel-download-lock-4').show();
} else {
    $('.dt-buttons').show();
    $('#excel-download-lock').hide();
    $('#excel-download-lock-2').hide();
    $('#excel-download-lock-3').hide();
    $('#excel-download-lock-4').hide();
}