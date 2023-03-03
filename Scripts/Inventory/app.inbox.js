var inboxViewModel = function () {
    var self = this;
    self.Chats = ko.observableArray([]);
    self.notifications = ko.observableArray([]);
    self.Message = ko.observable();
    self.NumberOfNotifications = ko.observable();
    self.mainUser = false;

    // Stuff For Product Editting
    self.ProductName = ko.observable('');
    self.ProductAvailable = ko.observable('');
    self.SellingPrice = ko.observable('');
    self.ProductReference = ko.observable('');

    var inboxDataTable = $("#inbox-table").DataTable({
        "responsive": true,
        "autoWidth": false,
        "info": true,
        lengthMenu: [[10, 20, 30, -1], [10, 20, 30, "All"]],
        ajax: {
            url: '/api/Inventory/GetMessages/',
            method: 'GET',
            "dataSrc": ""
        },
        'columns': [
            {
                'data': 'Id',
                'searchable': true,
                'render': function (data, type, full, meta) {
                    if (full.Status == "Created") {
                        return '<td class="text-right py-0 align-middle"> <i class="fa fa-circle text-info"> </td>'
                    } else {
                        return '<td class="text-right py-0 align-middle"> <i class="fa fa-circle text-light"> </td>'
                    }
                    
                }
            },
            {
                'data': 'From',
                'searchable': true,
                'render': function (data, type, full, meta) {
                    if (full.Status == "Created") {
                        return '<td><span class="text-bold">' + data + '</span></td>'
                    } else {
                        return '<td><span>' + data + '</span></td>'
                    }
                    
                }
            },
            {
                'data': 'To',
                'searchable': true
            },
            {
                'data': 'Body',
                'searchable': true
            },
            {
                'data': 'DateCreated',
                'searchable': true
            },
            {
                'data': 'Id',
                'searchable': true,
                'render': function (data, type, full, meta) {
                    return '<td class="text-right py-0 align-middle"> <div class="btn-group btn-group-sm"><a id="' + full.Id + '" class="btn btn-white" href="#" onclick="messageRead(this.id)"><i class="fas fa-eye text-info"></i></a><a id="' + full.Id +'" class="btn btn-white" href="#" onclick="messageDelete(this.id)"><i class="fas fa-trash text-info"></i></a> </div> </td>'
                }
            }
        ]
    });

    var chats = function (name, dateCreated, body,dizzy, mainUser) {
        self.Name = ko.observable(name);
        self.DateCreated = ko.observable(dateCreated);
        self.Body = ko.observable(body);
        self.check = ko.observable(dizzy);

        if (name == mainUser) {
            self.UserChatPos = ko.observable("left");
        } else {
            self.UserChatPos = ko.observable("right");
        }
        
        if (name == name) {
            self.mainUser = true;
        }

    }

    self.GetChats = function (id) {
        self.Chats.removeAll();
        $.ajax({
            url: '/api/Inventory/GetChats/' + id,
            method: 'GET',
            dataType: 'json'
        }).done(function (data) {
            console.log(data);
            $('.send-btn').attr('id', id);
            for (var i = 0; i < data.length; i++) {
                self.Chats.push(new chats(data[i].From, data[i].DateCreated, data[i].Body, data[i].To, data[i].MainUser));
            }
        });
    }

    self.CreateMessage = function (id) {
        var text = $('#input-message').val();
        var message = { 'Body': text, 'To': id };

        if (message.Body != "") {
            $.ajax({
                url: '/api/Inventory/MessageCreate',
                method: 'POST',
                dataType: 'json',
                data: message
            }).done(function (data) {
                console.log(data);
                self.GetChats(data.Id);
                inboxDataTable.ajax.reload();
                $('#input-message').val('');
            });
        } else {
            toastr.error('Message cannot be empty, please select user and type message!');
        }
        
    }

    var notifis = function (name, dateCreated, body) {
        self.Name = ko.observable(name);
        self.DateCreated = ko.observable(dateCreated);
        self.Body = ko.observable(body.substring(0,50) + "...");
    }

    self.Notification = function () {
        self.notifications.removeAll();
        $.ajax({
            url: '/api/Inventory/GetNotifications/',
            method: 'GET',
            dataType: 'json'
        }).done(function (data) {
            console.log(data);
            for (var i = 0; i < data.length; i++) {
                self.notifications.push(new notifis(data[i].From, data[i].DateCreated, data[i].Body));
                self.NumberOfNotifications(data[i].Id);
            }
        });
    }


    self.OrderAuditData = ko.observableArray([]);
    self.LabelDates = ko.observable();

    var audiData = function (description, changed, dateCreated, userId) {
        self.Description = ko.observable(description);
        self.Changed = ko.observable(changed);
        self.DateCreated = ko.observable(dateCreated);
        self.LabelDates(dateCreated);
        self.UserName = ko.observable(userId);

        if (changed == "Inception") {
            self.Theme = ko.observable('bg-danger');
        }
        if (changed == "Processed") {
            self.Theme = ko.observable('bg-danger');
        }
        if (changed == "Packaged") {
            self.Theme = ko.observable('bg-warning');
        }
        if (changed == "InTransit") {
            self.Theme = ko.observable('bg-info');
        }
        if (changed == "Delivered") {
            self.Theme = ko.observable('bg-success');
        }
        
    }

    self.GetOrderAudit = function (id) {

        var orderData = {
            'OrderId': id
        }
        self.OrderAuditData.removeAll();
        $.ajax({
            url: '/api/Inventory/GetOrderAudit',
            method: 'POST',
            dataType: 'json',
            data: orderData
        }).done(function (data) {
            console.log(data);
            for (var i = 0; i < data.length; i++) {
                self.OrderAuditData.push(new audiData(data[i].Description, data[i].Changed, data[i].CreatedDate, data[i].UserId));
            }
            $('#view-order-title').text(id);
        });
    }

    self.messageRead = function (id) {
        var status = "Read";

        var message = {
            'Id': id,
            'Status': status
        };

        $.ajax({
            url: '/api/Inventory/MessageStatusChange',
            method: 'POST',
            dataType: 'json',
            data: message,
            contextType: "application/json",
            traditional: true
        }).done(function (data) {
            if (data.Success) {
                //toastr.success('Successfully deleted message search inbox table for confirmation!');
                inboxDataTable.ajax.reload();
                self.Notification();
                self.GetChats(data.UserToId);
            } else {
                console.log(data);
                toastr.error('An error occured while trying to open message contact administrator!');
            }

        });
    }

    self.messageDelete = function (id) {
        var status = "Deleted";

        var message = {
            'Id': id,
            'Status': status
        };

        $.ajax({
            url: '/api/Inventory/MessageStatusChange',
            method: 'POST',
            dataType: 'json',
            data: message,
            contextType: "application/json",
            traditional: true
        }).done(function (data) {
            if (data.Success) {
                //toastr.success('Successfully deleted message search inbox table for confirmation!');
                inboxDataTable.ajax.reload();
                self.Notification();
            } else {
                console.log(data);
                toastr.error('An error occured while trying to delete message contact administrator!');
            }

        });
    }

    
    self.Notification();
}
ko.applyBindings(inboxViewModel); 