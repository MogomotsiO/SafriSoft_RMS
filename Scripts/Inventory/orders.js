var orderDataTable = $("#order-table").DataTable({
    "responsive": true,
    "autoWidth": false,
    "info": true,
    lengthMenu: [[10, 20, 30, -1], [10, 20, 30, "All"]],
    "order": [[9, "asc"]],
    ajax: {
        url: '/api/Inventory/GetOrders/',
        method: 'GET',
        "dataSrc": ""
    },
    'columns': [
        {
            'data': 'OrderId',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"> <a id="' + full.OrderId + '" href="#" onclick="viewOrder(this.id)" class="btn btn-white" href="#"><i class="fa fa-eye text-info"></i></a> <a id="' + full.OrderId + '" class="btn btn-white" href="#" onclick="invoiceViewModal(this.id)"><i class="fas fa-file-invoice text-info"></i></a></div> </td>';
            }
        },
        {
            'data': 'OrderId',
            'searchable': true
        },
        {
            'data': 'ProductName',
            'searchable': true
        },
        {
            'data': 'NumberOfItems',
            'searchable': true
        },
        {
            'data': 'CustomerName',
            'searchable': true
        },
        {
            'data': 'DateOrderCreated',
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
                if (data == "Processed") {
                    return '<div class="btn-group" style="width:100%;margin:0px !important;"> <button id="btn-' + data + '" class="btn btn-danger" type="button">' + data + '</button> <button id="dropdown-' + full.OrderId.replace('#', ' ').trim() + '" class="btn btn-danger dropdown-toggle" type="button" data-toggle="dropdown"> <span class="sr-only">Toggle Dropdown</span> <div class="dropdown-menu" role="menu"> <a name="Packaged" class="dropdown-item" href="#" id="Packaged-' + full.OrderId.replace('#', ' ').trim() + '" onclick="changeStatus(this.id)">Packaged</a><a name="InTransit" class="dropdown-item" href="#" id="InTransit-' + full.OrderId.replace('#', ' ').trim() + '" onclick="changeStatus(this.id)">In Transit</a><a name="Delivered" class="dropdown-item" href="#" id="Delivered-' + full.OrderId.replace('#', ' ').trim() + '" onclick="changeStatus(this.id)">Delivered</a> </div> </button><button hidden id="wait-' + full.OrderId.replace('#', ' ').trim() + '" type="button" class="btn btn-danger"><i class="fas fa-spinner fa-pulse text-white"></i> </button> </div>'
                }
                if (data == "Packaged") {
                    return '<div class="btn-group" style="width:100%;margin:0px !important;"> <button id="btn-' + data + '" class="btn btn-warning" type="button">' + data + '</button> <button id="dropdown-' + full.OrderId.replace('#', ' ').trim() + '" class="btn btn-warning dropdown-toggle" type="button" data-toggle="dropdown"> <span class="sr-only">Toggle Dropdown</span> <div class="dropdown-menu" role="menu"> <a name="Packaged" class="dropdown-item" href="#" id="Packaged-' + full.OrderId.replace('#', ' ').trim() + '" onclick="changeStatus(this.id)">Packaged</a><a name="InTransit" class="dropdown-item" href="#" id="InTransit-' + full.OrderId.replace('#', ' ').trim() + '" onclick="changeStatus(this.id)">In Transit</a><a name="Delivered" class="dropdown-item" href="#" id="Delivered-' + full.OrderId.replace('#', ' ').trim() + '" onclick="changeStatus(this.id)">Delivered</a> </div> </button><button hidden id="wait-' + full.OrderId.replace('#', ' ').trim() + '" type="button" class="btn btn-warning"><i class="fas fa-spinner fa-pulse text-white"></i> </button> </div>'
                }
                if (data == "InTransit") {
                    return '<div class="btn-group" style="width:100%;margin:0px !important;"> <button id="btn-' + data + '" class="btn btn-info" type="button">' + data + '</button> <button id="dropdown-' + full.OrderId.replace('#', ' ').trim() + '" class="btn btn-info dropdown-toggle" type="button" data-toggle="dropdown"> <span class="sr-only">Toggle Dropdown</span> <div class="dropdown-menu" role="menu"> <a name="Packaged" class="dropdown-item" href="#" id="Packaged-' + full.OrderId.replace('#', ' ').trim() + '" onclick="changeStatus(this.id)">Packaged</a><a name="InTransit" class="dropdown-item" href="#" id="InTransit-' + full.OrderId.replace('#', ' ').trim() + '" onclick="changeStatus(this.id)">In Transit</a><a name="Delivered" class="dropdown-item" href="#" id="Delivered-' + full.OrderId.replace('#', ' ').trim() + '" onclick="changeStatus(this.id)">Delivered</a> </div> </button><button hidden id="wait-' + full.OrderId.replace('#', ' ').trim() + '" type="button" class="btn btn-info"><i class="fas fa-spinner fa-pulse text-white"></i> </button> </div>'
                }
                if (data == "Delivered") {
                    return '<div class="btn-group" style="width:100%;margin:0px !important;"> <button id="btn-' + data + '" class="btn btn-success" type="button">' + data + '</button> <button id="dropdown-' + full.OrderId.replace('#', ' ').trim() + '" class="btn btn-success dropdown-toggle" type="button" data-toggle="dropdown"> <span class="sr-only">Toggle Dropdown</span> <div class="dropdown-menu" role="menu"> <a name="Packaged" class="dropdown-item" href="#" id="Packaged-' + full.OrderId.replace('#', ' ').trim() + '" onclick="changeStatus(this.id)">Packaged</a><a name="InTransit" class="dropdown-item" href="#" id="InTransit-' + full.OrderId.replace('#', ' ').trim() + '" onclick="changeStatus(this.id)">In Transit</a><a name="Delivered" class="dropdown-item" href="#" id="Delivered-' + full.OrderId.replace('#', ' ').trim() + '" onclick="changeStatus(this.id)">Delivered</a> </div> </button><button hidden id="wait-' + full.OrderId.replace('#', ' ').trim() + '" type="button" class="btn btn-success"><i class="fas fa-spinner fa-pulse text-white"></i> </button> </div>'
                }                
            }
        },
        {
            'data': 'OrderProgress',
            'searchable': true,
            'render': function (data, type, full, meta) {
                if (full.OrderStatus == "Processed") {
                    return '<div class="progress"> <div class="progress-bar progress-bar-striped bg-danger" style="width:' + data + '%"></div> </div><div class="progress"> <div class="progress-bar progress-bar-striped bg-danger" style="width:' + data + '%"></div> </div><div class="progress progress-xxs"> <div class="progress-bar progress-bar-striped bg-danger" style="width:' + data + '%"></div> </div><div class="progress progress-xxs"> <div class="progress-bar progress-bar-striped bg-danger" style="width:' + data + '%"></div> </div>'
                }
                if (full.OrderStatus == "Packaged") {
                    return '<div class="progress"> <div class="progress-bar progress-bar-striped bg-warning" style="width:' + data + '%"></div> </div><div class="progress"> <div class="progress-bar progress-bar-striped bg-warning" style="width:' + data + '%"></div> </div><div class="progress progress-xxs"> <div class="progress-bar progress-bar-striped bg-warning" style="width:' + data + '%"></div> </div><div class="progress progress-xxs"> <div class="progress-bar progress-bar-striped bg-warning" style="width:' + data + '%"></div> </div>'
                }
                if (full.OrderStatus == "InTransit") {
                    return '<div class="progress"> <div class="progress-bar progress-bar-striped bg-info" style="width:' + data + '%"></div> </div><div class="progress"> <div class="progress-bar progress-bar-striped bg-info" style="width:' + data + '%"></div> </div><div class="progress progress-xxs"> <div class="progress-bar progress-bar-striped bg-info" style="width:' + data + '%"></div> </div><div class="progress progress-xxs"> <div class="progress-bar progress-bar-striped bg-info" style="width:' + data + '%"></div> </div>'
                }
                if (full.OrderStatus == "Delivered") {
                    return '<div class="progress"> <div class="progress-bar progress-bar-striped bg-success" style="width:' + data + '%"></div> </div><div class="progress"> <div class="progress-bar progress-bar-striped bg-success" style="width:' + data + '%"></div> </div><div class="progress progress-xxs"> <div class="progress-bar progress-bar-striped bg-success" style="width:' + data + '%"></div> </div><div class="progress progress-xxs"> <div class="progress-bar progress-bar-striped bg-success" style="width:' + data + '%"></div> </div>'
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
        },
        {
            'data': 'OrderId',
            'render': function (data, type, full, meta) {
                return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"> <a id="' + full.OrderId + '" class="btn btn-white" href="#" onclick="orderDeleteDetails(this.id)"><i class="fas fa-trash text-info"></i></a> </div> </td>'
            }
        }
    ]
});

$('#order-create-view').on('click', function () {
    $('#customer-orders').modal('hide');
    $('#creat-order-modal').modal('show');
});

function populateCustomer(value) {
    $("#customer-name-input").val($("#" + value).attr('name'));
    $("#customer-name-input").attr('name', value.charAt(0, "-"));
}

function populateProduct(value) {
    $("#product-name-input").val($("#" + value).attr('name'));
    $("#product-name-input").attr('name', value);
}

function addDays(date, days) {
    var result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
}


$('#order-final-wait').hide();
$('#order-final-create').on('click', function () {
    var customerId = $("#customer-name-input").attr('name');
    var customerName = $("#customer-name-input").val();
    var productName = $("#product-name-input").val();
    var productReference = $("#product-name-input").attr('name');
    var numberOfItems = $("#number-of-items").val();
    var numberOfDays = $("#number-of-days").val();
    var shippingCost = $("#shipping-cost").val() != "" ? $("#shipping-cost").val() : 0;
    var dateOrderCreated = new Date();
    var orderCreatedDate = dateOrderCreated.getDate() + "/" + (dateOrderCreated.getMonth() + 1) + "/" + dateOrderCreated.getFullYear();
    var deliveryDate = addDays(dateOrderCreated, 5);
    var finalDeliveryDate = deliveryDate.getDate() + "/" + (deliveryDate.getMonth() + 1) + "/" + deliveryDate.getFullYear();

    if (customerName != "" && productName != "" && numberOfItems != "" && numberOfDays != "") {

        var order = {
            'CustomerId': customerId,
            'CustomerName': customerName,
            'ProductName': productName,
            'ProductReference': productReference,
            'NumberOfItems': numberOfItems,
            'DateOrderCreated': orderCreatedDate,
            'ExpectedDeliveryDate': finalDeliveryDate,
            'ShippingCost': shippingCost
        };
        $('#order-final-create').hide();
        $('#order-final-wait').show();
        $.ajax({
            url: '/api/Inventory/OrderCreate',
            method: 'POST',
            dataType: 'json',
            data: order,
            contextType: "application/json",
            traditional: true
        }).done(function (data) {
            if (data.Success) {
                console.log(data);
                $('#creat-order-modal').modal('hide');
                $('#customer-orders').modal('show');
                customerOrderDataTable.ajax.url('/api/Inventory/GetOrders/' + data.CustomerID).load();
                orderDataTable.ajax.reload();
                toastr.success('Successfully created order search order table for confirmation!');
            } else {
                console.log(data);
                toastr.error(data.Error);
            }
            $('#order-final-create').show();
            $('#order-final-wait').hide();
        });
        
    } else {
        toastr.error('All fields must be populated!');
    }



});

function changeStatus(value) {
    var orderStatus = $('#' + value).attr('name');
    var percentage;
    if (orderStatus == "Packaged") {
        percentage = 50;
    }
    if (orderStatus == "InTransit") {
        percentage = 75;
    }
    if (orderStatus == "Delivered") {
        percentage = 100;
    }

    var indexOfVal = value.indexOf("-") + 1;
    var finalOrderId = '#' + value.substr(indexOfVal);
    $('#wait-' + value.substr(indexOfVal)).removeAttr('hidden');
    $('#dropdown-' + value.substr(indexOfVal)).attr("hidden", true);
    var status = { 'OrderProgress': percentage, 'OrderStatus': orderStatus, 'OrderId': finalOrderId };
    $.ajax({
        url: '/api/Inventory/ChangeStatus',
        method: 'POST',
        dataType: 'json',
        data: status,
        contextType: "application/json",
        traditional: true
    }).done(function (data) {
        if (data.Success) {
            console.log(data);
            orderDataTable.ajax.reload();
        } else {
            console.log(data);
            toastr.error('An error occured while trying to save customer contact administrator!');
        }
        $('#dropdown-' + value.substr(indexOfVal)).removeAttr('hidden');
        $('#wait-' + value.substr(indexOfVal)).attr("hidden", true);
    });
}

function orderDeleteDetails(id) {
    var order = {
        'OrderId': id
    };

    $.ajax({
        url: '/api/Inventory/OrderDelete',
        method: 'POST',
        dataType: 'json',
        data: order,
        contextType: "application/json",
        traditional: true
    }).done(function (data) {
        if (data.Success) {
            toastr.success('Successfully deleted order search order table for confirmation!');
            orderDataTable.ajax.reload();
        } else {
            console.log(data);
            toastr.error('An error occured while trying to delete order contact administrator!');
        }

    });
}

function viewOrder(id) {
    GetOrderAudit(id);
    $('#view-order').modal('show');
}

function invoiceViewModal(id) {
    var orderData = {
        'OrderId': id
    }
    $.ajax({
        url: '/api/Inventory/GetInvoiceData',
        method: 'POST',
        dataType: 'json',
        data: orderData
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
    
}

