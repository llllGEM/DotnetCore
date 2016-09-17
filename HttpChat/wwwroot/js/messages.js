(function () {
    var Message;
    Message = function (arg) {
        this.text = arg.text, this.message_side = arg.message_side, this.username = arg.username;
        this.draw = function (_this) {
            return function () {
                var $message;
                $message = $($('.message_template').clone().html());
                $message.addClass(_this.message_side).find('.text').html(_this.text)
                                                     .find('.username').html(_this.username);
                $('.messages').append($message);
                return setTimeout(function () {
                    return $message.addClass('appeared');
                }, 0);
            };
        }(this);
        return this;
    };
    $(function () {
        var getUsername, getMessageText, message_side, displayMessage,sendMessage;
        message_side = 'right';
        getMessageText = function () {
            var $message_input;
            $message_input = $('.message_input');
            return $message_input.val();
        };
        getUsername = function () {
            var $username;
            $username = $('.username');
            return $username.val();
        };
        displayMessage = function (name, text) {
            var $messages, message, username;
            if (text.trim() === '') {
                return;
            }
            sendMessage(name, text);
            $('.message_input').val('');
            $messages = $('.messages');
            message_side = message_side === 'left' ? 'right' : 'left';
            message = new Message({
                text: text,
                message_side: message_side,
                username: name
            });
            message.draw();
            return $messages.animate({ scrollTop: $messages.prop('scrollHeight') }, 300);
        };
        sendMessage = function (name, text) {
            $.ajax({
                type: "post",
                url: "/home/send",
                data: {username: name, message: text},
                success: function (response) { 
                    var user = response;
                },
                error: function (response) {
                    console.log("Error"); 
                }
            })
            .done(function (response){
                //todo handle receipt
            });
        };
        $('.send_message').click(function (e) {
            return displayMessage(getUsername(), getMessageText());
        });
        $('.message_input').keyup(function (e) {
            if (e.which === 13) {
                return displayMessage(getUsername(), getMessageText());
            }
        });
        // displayMessage('Hello Philip! :)');
        // setTimeout(function () {
        //     return displayMessage('Hi Sandy! How are you?');
        // }, 1000);
        // return setTimeout(function () {
        //     return displayMessage('I\'m fine, thank you!');
        // }, 2000);
    });
}.call(this));
