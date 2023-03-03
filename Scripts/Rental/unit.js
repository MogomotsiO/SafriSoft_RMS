var unitDataTable = $("#unit-table").DataTable({
    "responsive": true,
    "autoWidth": false,
    "success": true,
    lengthMenu: [[10, 20, 30, -1], [10, 20, 30, "All"]],
    ajax: {
        url: '/api/Rental/GetUnits/',
        method: 'GET',
        "dataSrc": ""
    },
    'columns': [
        {
            'data': 'Id',
            'searchable': true
        },
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
                return '<td class="text-right py-0 align-middle">' + new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data) + '</td>';
            }
        },
        {
            'data': 'NumberOfTenants',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"> <a id="' + full.Id + '" name="' + full.UnitName + '" href="#" onclick="openAssignModal(this.id, this.name)" class="btn btn-white" href="#"><i class="fa fa-user-plus text-success"></i></a><a class="btn btn-white" href="#">' + data + '</a> </div> </td>';
            }
        },
        {
            'data': 'Id',
            'render': function (data, type, full, meta) {
                return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"> <a id="' + full.Id + '" class="btn btn-white" href="#" onclick="editUnitDetails(this.id)"><i class="fas fa-folder-open text-success"></i></a> <a id="' + full.Id + '" class="btn btn-white" href="#" name="Unit" onclick="changeRecordStatus(this.id,this.name)"><i class="fas fa-trash text-success"></i></a> </div> </td>'
            }
        }
    ]
});

$('#CreateUnit').on('click', function () {
    $('#create-unit-modal').modal('show');
});

$('#unit-final-create').on('click', function () {
    var unitName = $("#unit-name").val();
    var unitDescription = $("#unit-description").val();
    var unitRooms = $("#unit-rooms").val();
    var unitPrice = $("#unit-price").val();
    var unitNumber = $("#unit-number").val();
    var unitSharing = $("#unit-sharing").val();

    if (unitName != "" && unitNumber != "" && unitPrice != "" && unitRooms != "") {

        var unit = {
            'UnitNumber': unitNumber,
            'UnitName': unitName,
            'UnitRooms': unitRooms,
            'UnitSharing': unitSharing,
            'UnitDescription': unitDescription == null ? "" : unitDescription,
            'UnitPrice': unitPrice
        };

        $.ajax({
            url: '/api/Rental/UnitCreate',
            method: 'POST',
            dataType: 'json',
            data: unit,
            contextType: "application/json",
            traditional: true
        }).done(function (data) {
            if (data.Success) {
                console.log(data);
                $('#create-unit-modal').modal('hide');
                toastr.success('Successfully created unit search units table for confirmation!');
                unitDataTable.ajax.reload();
            } else {
                console.log(data);
                toastr.error('An error occured while trying to save unit contact administrator!');
            }

        });
    } else {
        toastr.error('Only Unit description can be empty!');
    }

});

function editUnitDetails(id) {

    $.ajax({
        url: '/api/Rental/GetUnit/' + id,
        method: 'GET',
        dataType: 'json'
    }).done(function (data) {
        console.log(data);
        $('#edit-unit-modal').modal('show');
        $('#edit-unit-number').val(data[0].UnitNumber);
        $('#edit-unit-name').val(data[0].UnitName);
        $('#edit-unit-description').val(data[0].UnitDescription);
        $('#edit-unit-rooms').val(data[0].UnitRooms);
        $('#edit-unit-price').val(data[0].UnitPrice);
        $('#edit-unit-sharing').val(data[0].UnitSharing);
        $('#edit-unit-id').val(id);

    });

}

$('#unit-final-update').on('click', function () {
    var unitNumber = $("#edit-unit-number").val();
    var unitName = $("#edit-unit-name").val();
    var unitDescription = $("#edit-unit-description").val();
    var unitRooms = $("#edit-unit-rooms").val();
    var unitPrice = $("#edit-unit-price").val();
    var unitSharing = $("#edit-unit-sharing").val();
    var unitId = $("#edit-unit-id").val();

    if (unitName != "" && unitNumber != "" && unitPrice != "" && unitRooms != "") {

        var unit = {
            'Id': unitId,
            'UnitNumber': unitNumber,
            'UnitName': unitName,
            'UnitRooms': unitRooms,
            'UnitSharing': unitSharing,
            'UnitDescription': unitDescription == null ? "" : unitDescription,
            'UnitPrice': unitPrice
        };

        $.ajax({
            url: '/api/Rental/UnitUpdate',
            method: 'POST',
            dataType: 'json',
            data: unit,
            contextType: "application/json",
            traditional: true
        }).done(function (data) {
            if (data.Success) {
                console.log(data);
                $('#edit-unit-modal').modal('hide');
                toastr.success('Successfully updated unit search unit table for confirmation!');
                unitDataTable.ajax.reload();
            } else {
                console.log(data);
                toastr.error('An error occured while trying to save unit contact administrator!');
            }

        });
    } else {
        toastr.error('Only Unit description can be empty!');
    }

});

var tenantAssDataTable = $("#assign-tenant-table").DataTable({
    "responsive": true,
    "autoWidth": false,
    "success": true,
    "order": [[4, "desc"]],
    lengthMenu: [[10, 20, 30, -1], [10, 20, 30, "All"]],
    ajax: {
        url: '/api/Rental/GetAssignedTenants/' + 1,
        method: 'GET',
        "dataSrc": ""
    },
    'columns': [
        {
            'data': 'Id',
            'searchable': true,
            'render': function (data, type, full, meta) {
                if (full.Assigned == "True") {
                    return '<td><div class="text-success">' + data + '</div></td>';
                } else {
                    return '<td><div class="">' + data + '</div></td>';
                }

            }
        },
        {
            'data': 'TenantName',
            'searchable': true,
            'render': function (data, type, full, meta) {
                if (full.Assigned == "True") {
                    return '<td><div class="text-success">' + data + '</div></td>';
                } else {
                    return '<td><div class="">' + data + '</div></td>';
                }
                
            }
        },
        {
            'data': 'TenantEmail',
            'searchable': true,
            'render': function (data, type, full, meta) {
                if (full.Assigned == "True") {
                    return '<td><div class="text-success">' + data + '</div></td>';
                } else {
                    return '<td><div class="">' + data + '</div></td>';
                }

            }
        },
        {
            'data': 'TenantCell',
            'searchable': true,
            'render': function (data, type, full, meta) {
                if (full.Assigned == "True") {
                    return '<td><div class="text-success">' + data + '</div></td>';
                } else {
                    return '<td><div class="">' + data + '</div></td>';
                }

            }
        },
        {
            'data': 'Assigned',
            'render': function (data, type, full, meta) {
                if (full.Assigned == "True") {
                    return '<td><div class="text-success">' + data + '</div></td>';
                } else {
                    return '<td><div class="">' + data + '</div></td>';
                }
            }
        },
        {
            'data': 'Id',
            'render': function (data, type, full, meta) {
                if (full.Assigned == "True") {
                    return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"> <a id="' + full.Id + '" class="btn btn-white" href="#" onclick="removeTenantUnit(this.id,' + full.UnitId +')"><i class="fa fa-user-times text-success"></i></a> </div> </td>';
                } else {
                    return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"> <a id="' + full.Id + '" class="btn btn-white" href="#" onclick="assignTenantUnit(this.id,' + full.UnitId +')"><i class="fa fa-user-plus"></i></a> </div> </td>';
                }
                
            }
        }
    ]
});

function openAssignModal(id, name) {

    $('#assign-title').text("Unit: " + name);
    tenantAssDataTable.ajax.url('/api/Rental/GetAssignedTenants/' + id).load();
    $('#assign-tenant-unit').modal('show');
}

function assignTenantUnit(tenantId, unitId) {
    var tenantUnit = {
        'Id': parseInt(tenantId),
        'UnitId': unitId
    };
    console.log(tenantUnit);
    $.ajax({
        url: '/api/Rental/AssignTenantUnit',
        method: 'POST',
        dataType: 'json',
        data: tenantUnit,
        contextType: "application/json",
        traditional: true 
    }).done(function (data) {
        if (data.Found == false) {
            console.log(data);
            toastr.success('Successfully assigned the tenant to the unit!');
            tenantAssDataTable.ajax.url('/api/Rental/GetAssignedTenants/' + unitId).load();
            unitDataTable.ajax.reload();
        } else {
            console.log(data);
            if (data.Message != "") {
                toastr.error(data.Message);
            } else {
                toastr.error('An error occured while trying to assign unit to tenant contact administrator!');
            }
        }

    });
}

function removeTenantUnit(tenantId, unitId) {
    var tenantUnit = {
        'Id': parseInt(tenantId),
        'UnitId': unitId
    };
    console.log(tenantUnit);
    $.ajax({
        url: '/api/Rental/RemoveTenantUnit',
        method: 'POST',
        dataType: 'json',
        data: tenantUnit,
        contextType: "application/json",
        traditional: true
    }).done(function (data) {
        if (data.Success == true) {
            console.log(data);
            toastr.success('Successfully removed the tenant from the unit!');
            tenantAssDataTable.ajax.url('/api/Rental/GetAssignedTenants/' + unitId).load();
            unitDataTable.ajax.reload();
        } else {
            console.log(data);
            toastr.error('An error occured while trying to remove unit from tenant contact administrator! ' + data.Error);
        }

    });
}