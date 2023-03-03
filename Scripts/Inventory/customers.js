//$(function () {
    var customerDataTable = $("#customer-table").DataTable({
        "responsive": true,
        "autoWidth": false,
        "info": true,
        lengthMenu: [[10, 20, 30, -1], [10, 20, 30, "All"]],
        ajax: {
            url: '/api/Inventory/GetCustomers/',
            method: 'GET',
            "dataSrc": ""
        },
        'columns': [
            {
                'data': 'Id',
                'searchable': true
            },
            {
                'data': 'CustomerName',
                'searchable': true,
                'render': function (data, type, full, meta) {
                    return '<td id="' + data + '" >' + data + '</td>';
                }
            },
            {
                'data': 'CustomerEmail',
                'searchable': true
            },
            {
                'data': 'CustomerCell',
                'searchable': true
            },
            {
                'data': 'CustomerAddress',
                'searchable': true
            },
            {
                'data': 'DateCustomerCreated',
                'searchable': true
            },
            {
                'data': 'NumberOfOrders',
                'searchable': true,
                'render': function (data, type, full, meta) {
                    return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"> <a id="' + full.Id + '" name="' + full.CustomerName + '" href="#" onclick="customerFeatures(this.id, this.name)" class="btn btn-white" href="#"><i class="fa fa-eye text-info"></i></a> <a class="btn btn-white" href="#">' + data + '</a> </div> </td>';
                }
            },
            {
                'data': 'Id',
                'render': function (data, type, full, meta) {
                    return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"> <a id="' + full.Id + '" class="btn btn-white" href="#" onclick="editCustomerDetails(this.id)"><i class="fas fa-folder-open text-info"></i></a> <a id="' + full.Id +'" class="btn btn-white" href="#" onclick="customerDeleteDetails(this.id)"><i class="fas fa-trash text-info"></i></a> </div> </td>'
                }
            }
        ]
    });
    
    var date = new Date();
    var createdDate = date.getDate() + "/" + (date.getMonth() + 1) + "/" + date.getFullYear();
    
    $('#CreateCustomer').on('click', function () {
        $('#creat-customer-modal').modal('show');
    });
    
    $('#customer-final-create').on('click', function () {
        var customerName = $("#customer-name").val();
        var customerEmail = $("#customer-email").val();
        var customerCell = $("#customer-cell").val();
        var customerAddress = $("#customer-address").val();
        var dateCustomerCreated = createdDate;
    
        if (customerName != "" && customerAddress != "" && (customerEmail != "" || customerCell != "")) {
    
            var customer = {
                'CustomerName': customerName,
                'CustomerEmail': customerEmail,
                'CustomerCell': customerCell,
                'CustomerAddress': customerAddress,
                'DateCustomerCreated': dateCustomerCreated
            };
    
            $.ajax({
                url: '/api/Inventory/CustomerCreate',
                method: 'POST',
                dataType: 'json',
                data: customer,
                contextType: "application/json",
                traditional: true
            }).done(function (data) {
                if (data.Success) {
                    console.log(data);
                    $('#creat-customer-modal').modal('hide');
                    toastr.success('Successfully created customer search customer table for confirmation!');
                    customerDataTable.ajax.reload();
                } else {
                    console.log(data);
                    toastr.error('An error occured while trying to save customer contact administrator!');
                }
    
            });
        } else {
            if (customerName == "") {
                toastr.error('Customer name cannot be empty!');
            }
            if (customerAddress == "") {
                toastr.error('Customer address cannot be empty!');
            }
            if (customerEmail == "" || customerCell == "") {
                toastr.error('Customer email or cell number cannot be empty!');
            }
        }
    
    });
    
    function customerDeleteDetails(id) {
        var customer = {
            'Id': id
        };

        $.ajax({
            url: '/api/Inventory/CustomerDelete',
            method: 'POST',
            dataType: 'json',
            data: customer,
            contextType: "application/json",
            traditional: true
        }).done(function (data) {
            if (data.Success) {
                toastr.success('Successfully deleted customer search customer table for confirmation!');
                customerDataTable.ajax.reload();
            } else {
                console.log(data);
                toastr.error('An error occured while trying to delete customer contact administrator!');
            }

        });
    }

    $('#customer-final-update').on('click', function () {
        var customerName = $("#edit-customer-name").val();
        var customerEmail = $("#edit-customer-email").val();
        var customerCell = $("#edit-customer-cell").val();
        var customerAddress = $("#edit-customer-address").val();
        var customerId = $("#edit-customer-id").val();

        if (customerName != "" && customerAddress != "" && (customerEmail != "" || customerCell != "")) {

            var customer = {
                'Id': customerId,
                'CustomerName': customerName,
                'CustomerEmail': customerEmail,
                'CustomerCell': customerCell,
                'CustomerAddress': customerAddress
            };

            $.ajax({
                url: '/api/Inventory/CustomerUpdate',
                method: 'POST',
                dataType: 'json',
                data: customer,
                contextType: "application/json",
                traditional: true
            }).done(function (data) {
                if (data.Success) {
                    console.log(data);
                    $('#edit-customer-modal').modal('hide');
                    toastr.success('Successfully updated customer search customer table for confirmation!');
                    customerDataTable.ajax.reload();
                } else {
                    console.log(data);
                    toastr.error('An error occured while trying to save customer contact administrator!');
                }

            });
        } else {
            if (customerName == "") {
                toastr.error('Customer name cannot be empty!');
            }
            if (customerAddress == "") {
                toastr.error('Customer address cannot be empty!');
            }
            if (customerEmail == "" || customerCell == "") {
                toastr.error('Customer email or cell number cannot be empty!');
            }
        }

    });


//});

var customerOrderDataTable = $("#customer-order-table").DataTable({
    "paging": false,
    "lengthChange": false,
    "searching": false,
    "ordering": false,
    "info": true,
    "autoWidth": false,
    "responsive": true,
    lengthMenu: [[10, 20, 30, -1], [10, 20, 30, "All"]],
    ajax: {
        url: '/api/Inventory/GetOrders/',
        method: 'GET',
        "dataSrc": ""
    },
    'columns': [
        {
            'data': 'OrderId',
            'searchable': true
        },
        {
            'data': 'ProductName',
            'searchable': true
        },
        {
            'data': 'ExpectedDeliveryDate',
            'searchable': true
        },
        {
            'data': 'OrderWorth',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data);
            }
        },
        {
            'data': 'OrderStatus',
            'searchable': true,
            'render': function (data, type, full, meta) {
                if (full.OrderStatus == "Processed") {
                    return '<span class="badge bg-danger">' + data + '</span>'
                }
                if (full.OrderStatus == "Packaged") {
                    return '<span class="badge bg-warning">' + data + '</span>'
                }
                if (full.OrderStatus == "InTransit") {
                    return '<span class="badge bg-info">' + data + '</span>'
                }
                if (full.OrderStatus == "Delivered") {
                    return '<span class="badge bg-success">' + data + '</span>'
                }
            }
        },
        {
            'data': 'OrderProgress',
            'searchable': true,
            'render': function (data, type, full, meta) {
                if (full.OrderStatus == "Processed") {
                    return '<div class="progress"> <div class="progress-bar progress-bar-striped bg-danger" style="width:' + data + '%"></div> </div><div class="progress"> <div class="progress-bar progress-bar-striped bg-danger" style="width:' + data + '%"></div> </div>'
                }
                if (full.OrderStatus == "Packaged") {
                    return '<div class="progress"> <div class="progress-bar progress-bar-striped bg-warning" style="width:' + data + '%"></div> </div><div class="progress"> <div class="progress-bar progress-bar-striped bg-warning" style="width:' + data + '%"></div> </div>'
                }
                if (full.OrderStatus == "InTransit") {
                    return '<div class="progress"> <div class="progress-bar progress-bar-striped bg-info" style="width:' + data + '%"></div> </div><div class="progress"> <div class="progress-bar progress-bar-striped bg-info" style="width:' + data + '%"></div> </div>'
                }
                if (full.OrderStatus == "Delivered") {
                    return '<div class="progress"> <div class="progress-bar progress-bar-striped bg-success" style="width:' + data + '%"></div> </div><div class="progress"> <div class="progress-bar progress-bar-striped bg-success" style="width:' + data + '%"></div> </div>'
                }
            }
        },
        {
            'data': 'OrderProgress',
            'searchable': true,
            'render': function (data, type, full, meta) {
                if (full.OrderStatus == "Processed") {
                    return '<span class="badge bg-danger">' + data + '%</span>'
                }
                if (full.OrderStatus == "Packaged") {
                    return '<span class="badge bg-warning">' + data + '%</span>'
                }
                if (full.OrderStatus == "InTransit") {
                    return '<span class="badge bg-info">' + data + '%</span>'
                }
                if (full.OrderStatus == "Delivered") {
                    return '<span class="badge bg-success">' + data + '%</span>'
                }
            }
        }
    ]
});

function customerFeatures(id,name) {
    customerOrderDataTable.ajax.url('/api/Inventory/GetOrders/' + id).load();
    $("#customer-orders-title").text(name + " Orders");
    $('#customer-orders').modal('show');
}


function editCustomerDetails(id) {
    
    $.ajax({
        url: '/api/Inventory/GetCustomer/' + id,
        method: 'GET',
        dataType: 'json'
    }).done(function (data) {
        console.log(data);
        $('#edit-customer-modal').modal('show');
        $('#edit-customer-name').val(data[0].CustomerName);
        $('#edit-customer-email').val(data[0].CustomerEmail);
        $('#edit-customer-cell').val(data[0].CustomerCell);
        $('#edit-customer-address').val(data[0].CustomerAddress);
        $('#edit-customer-id').val(id);

    });

}

