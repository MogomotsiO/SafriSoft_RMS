var userDataTable = $("#example1").DataTable({
    "responsive": true,
    "autoWidth": false,
    "info": true,
    lengthMenu: [[7, 15, 30, -1], [7, 15, 30, "All"]],
    ajax: {
        url: '/api/Inventory/GetUserData/',
        method: 'GET',
        "dataSrc": ""
    },
    'columns': [
        {
            'data': 'Id',
            'searchable': true
        },
        {
            'data': 'Username',
            'searchable': true
        },
        {
            'data': 'Email',
            'searchable': true
        },
        {
            'data': 'UserRole',
            'searchable': true
        },
        {
            'data': 'NumberOfOrders',
            'searchable': true
        },
        {
            'data': 'RandValueSold',
            'searchable': true,
            'render': function (data, type, full, meta) {
                return new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(data);
            }
        },
        {
            'data': 'Id',
            'render': function (data, type, full, meta) {
                if (full.UserState == "Locked") {
                    return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"><a id="' + full.UserId + '" class="btn btn-white" href="#" onclick="userLock(this.id)"><i class="fas fa-lock text-info"></i></a> </div> </td>'
                } else {
                    return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"><a id="' + full.UserId + '" class="btn btn-white" href="#" onclick="userLock(this.id)"><i class="fas fa-unlock-alt text-info"></i></a> </div> </td>'
                }

            }
        }
    ]
});

var organisationValue = $('#organisation-value').text();
$('#CreateUser').on('click', function () {
    $('#creat-user-modal').modal('show');
    $('#user-role').val('Captain');
    $('#organisation').val(organisationValue);
    $('#password').val(organisationValue + '&1');
});
var passwordCreation = organisationValue + '&1';
$("#username").on("input", function () {
    //$('#password').val(passwordCreation + $(this).val());
});
$('#final-create').on('click', function () {
    // Get all inputs and assign to variables
    var username = $('#username').val();
    var email = $('#user-email').val();
    var role = $('#user-role').val();
    var organisation = $('#organisation').val();
    var password = $('#password').val();

    if (username != "" && email != "") {

        var user = {
            'Username': username,
            'Email': email,
            'Role': role,
            'OrganisationName': organisation,
            'Password': password.trim()
        };

        $.ajax({
            url: '/api/Inventory/UserCreate',
            method: 'POST',
            dataType: 'json',
            data: user,
            contextType: "application/json",
            traditional: true
        }).done(function (data) {
            if (data.Succeeded) {
                console.log(data);
                $('#creat-user-modal').modal('hide');
                toastr.success('Successfully created user, search user table for confirmation!');
                userDataTable.ajax.reload();
            } else {
                console.log(data);
                toastr.error('An error occured while trying to save user, contact administrator!');
            }

        });
    } else {
        if (username == "") {
            toastr.error('Username cannot be empty!');
        }
        if (email == "") {
            toastr.error('Email cannot be empty!');
        }
    }

});

function editOrganisation() {
    $('#img-source').hide();
    $('#img-filename').hide();
    //$('#logo').hide();
    $.ajax({
        url: '/api/Inventory/GetOrganisationDetails',
        method: 'GET',
        dataType: 'json'
    }).done(function (data) {
        $('.input-group').addClass('is-filled');
        $('#update-organisation-modal').modal('show');
        $('#organisation-name').val(data[0].OrganisationName);
        $('#organisation-email').val(data[0].OrganisationEmail);
        $('#organisation-cell').val(data[0].OrganisationCell);
        $('#organisation-street').val(data[0].OrganisationStreet);
        $('#organisation-suburb').val(data[0].OrganisationSuburb);
        $('#organisation-city').val(data[0].OrganisationCity);
        $('#organisation-code').val(data[0].OrganisationCode);
        $('#account-name').val(data[0].AccountName);
        $('#account-no').val(data[0].AccountNo);
        $('#bank-name').val(data[0].BankName);
        $('#branch-name').val(data[0].BranchName);
        $('#branch-code').val(data[0].BranchCode);
        $('#clients-reference').val(data[0].ClientReference);
        $('#vat-number').val(data[0].VATNumber);
        $('#logo').attr('src', data[0].OrganisationLogo);
    });

}

function finalUpdateOrganisation() {

    var OrganisationName = $('#organisation-name').val();
    var OrganisationEmail = $('#organisation-email').val();
    var OrganisationCell = $('#organisation-cell').val();
    var OrganisationStreet = $('#organisation-street').val();
    var OrganisationSuburb = $('#organisation-suburb').val();
    var OrganisationCity = $('#organisation-city').val();
    var OrganisationCode = $('#organisation-code').val();
    var AccountName = $('#account-name').val();
    var AccountNo = $('#account-no').val();
    var BankName = $('#bank-name').val();
    var BranchName = $('#branch-name').val();
    var BranchCode = $('#branch-code').val();
    var ClientReference = $('#clients-reference').val();
    var VATNumber = $('#vat-number').val();
    var OrganisationLogo = $('#img-source').val();
    
    var organisation = {
        'OrganisationId': 1,
        'OrganisationName': OrganisationName != "" ? OrganisationName : "Change",
        'OrganisationEmail': OrganisationEmail != "" ? OrganisationEmail : "Change",
        'OrganisationCell': OrganisationCell != "" ? OrganisationCell : "Change",
        'OrganisationLogo': OrganisationLogo != "" ? OrganisationLogo : "Change",
        'OrganisationStreet': OrganisationStreet != "" ? OrganisationStreet : "Change",
        'OrganisationSuburb': OrganisationSuburb != "" ? OrganisationSuburb : "Change",
        'OrganisationCity': OrganisationCity != "" ? OrganisationCity : "Change",
        'OrganisationCode': OrganisationCode,
        'AccountName': AccountName != "" ? AccountName : "Change",
        'AccountNo': AccountNo,
        'BankName': BankName != "" ? BankName : "Change",
        'BranchName': BranchName != "" ? BranchName : "Change",
        'BranchCode': BranchCode != "" ? BranchCode : "Change",
        'ClientReference': ClientReference != "" ? ClientReference : "Change",
        'VATNumber': VATNumber,
        'ImgLogoSource': "Change",
    }
    
    $.ajax({
        url: '/api/Inventory/SaveOrganisationDetails',
        method: 'POST',
        dataType: 'json',
        data: organisation,
        contextType: "application/json",
        traditional: true
    }).done(function (data) {
        if (data.Success) {
            console.log(data);
            $('#update-organisation-modal').modal('hide');
            editOrganisation();
            toastr.success('Successfully updated organisation details');
        } else {
            console.log(data);
            toastr.error('An error occured while trying to update organisation, contact administrator!');
        }

    });
}

function readURL(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $('#img-source').val(e.target.result);
            $('#img-filename').val(input.value.substring((input.value.lastIndexOf("\\")) + 1));
            $('#logo').attr('src', e.target.result);
        }

        reader.readAsDataURL(input.files[0]); // convert to base64 string
    }
}

$("#input-logo").change(function () {
    $('#logo').show();
    readURL(this);
});


function userLock(id) {
    var user = {
        'UserId': id
    };

    $.ajax({
        url: '/api/Inventory/UserLock',
        method: 'POST',
        dataType: 'json',
        data: user,
        contextType: "application/json",
        traditional: true
    }).done(function (data) {
        if (data.Success) {
            console.log(data);
            toastr.success('Successfully locked user, search user table for confirmation!');
            userDataTable.ajax.reload();
        } else {
            console.log(data);
            toastr.error('An error occured while trying to lock user, contact administrator!');
        }

    });
}