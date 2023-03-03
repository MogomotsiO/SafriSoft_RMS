var tenantDataTable = $("#tenant-table").DataTable({
    "responsive": true,
    "autoWidth": false,
    "success": true,
    lengthMenu: [[10, 20, 30, -1], [10, 20, 30, "All"]],
    ajax: {
        url: '/api/Rental/GetTenants/',
        method: 'GET',
        "dataSrc": ""
    },
    'columns': [
        {
            'data': 'Id',
            'searchable': true
        },
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
        },
        {
            'data': 'Documents',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"> <a id="' + full.Id + '" name="' + full.TenantName + '" href="#" onclick="openDocumentModal(this.id, this.name)" class="btn btn-white" href="#"><i class="fa fa-eye text-success"></i></a> <a class="btn btn-white" href="#">' + data + '</a> </div> </td>';
            }
        },
        {
            'data': 'NOK',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"> <a id="' + full.Id + '" name="' + full.TenantName + '" href="#" onclick="openNokModal(this.id, this.name)" class="btn btn-white" href="#"><i class="fa fa-eye text-success"></i></a> <a class="btn btn-white" href="#">' + data + '</a> </div> </td>';
            }
        },
        {
            'data': 'Id',
            'render': function (data, type, full, meta) {
                return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"> <a id="' + full.Id + '" class="btn btn-white" href="#" onclick="editTenantDetails(this.id)"><i class="fas fa-folder-open text-success"></i></a> <a id="' + full.Id + '" class="btn btn-white" href="#" name="Tenant" onclick="changeRecordStatus(this.id,this.name)"><i class="fas fa-trash text-success"></i></a> </div> </td>'
            }
        }
    ]
});

$('#CreateTenant').on('click', function () {
    $('#create-tenant-modal').modal('show');
});

$('#tenant-final-create').on('click', function () {
    var tenantName = $("#tenant-name").val();
    var tenantEmail = $("#tenant-email").val();
    var tenantCell = $("#tenant-cell").val();
    var tenantAddress = $("#tenant-address").val();
    var tenantWorkAddress = $("#tenant-work-address").val();
    var tenantWorkCell = $("#tenant-work-cell").val();
    var dateLeaseStart = $("#date-lease-start").val();
    var dateLeaseEnd = $("#date-lease-end").val();

    if (tenantName != "" && tenantAddress != "" && (tenantEmail != "" || tenantCell != "")) {

        var tenant = {
            'TenantName': tenantName,
            'TenantEmail': tenantEmail,
            'TenantCell': tenantCell,
            'TenantAddress': tenantAddress,
            'TenantWorkAddress': tenantWorkAddress,
            'TenantWorkCell': tenantWorkCell,
            'DateLeaseStart': dateLeaseStart,
            'DateLeaseEnd': dateLeaseEnd
        };

        $.ajax({
            url: '/api/Rental/TenantCreate',
            method: 'POST',
            dataType: 'json',
            data: tenant,
            contextType: "application/json",
            traditional: true
        }).done(function (data) {
            if (data.Success) {
                console.log(data);
                $('#create-tenant-modal').modal('hide');
                toastr.success('Successfully created tenant search tenant table for confirmation!');
                tenantDataTable.ajax.reload();
            } else {
                console.log(data);
                toastr.error('An error occured while trying to save tenant contact administrator!');
            }

        });
    } else {
        if (tenantName == "") {
            toastr.error('Tenant name cannot be empty!');
        }
        if (tenantAddress == "") {
            toastr.error('Tenant address cannot be empty!');
        }
        if (tenantEmail == "" || tenantCell == "") {
            toastr.error('Tenant email or cell number cannot be empty!');
        }
    }

});

function editTenantDetails(id) {

    $.ajax({
        url: '/api/Rental/GetTenant/' + id,
        method: 'GET',
        dataType: 'json'
    }).done(function (data) {
        console.log(data);
        $('#edit-tenant-modal').modal('show');
        $('#edit-tenant-name').val(data[0].TenantName);
        $('#edit-tenant-email').val(data[0].TenantEmail);
        $('#edit-tenant-cell').val(data[0].TenantCell);
        $('#edit-tenant-address').val(data[0].TenantAddress);
        $('#edit-tenant-work-address').val(data[0].TenantWorkAddress);
        $('#edit-tenant-work-cell').val(data[0].TenantWorkCell);
        $('#edit-tenant-lease-start').val(data[0].DateLeaseStart);
        $('#edit-tenant-lease-end').val(data[0].DateLeaseEnd);
        $('#edit-tenant-id').val(id);

    });

}

$('#tenant-final-update').on('click', function () {
    var tenantName = $("#edit-tenant-name").val();
    var tenantEmail = $("#edit-tenant-email").val();
    var tenantCell = $("#edit-tenant-cell").val();
    var tenantAddress = $("#edit-tenant-address").val();
    var tenantWorkAddress = $("#edit-tenant-work-address").val();
    var tenantWorkCell = $("#edit-tenant-work-cell").val();
    var dateLeaseStart = $("#edit-tenant-lease-start").val();
    var dateLeaseEnd = $("#edit-tenant-lease-end").val();
    var tenantId = $("#edit-tenant-id").val();

    if (tenantName != "" && tenantAddress != "" && (tenantEmail != "" || tenantCell != "")) {

        var tenant = {
            'Id': tenantId,
            'TenantName': tenantName,
            'TenantEmail': tenantEmail,
            'TenantCell': tenantCell,
            'TenantAddress': tenantAddress,
            'TenantWorkAddress': tenantWorkAddress,
            'TenantWorkCell': tenantWorkCell,
            'DateLeaseStart': dateLeaseStart,
            'DateLeaseEnd': dateLeaseEnd
        };

        $.ajax({
            url: '/api/Rental/TenantUpdate',
            method: 'POST',
            dataType: 'json',
            data: tenant,
            contextType: "application/json",
            traditional: true
        }).done(function (data) {
            if (data.Success) {
                console.log(data);
                $('#edit-tenant-modal').modal('hide');
                toastr.success('Successfully updated tenant search tenant table for confirmation!');
                tenantDataTable.ajax.reload();
            } else {
                console.log(data);
                toastr.error('An error occured while trying to save tenant contact administrator!');
            }

        });
    } else {
        if (tenantName == "") {
            toastr.error('Tenant name cannot be empty!');
        }
        if (tenantAddress == "") {
            toastr.error('Tenant address cannot be empty!');
        }
        if (tenantEmail == "" || tenantCell == "") {
            toastr.error('Tenant email or cell number cannot be empty!');
        }
    }

});

function openNokModal(id, name) {
    $("#tenants-noks-title").text(name);
    $('#tenant-id').val(id);
    nokDataTable.ajax.url('/api/Rental/GetNOKs/' + id).load();
    $('#tenants-noks').modal('show');
}

$('#CreateTenantNok').on('click', function () {
    $('#tenants-noks').modal('hide');
    $('#create-noks-modal').modal('show');

});

$('#tenant-nok-final-create').on('click', function () {
    var nokName = $('#nok-name').val();
    var nokEmail = $('#nok-email').val();
    var nokCell = $('#nok-cell').val();
    var nokRelation = $('#nok-relationship').val();
    var tenantId = $('#tenant-id').val();

    if (nokName != "" && (nokEmail != "" || nokCell != "")) {

        var nok = {
            'NOKName': nokName,
            'NOKEmail': nokEmail,
            'NOKCell': nokCell,
            'NOKRelation': nokRelation,
            'TenantId': tenantId
        };

        $.ajax({
            url: '/api/Rental/NOKCreate',
            method: 'POST',
            dataType: 'json',
            data: nok,
            contextType: "application/json",
            traditional: true
        }).done(function (data) {
            if (data.Success) {
                console.log(data);
                $('#create-noks-modal').modal('hide');
                $('#tenants-noks').modal('show');
                toastr.success('Successfully created NOK search NOK table for confirmation!');
                nokDataTable.ajax.url('/api/Rental/GetNOKs/' + tenantId).load();
                tenantDataTable.ajax.reload();
            } else {
                console.log(data);
                toastr.error('An error occured while trying to save NOK contact administrator!');
            }

        });
    } else {
        if (nokName == "") {
            toastr.error('NOK name cannot be empty!');
        }
        if (nokEmail == "" || nokCell == "") {
            toastr.error('NOK email or cell number cannot be empty!');
        }
    }

});

var nokDataTable = $("#tenant-nok-table").DataTable({
    "responsive": true,
    "autoWidth": false,
    "success": true,
    lengthMenu: [[10, 20, 30, -1], [10, 20, 30, "All"]],
    ajax: {
        url: '/api/Rental/GetNOKs/',
        method: 'GET',
        "dataSrc": ""
    },
    'columns': [
        {
            'data': 'Id',
            'searchable': true
        },
        {
            'data': 'NOKName',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td id="' + data + '" >' + data + '</td>';
            }
        },
        {
            'data': 'NOKEmail',
            'searchable': true
        },
        {
            'data': 'NOKCell',
            'searchable': true
        },
        {
            'data': 'NOKRelation',
            'searchable': true
        },
        {
            'data': 'DateNOKCreated',
            'searchable': true
        },
        {
            'data': 'Id',
            'render': function (data, type, full, meta) {
                return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"> <a id="' + full.Id + '" class="btn btn-white" href="#" onclick="editNOKDetails(this.id)"><i class="fas fa-folder-open text-success"></i></a> <a id="' + full.Id + '" class="btn btn-white" href="#" name="NOK" onclick="changeRecordStatus(this.id,this.name)"><i class="fas fa-trash text-success"></i></a> </div> </td>'
            }
        }
    ]
});

function editNOKDetails(id) {

    $.ajax({
        url: '/api/Rental/GetNOK/' + id,
        method: 'GET',
        dataType: 'json'
    }).done(function (data) {
        console.log(data);
        $('#tenants-noks').modal('hide');
        $('#update-noks-modal').modal('show');
        $('#edit-nok-name').val(data[0].NOKName);
        $('#edit-nok-email').val(data[0].NOKEmail);
        $('#edit-nok-cell').val(data[0].NOKCell);
        $('#edit-nok-relation').val(data[0].NOKRelation);
        $('#nok-id').val(data[0].Id);

    });

    
}


$('#tenant-nok-final-update').on('click', function () {
    var nokName = $('#edit-nok-name').val();
    var nokEmail = $('#edit-nok-email').val();
    var nokCell = $('#edit-nok-cell').val();
    var nokRelation = $('#edit-nok-relationship').val();
    var nokId = $('#nok-id').val();

    if (nokName != "" && (nokEmail != "" || nokCell != "")) {

        var nok = {
            'Id': nokId,
            'NOKName': nokName,
            'NOKEmail': nokEmail,
            'NOKCell': nokCell,
            'NOKRelation': nokRelation
        };

        $.ajax({
            url: '/api/Rental/NOKUpdate',
            method: 'POST',
            dataType: 'json',
            data: nok,
            contextType: "application/json",
            traditional: true
        }).done(function (data) {
            if (data.Success) {
                console.log(data);
                $('#update-noks-modal').modal('hide');
                $('#tenants-noks').modal('show');
                toastr.success('Successfully update NOK search NOK table for confirmation!');
                nokDataTable.ajax.url('/api/Rental/GetNOKs/' + $('#tenant-id').val()).load();
                tenantDataTable.ajax.reload();
            } else {
                console.log(data);
                toastr.error('An error occured while trying to update NOK contact administrator!');
            }

        });
    } else {
        if (nokName == "") {
            toastr.error('NOK name cannot be empty!');
        }
        if (nokEmail == "" || nokCell == "") {
            toastr.error('NOK email or cell number cannot be empty!');
        }
    }
});


function openDocumentModal(id, name) {
    $("#tenants-documents-title").text(name);
    $('#tenant-id').val(id);
    documentDataTable.ajax.url('/api/Rental/GetDocuments/' + id).load();
    $('#tenants-documents').modal('show');

    $('#dropzone').fileupload({
        dropZone: $('#dropzone'),
        dataType: "application/json",
        url: '/api/Rental/UploadDocument/' + id
    }).on('fileuploadadd', function (e, data) {
        data.submit();
    }).on('fileuploadalways', function (e, data) {
        documentDataTable.ajax.url('/api/Rental/GetDocuments/' + id).load();
        tenantDataTable.ajax.reload();
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
}

var documentDataTable = $("#tenant-document-table").DataTable({
    "responsive": true,
    "autoWidth": false,
    "success": true,
    lengthMenu: [[10, 20, 30, -1], [10, 20, 30, "All"]],
    ajax: {
        url: '/api/Rental/GetDocuments/',
        method: 'GET',
        "dataSrc": ""
    },
    'columns': [
        {
            'data': 'Id',
            'searchable': true
        },
        {
            'data': 'FileName',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td id="' + data + '" >' + data + '</td>';
            }
        },
        {
            'data': 'DateFileCreated',
            'searchable': true
        },
        {
            'data': 'Id',
            'render': function (data, type, full, meta) {
                return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"> <a id="' + full.Id + '" class="btn btn-white" href="DownloadFile?fileId=' + full.Id + '"><i class="fas fa-download text-success"></i></a> <a id="' + full.Id + '" class="btn btn-white" href="#" name="Documents" onclick="changeRecordStatus(this.id,this.name)"><i class="fas fa-trash text-success"></i></a> </div> </td>'
            }
        }
    ]
});

function downloadFile(fileId) {

    if (fileId != "") {
        $.ajax({
            url: '/Rental/DownloadFile?fileId=' + fileId,
            method: 'POST',
            dataType: 'binary',
            processData: false
        }).done(function (data) {
            console.log(data);

        });
    }

}

function changeRecordStatus(id, name) {
    var recordDetails = {
        'Id': id,
        'Name': name
    };

    $.ajax({
        url: '/api/Rental/ChangeRecordStatus',
        method: 'POST',
        dataType: 'json',
        data: recordDetails,
        contextType: "application/json",
        traditional: true
    }).done(function (data) {
        if (data.Success) {
            console.log(data);
            toastr.success('Successfully removed the record!');
            nokDataTable.ajax.url('/api/Rental/GetNOKs/' + $('#tenant-id').val()).load();
            documentDataTable.ajax.url('/api/Rental/GetDocuments/' + $('#tenant-id').val()).load();
            tenantDataTable.ajax.reload();
            unitDataTable.ajax.reload();
        } else {
            console.log(data);
            toastr.error('An error occured while trying to remove the record contact administrator!');
        }

    });

}