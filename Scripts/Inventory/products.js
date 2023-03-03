//$(function () {
    var productDataTable = $("#products-table").DataTable({
        "responsive": true,
        "autoWidth": false,
        "info": true,
        lengthMenu: [[10, 20, 30, -1], [10, 20, 30, "All"]],
        ajax: {
            url: '/api/Inventory/GetProducts/',
            method: 'GET',
            "dataSrc": ""
        },
        'columns': [
            {
                'data': 'Id',
                'searchable': true
            },
            {
                'data': 'ProductImage',
                'render': function (data, type, full, meta) {
                    if (data.includes(".jpg") || data.includes(".png") || data.includes(".jpeg")) {
                        return '<td> <img src="../../assets/img/' + data + '" alt="No Image" height="50" width="50" /></td>'
                    } else {
                        return '<td> <img src="' + data + '" height="110" width="150" style="border-radius:2px" /></td>'
                    }
                    
                }
            },
            {
                'data': 'ProductCode',
                'searchable': true
            },
            {
                'data': 'ProductCategory',
                'searchable': true
            },
            {
                'data': 'ProductName',
                'searchable': true
            },
            {
                'data': 'ProductReference',
                'searchable': true
            },
            {
                'data': 'SellingPrice',
                'searchable': true,
                'render': function (data, type, full, meta) {
                    return new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data);
                }
            },
            {
                'data': 'ItemsSold',
                'searchable': true
            },
            {
                'data': 'ItemsAvailable',
                'searchable': true
            },
            {
                'data': 'Id',
                'render': function (data, type, full, meta) {
                    return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"> <a id="' + full.Id + '" class="btn btn-white" href="#" onclick="editProductDetails(this.id)"><i class="fas fa-folder-open text-info"></i></a> <a id="' + full.Id +'" class="btn btn-white" href="#" onclick="productDeleteDetails(this.id)"><i class="fas fa-trash text-info"></i></a> </div> </td>'
                }
            }
        ]
    });

    var date = new Date();
    var productDate = date.getFullYear() + "/" + (date.getMonth() + 1) + "/" + date.getDate();

    $('#CreateProduct').on('click', function () {
        $('#logo').attr('src', "../../assets/img/iconOnly.png");
        $('#creat-product-modal').modal('show');
        $('#product-reference').val(productDate);
    });

    $("#product-code").on("input", function () {
        $('#product-reference').val($(this).val() + "-" + productDate);
    });

    $('#product-final-create').on('click', function () {
        var productCode = $("#product-code").val();
        var productCategory = $("#product-category").val();
        var productName = $("#product-name").val();
        var productReference = $("#product-reference").val();
        var sellingPriceInput = $("#product-price").val();
        var itemsAvailable = $("#product-available").val();
        var productImage = $('#img-source').val();
        var itemsSold = "0";
        var sellingPrice = 0;

        if (sellingPriceInput != 0) {
            sellingPrice = sellingPriceInput;
        }       

        if (productName != "" && itemsAvailable != 0 && productCode != "" && productImage != "") {

            var product = {
                'ProductCode': productCode,
                'ProductCategory': productCategory,
                'ProductName': productName,
                'ProductReference': productReference,
                'SellingPrice': sellingPrice,
                'ItemsAvailable': itemsAvailable,
                'ItemsSold': itemsSold,
                'ProductImage': productImage
            };

            $.ajax({
                url: '/api/Inventory/ProductCreate',
                method: 'POST',
                dataType: 'json',
                data: product,
                contextType: "application/json",
                traditional: true
            }).fail(function (data) {
                toastr.error('Product already exists!');
            }).done(function (data) {
                if (data.Success) {
                    console.log(data);
                    $('#creat-product-modal').modal('hide');
                    toastr.success('Successfully created product search product table for confirmation!');
                    productDataTable.ajax.reload();
                } else {
                    console.log(data);
                    toastr.error('An error occured while trying to save product contact administrator!');
                }

            });
        } else {
            if (productName == "") {
                toastr.error('Product name cannot be empty!');
            }else if (itemsAvailable == "") {
                toastr.error('Items available cannot be empty!');
            }else if (productCode == "") {
                toastr.error('Product Code cannot be empty!');
            } else if (productImage == "") {
                toastr.error('Please select an image for your product!');
            }
        }

    });

    // Product
    //      Edit 
    //          Stuff
    //              For Some Reason
    //

    $('#product-final-update').on('click', function () {
        var productId = $("#edit-product-id").val();
        var productName = $("#edit-product-name").val();
        var productReference = $("#edit-product-reference").val();
        var sellingPriceInput = $("#edit-product-price").val();
        var itemsAvailable = $("#edit-product-available").val();
        var productCode = $("#edit-product-code").val();
        var productCategory = $("#edit-product-category").val();
        var productImage = $("#edit-logo").attr('src');
        var sellingPrice = 0;

        if (sellingPriceInput != 0) {
            sellingPrice = sellingPriceInput;
        }

        if (productName != "" && itemsAvailable != 0 && productCode != "" && productImage != "") {

            var product = {
                'Id': productId,
                'ProductName': productName,
                'ProductReference': productReference,
                'SellingPrice': sellingPrice,
                'ItemsAvailable': itemsAvailable,
                'ProductCode': productCode,
                'ProductCategory': productCategory,
                'ProductImage': productImage
            };

            console.log(product);

            $.ajax({
                url: '/api/Inventory/ProductUpdate',
                method: 'POST',
                dataType: 'json',
                data: product,
                contextType: "application/json",
                traditional: true
            }).done(function (data) {
                if (data.Success) {
                    console.log(data);
                    $('#edit-product-modal').modal('hide');
                    toastr.success('Successfully updated product search product table for confirmation!');
                    productDataTable.ajax.reload();
                } else {
                    console.log(data);
                    toastr.error('An error occured while trying to save product contact administrator!');
                }

            });
        } else {
            if (productName == "") {
                toastr.error('Product name cannot be empty!');
            } else if (itemsAvailable == "") {
                toastr.error('Items available cannot be empty!');
            } else if (productCode == "") {
                toastr.error('Product Code cannot be empty!');
            } else if (productImage == "") {
                toastr.error('Please select an image for your product!');
            }
        }

    });

    function productDeleteDetails(id) {
        var product = {
            'Id': id
        };

        $.ajax({
            url: '/api/Inventory/ProductDelete',
            method: 'POST',
            dataType: 'json',
            data: product,
            contextType: "application/json",
            traditional: true
        }).done(function (data) {
            if (data.Success) {
                toastr.success('Successfully deleted product search product table for confirmation!');
                productDataTable.ajax.reload();
            } else {
                console.log(data);
                toastr.error('An error occured while trying to delete product contact administrator!');
            }

        });
    }

//});

function editProductDetails(id) {
    $('#edit-logo').attr('src', "");
    $('#edit-img-source').hide();
    $('#edit-img-filename').hide();
    $.ajax({
        url: '/api/Inventory/GetProducts/' + id,
        method: 'GET',
        dataType: 'json'
    }).done(function (data) {
        console.log(data);
        $('#edit-product-modal').modal('show');
        $('#edit-product-name').val(data[0].ProductName);
        $('#edit-product-available').val(data[0].ItemsAvailable);
        $('#edit-product-price').val(data[0].SellingPrice);
        $('#edit-product-reference').val(data[0].ProductReference);
        $('#edit-product-code').val(data[0].ProductCode);
        $('#edit-product-category').val(data[0].ProductCategory);
        $('#edit-logo').attr('src', data[0].ProductImage);
        if ($('#edit-logo').val() == null) {
            $('#edit-logo').attr('src', "../../assets/img/iconOnly.png");
        }
        $('#edit-product-id').val(id);

    });

}

$("#edit-product-name").on("input", function () {
    $('#edit-product-reference').val($(this).val() + "-" + productDate);
});

$('#ImportProduct').on('click', function () {
    $('#upload-excel').modal('show');

    $('#dropzone').fileupload({
        dropZone: $('#dropzone'),
        dataType: "application/json",
        url: '/api/Inventory/UploadExcelData/' + "1"
    }).on('fileuploadadd', function (e, data) {
        data.submit();
    }).on('fileuploadalways', function (e, data) {
        console.log(data);
        productDataTable.ajax.reload();
        var dropZone = $('#dropzone');
        dropZone.removeClass('hover');
    });

    $(document).bind('dragover', function (e) {
        var dropZone = $('#dropzone');
        dropZone.addClass('hover');
    });

    $(document).bind('dragleave', function (e) {
        var dropZone = $('#dropzone');
        dropZone.removeClass('hover');
    });

    $(document).bind('drop dragover', function (e) {
        e.preventDefault();
    });

});

$('#img-source').hide();
$('#img-filename').hide();

function readURL(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $('#img-source').val(e.target.result);
            $('#img-filename').val(input.value.substring((input.value.lastIndexOf("\\")) + 1));
            $('#logo').attr('src', e.target.result);

            $('#edit-img-source').val(e.target.result);
            $('#edit-img-filename').val(input.value.substring((input.value.lastIndexOf("\\")) + 1));
            $('#edit-logo').attr('src', e.target.result);

        }

        reader.readAsDataURL(input.files[0]); // convert to base64 string
    }
}
