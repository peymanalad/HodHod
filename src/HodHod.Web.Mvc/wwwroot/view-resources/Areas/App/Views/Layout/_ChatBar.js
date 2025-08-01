﻿var chatService = abp.services.app.chat;
var friendshipService = abp.services.app.friendship;

var chat = {
    friends: [],
    serverClientTimeDifference: 0,
    selectedUser: null,
    isOpen: false,
    isModalOpen: false,
    lastFilteredUserCount: 0,

    getFriendOrNull: function (userId, tenantId) {
        var friend = _.where(chat.friends, {
            friendUserId: parseInt(userId),
            friendTenantId: tenantId ? parseInt(tenantId) : null,
        });
        if (friend.length) {
            return friend[0];
        }

        return null;
    },

    getFixedMessageTime: function (messageTime) {
        return moment(messageTime)
            .add(-1 * chat.serverClientTimeDifference, 'seconds')
            .format('YYYY-MM-DDTHH:mm:ssZ');
    },

    getFriendsAndSettings: function (callBack) {
        chatService.getUserChatFriendsWithSettings().done(function (result) {
            chat.friends = result.friends;
            chat.lastFilteredUserCount = result.friends.length;
            chat.serverClientTimeDifference = app.calculateTimeDifference(abp.clock.now(), result.serverTime, 'seconds');

            chat.triggerUnreadMessageCountChangeEvent();
            chat.renderFriendLists(chat.friends);
            callBack();
        });
    },

    showChatBar: function () {
        KTDrawer.getInstance(document.querySelector('#kt_drawer_chat')).show();
    },

    loadLastState: function () {
        app.localStorage.getItem('app.chat.isOpen', function (isOpen) {
            chat.isOpen = isOpen;
            chat.adjustNotifyPosition();

            app.localStorage.getItem('app.chat.pinned', function (pinned) {
                chat.pinned = pinned;
                var $sidebarPinner = $('a.page-quick-sidebar-pinner');
                $sidebarPinner.find('i:eq(0)').attr('class', 'fa fa-map-pin ' + (chat.pinned ? '' : 'fa-rotate-90'));
            });

            if (chat.isOpen) {
                chat.showChatBar();
                app.localStorage.getItem('app.chat.selectedUser', function (user) {
                    if (user) {
                        chat.showMessagesPanel();
                        chat.selectFriend(user.friendUserId, user.friendTenantId);
                    } else {
                        chat.showFriendsPanel();
                    }
                });
            }
        });
    },

    changeChatPanelIsOpenOnLocalStorage: function () {
        app.localStorage.setItem('app.chat.isOpen', chat.isOpen);
    },

    changeChatUserOnLocalStorage: function () {
        app.localStorage.setItem('app.chat.selectedUser', chat.selectedUser);
    },

    changeChatPanelPinnedOnLocalStorage: function () {
        app.localStorage.setItem('app.chat.pinned', chat.pinned);
    },

    changeChatPanelPinned: function (pinned) {
        chat.pinned = pinned;
        var $sidebarPinner = $('.page-quick-sidebar-pinner');
        $sidebarPinner.find('i:eq(0)').attr('class', 'fa fa-map-pin ' + (chat.pinned ? '' : 'fa-rotate-90'));

        chat.changeChatPanelPinnedOnLocalStorage();
    },

    // Friends
    selectFriend: function (friendUserId, friendTenantId) {
        var chatUser = chat.getFriendOrNull(friendUserId, friendTenantId);
        chat.selectedUser = chatUser;
        chat.changeChatUserOnLocalStorage();
        chat.user.setSelectedUserOnlineStatus(chatUser.isOnline);

        chat.showMessagesPanel();

        $('#selectedChatUserName').text(
            chat.user.getShownUserName(chat.selectedUser.friendTenancyName, chat.selectedUser.friendUserName),
        );

        $('#ChatMessage').val('');

        if (chat.selectedUser.state !== app.consts.friendshipState.blocked) {
            $('#liBanChatUser, #ChatMessageWrapper').show();
            $('#liUnbanChatUser, #UnblockUserButton').hide();
            $('#ChatMessage').removeAttr('disabled');
            $('#ChatMessage').focus();
            $('#SendChatMessageButton').removeAttr('disabled');
        } else {
            $('#liBanChatUser, #ChatMessageWrapper').hide();
            $('#liUnbanChatUser, #UnblockUserButton').show();
            $('#ChatMessage').attr('disabled', 'disabled');
            $('#SendChatMessageButton').attr('disabled', 'disabled');
        }

        if (!chatUser.messagesLoaded) {
            chat.user.loadMessages(chatUser, function () {
                chatUser.messagesLoaded = true;
                chat.scrollToBottom();
            });
        } else {
            var renderedMessages = chat.renderMessages(chatUser.messages);
            $('#UserChatMessages').html(renderedMessages);
            chat.scrollToBottom();

            $('.timeago').timeago();

            chat.user.markAllUnreadMessagesOfUserAsRead(chat.selectedUser);
        }
    },

    showMessagesPanel: function () {
        $('#kt_drawer_chat_friends').addClass('d-none');
        $('#kt_drawer_chat_messenger').removeClass('d-none', function () {
            chat.scrollToBottom();
        });
        $('#kt_quick_sidebar_back').removeClass('d-none');
    },

    showFriendsPanel: function () {
        $('#kt_drawer_chat_friends').removeClass('d-none');
        $('#kt_drawer_chat_messenger').addClass('d-none');
    },

    changeFriendState: function (user, state) {
        var friend = chat.getFriendOrNull(user.friendUserId, user.friendTenantId);
        if (!friend) {
            return;
        }

        friend.state = state;
        chat.renderFriendLists(chat.friends);
    },

    removeFriendUI: function (user) {
        let deletedFriend = chat.getFriendOrNull(user.friendUserId, user.friendTenantId);
        if (!deletedFriend) {
            return;
        }

        chat.friends = chat.friends.filter((item) => {
            if (item.friendUserId === deletedFriend.friendUserId && item.friendTenantId === deletedFriend.friendTenantId) {
                return false;
            }
            return true;
        });

        chat.selectedUser = null;
        chat.changeChatUserOnLocalStorage();
        chat.renderFriendLists(chat.friends);
        chat.showFriendsPanel();
    },

    getFormattedFriends: function (friends) {
        $.each(friends, function (index, friend) {
            friend.shownUserName = chat.user.getShownUserName(friend.friendTenancyName, friend.friendUserName);
        });

        return friends;
    },

    renderFriendList: function (friends, $element) {
        var template = $('#UserFriendTemplate').html();
        Mustache.parse(template);
        var rendered = Mustache.render(template, {
            friends: friends,
            appPath: abp.appPath,
        });
        $element.html(rendered);
    },

    renderFriendLists: function (friends) {
        friends = chat.getFormattedFriends(friends);

        var acceptedFriends = _.where(friends, { state: app.consts.friendshipState.accepted });
        acceptedFriends.isFriend = true;
        chat.renderFriendList(acceptedFriends, $('#friendListFriends'));

        var blockedFriends = _.where(friends, { state: app.consts.friendshipState.blocked });
        blockedFriends.isFriend = false;
        chat.renderFriendList(blockedFriends, $('#friendListBlockeds'));

        if (acceptedFriends.length) {
            $('#EmptyFriendListInfo').hide();
        } else {
            $('#EmptyFriendListInfo').show();
        }

        if (blockedFriends.length) {
            $('#EmptyBlockedFriendListInfo').hide();
        } else {
            $('#EmptyBlockedFriendListInfo').show();
        }
    },

    //Messages
    sendMessage: function () {
        if (!$("form[name='chatMessageForm']").valid() || chat.selectedUser.state === app.consts.friendshipState.blocked) {
            return;
        }

        $('#SendChatMessageButton').attr('disabled', 'disabled');

        app.chat.sendMessage(
            {
                tenantId: chat.selectedUser.friendTenantId,
                userId: chat.selectedUser.friendUserId,
                message: $('#ChatMessage').val(),
                tenancyName: app.session.tenant ? app.session.tenant.tenancyName : null,
                userName: app.session.user.userName,
                profilePictureId: app.session.user.profilePictureId,
            },
            function () {
                $('#ChatMessage').val('');
                $('#SendChatMessageButton').removeAttr('disabled');
            },
        );
    },

    getFormattedMessages: function (messages) {
        $.each(messages, function (index, message) {
            message.creationTime = chat.getFixedMessageTime(message.creationTime);
            message.isIn = message.side !== app.chat.side.sender;
            message.shownUserName =
                message.side === app.chat.side.sender ? app.session.user.userName : chat.selectedUser.friendUserName;

            var readStateClass = message.receiverReadState === app.chat.readState.read ? ' text-primary' : ' text-muted';
            message.readStateCheck =
                message.side === app.chat.side.sender
                    ? '<i class="read-state-check fa fa-check' + readStateClass + '" aria-hidden="true"></i>'
                    : '';

            if (message.message.startsWith('[image]')) {
                var image = JSON.parse(message.message.substring('[image]'.length));
                var imageUrl = abp.appPath + 'App/Chat/GetImage?id=' + message.id + '&contentType=' + image.contentType + '&fileName=' + image.name;
                var uploadedImageMsg =
                    '<a href="' + imageUrl + '" target="_blank"><img src="' + imageUrl + '" class="chat-image-preview"></a>';

                message.formattedMessage = uploadedImageMsg;
            } else if (message.message.startsWith('[file]')) {
                var file = JSON.parse(message.message.substring('[file]'.length));
                var fileUrl = abp.appPath + 'App/Chat/GetFile?id=' + message.id + '&contentType=' + file.contentType;

                var uploadedFileMsg =
                    '<a href="' +
                    fileUrl +
                    '" download="' +
                    file.name +
                    '" class="chat-file-preview"><i class="la la-file"></i> ' +
                    file.name +
                    ' <i class="la la-download pull-right"></i></a>';

                message.formattedMessage = uploadedFileMsg;
            } else if (message.message.startsWith('[link]')) {
                var linkMessage = JSON.parse(message.message.substring('[file]'.length));

                message.formattedMessage =
                    '<a href="' +
                    linkMessage.message +
                    '" target="_blank" class="chat-link-message"><i class="fa fa-link"></i> ' +
                    linkMessage.message +
                    '</a>';
            } else {
                message.formattedMessage = Mustache.escape(message.message);
            }
        });

        return messages;
    },

    renderMessages: function (messages) {
        messages = chat.getFormattedMessages(messages);

        var template = $('#UserChatMessageTemplate').html();
        Mustache.parse(template);

        return Mustache.render(template, {
            messages: messages,
            appPath: abp.appPath,
        });
    },

    scrollToPosition: function (position) {
        const messagePanel = $('#kt_drawer_chat_messenger_body');
        if (!messagePanel.length) return;

        const images = messagePanel.find('img');
        if (images.length > 0) {
            const loadPromises = [];

            images.each(function () {
                const img = this;
                if (img.complete) return;

                loadPromises.push(
                    new Promise((resolve) => {
                        $(img).on('load error', resolve);
                    }),
                );
            });

            Promise.all(loadPromises).then(() => {
                chat.applyScroll(position, messagePanel);
            });
        } else {
            chat.applyScroll(position, messagePanel);
        }
    },

    applyScroll: function (position, messagePanel) {
        const el = messagePanel[0];

        if (position === 'bottom') {
            el.scrollTop = el.scrollHeight + 20;
        } else if (position === 'top') {
            el.scrollTop = 0;
        } else if (typeof position === 'number') {
            el.scrollTop = position;
        }
    },

    scrollToBottom: function () {
        setTimeout(() => {
            chat.scrollToPosition('bottom');
        }, 500);
    },

    scrollToTop: function () {
        chat.scrollToPosition('top');
    },

    //Events & UI

    adjustNotifyPosition: function () {
        if (chat.isOpen) {
            app.changeNotifyPosition('toast-chat-open');
        } else {
            app.changeNotifyPosition('toast-bottom-right');
        }
    },

    triggerUnreadMessageCountChangeEvent: function () {
        var totalUnreadMessageCount = 0;
        if (chat && chat.friends) {
            totalUnreadMessageCount = _.reduce(
                chat.friends,
                function (memo, friend) {
                    return memo + friend.unreadMessageCount;
                },
                0,
            );
        }

        abp.event.trigger('app.chat.unreadMessageCountChanged', totalUnreadMessageCount);
    },

    bindUiEvents: function () {
        $('#kt_drawer_chat').on('click', '#kt_quick_sidebar_back', function () {
            chat.selectedUser = null;
            chat.changeChatUserOnLocalStorage();
        });

        KTDrawer.getInstance(document.querySelector('#kt_drawer_chat')).on('kt.drawer.toggled', function (drawer) {
            chat.isOpen = drawer.isShown();
            chat.adjustNotifyPosition();
            chat.changeChatPanelIsOpenOnLocalStorage();
            if (chat.isOpen) {
                chat.user.markAllUnreadMessagesOfUserAsRead(chat.selectedUser);
            }
        });

        KTDrawer.getInstance(document.querySelector('#kt_drawer_chat')).on('kt.drawer.after.hidden', function (drawer) {
            $('#ChatUserSearchUserName').popover('hide');
            chat.isOpen = drawer.isShown();
            chat.adjustNotifyPosition();
            chat.changeChatPanelIsOpenOnLocalStorage();
        });

        $('#friendListFriends, #friendListBlockeds').on('click', '.chat-user', function () {
            var friendUserId = $(this).attr('data-friend-user-id');
            var friendTenantId = $(this).attr('data-friend-tenant-id');
            chat.selectFriend(friendUserId, friendTenantId);
        });

        $('#kt_quick_sidebar_back').on('click', function () {
            chat.showFriendsPanel();
        });

        $('#liBanChatUser').click(function () {
            chat.user.block(chat.selectedUser);
        });

        $('#liUnbanChatUser, #UnblockUserButton').click(function () {
            chat.user.unblock(chat.selectedUser);
        });

        $('#removeFriend').click(function () {
            abp.message.confirm(app.localize('AreYouSureToRemoveFromFriends'), null, function (isConfirmed) {
                if (isConfirmed) {
                    chat.user.removeFriend(chat.selectedUser);
                }
            });
        });

        $('#SearchChatUserButton').click(function () {
            chat.user.openSearchModal();
        });

        $('#ChatUserSearchUserName, #ChatUserSearchTenancyName').keypress(function (e) {
            if (e.which === 13) {
                e.preventDefault();
                chat.user.search();
            }
        });

        $('#ChatMessage').keypress(function (e) {
            if (e.which === 13) {
                e.preventDefault();
                chat.sendMessage();
            }
        });

        $('#SendChatMessageButton').click(function (e) {
            chat.sendMessage();
        });

        $('#kt_drawer_chat_messenger_body').on('scroll', function (e) {
            if (e.target.scrollTop === 0 && !chat.user.allPreviousMessagesLoaded) {
                chat.user.loadMessages(chat.selectedUser, () => {
                    if (chat.user.allPreviousMessagesLoaded) {
                        return;
                    }

                    chat.scrollToBottom();
                });
            }
        });

        $('#ChatUserSearchUserName').on('keyup', function () {
            var friends = _.filter(chat.friends, function (friend) {
                return (
                    chat.user
                        .getShownUserName(friend.friendTenancyName, friend.friendUserName)
                        .toLowerCase()
                        .indexOf($('#ChatUserSearchUserName').val().toLowerCase()) >= 0
                );
            });

            if (friends.length > 0 && friends.length !== chat.lastFilteredUserCount) {
                chat.renderFriendLists(friends);
            }

            chat.lastFilteredUserCount = friends.length;
        });

        $("form[name='chatMessageForm']").validate({
            invalidHandler: function () {
                $('#SendChatMessageButton').attr('disabled', 'disabled');
            },
            errorPlacement: function () { },
            success: function () {
                $('#SendChatMessageButton').removeAttr('disabled');
            },
        });

        $('.page-quick-sidebar-pinner').click(function () {
            chat.changeChatPanelPinned(!chat.pinned);
        });

        if (!chat.tenantToTenantChatAllowed && chat.tenantToHostChatAllowed) {
            $('#InterTenantChatHintIcon').hide();
        }
    },

    hideChatPanel: function () {
        KTDrawer.hideAll(null, '#kt_drawer_chat');
        chat.isOpen = false;
        chat.adjustNotifyPosition();
        chat.changeChatPanelIsOpenOnLocalStorage();
    },

    registerEvents: function () {
        abp.event.on('app.chat.messageReceived', function (message) {
            var user = chat.getFriendOrNull(message.targetUserId, message.targetTenantId);

            if (user) {
                user.messages = user.messages || [];
                user.messages.push(message);

                if (message.side === app.chat.side.receiver) {
                    user.unreadMessageCount += 1;
                    message.readState = app.chat.readState.unread;
                    chat.user.changeUnreadMessageCount(user.friendTenantId, user.friendUserId, user.unreadMessageCount);
                    chat.triggerUnreadMessageCountChangeEvent();

                    if (
                        chat.isOpen &&
                        chat.selectedUser !== null &&
                        user.friendTenantId === chat.selectedUser.friendTenantId &&
                        user.friendUserId === chat.selectedUser.friendUserId
                    ) {
                        chat.user.markAllUnreadMessagesOfUserAsRead(chat.selectedUser);
                    } else {
                        abp.notify.info(
                            abp.utils.formatString('{0}: {1}', user.friendUserName, abp.utils.truncateString(message.message, 100)),
                            null,
                            {
                                onclick: function () {
                                    if (!$('#kt_drawer_chat').hasClass('drawer-on')) {
                                        chat.showChatBar();
                                        chat.isOpen = true;
                                        chat.changeChatPanelIsOpenOnLocalStorage();
                                    }

                                    chat.showMessagesPanel();

                                    chat.selectFriend(user.friendUserId, user.friendTenantId);
                                    chat.changeChatPanelPinned(true);
                                },
                            },
                        );
                    }
                }

                if (
                    chat.selectedUser !== null &&
                    user.friendUserId === chat.selectedUser.friendUserId &&
                    user.friendTenantId === chat.selectedUser.friendTenantId
                ) {
                    var renderedMessage = chat.renderMessages([message]);
                    $('#UserChatMessages').append(renderedMessage);
                    $('.timeago').timeago();
                }

                chat.scrollToBottom();
            }
        });

        abp.event.on('app.chat.friendshipRequestReceived', function (data, isOwnRequest) {
            if (!isOwnRequest) {
                abp.notify.info(abp.utils.formatString(app.localize('UserSendYouAFriendshipRequest'), data.friendUserName));
            }

            chatService.getUserChatFriendsWithSettings().done(function (result) {
                chat.friends = result.friends;
                chat.renderFriendLists(chat.friends);
                chat.triggerUnreadMessageCountChangeEvent();
            });
        });

        abp.event.on('app.chat.userConnectionStateChanged', function (data) {
            chat.user.setFriendOnlineStatus(data.friend.userId, data.friend.tenantId, data.isConnected);
        });

        abp.event.on('app.chat.userStateChanged', function (data) {
            var user = chat.getFriendOrNull(data.friend.userId, data.friend.tenantId);
            if (!user) {
                return;
            }

            user.state = data.state;
            chat.renderFriendLists(chat.friends);
        });

        abp.event.on('app.chat.userDeleted', function (data) {
            var user = chat.getFriendOrNull(data.user.userId, data.user.tenantId);
            chat.removeFriendUI(user);
        });

        abp.event.on('app.chat.allUnreadMessagesOfUserRead', function (data) {
            var user = chat.getFriendOrNull(data.friend.userId, data.friend.tenantId);
            if (!user) {
                return;
            }

            user.unreadMessageCount = 0;
            chat.user.changeUnreadMessageCount(user.friendTenantId, user.friendUserId, user.unreadMessageCount);
            chat.triggerUnreadMessageCountChangeEvent();
        });

        abp.event.on('app.chat.readStateChange', function (data) {
            var user = chat.getFriendOrNull(data.friend.userId, data.friend.tenantId);
            if (!user) {
                return;
            }

            $.each(user.messages, function (index, message) {
                message.receiverReadState = app.chat.readState.read;
            });

            if (chat.selectedUser && chat.selectedUser.friendUserId === data.friend.userId) {
                $('.read-state-check').not('.m--font-info').addClass('m--font-info');
            }
        });

        abp.event.on('app.chat.connected', function () {
            $('#chat_is_connecting_icon').addClass('d-none');
            $('#kt_drawer_chat_toggle').removeClass('d-none');
            chat.getFriendsAndSettings(function () {
                chat.bindUiEvents();
                chat.loadLastState();
            });
        });
    },

    init: function () {
        chat.registerEvents();
    },

    user: {
        loadingPreviousUserMessages: false,

        getShownUserName: function (tenanycName, userName) {
            return (tenanycName ? tenanycName : '.') + '\\' + userName;
        },

        block: function (user) {
            friendshipService
                .blockUser({
                    userId: user.friendUserId,
                    tenantId: user.friendTenantId,
                })
                .done(function () {
                    chat.changeFriendState(user, app.consts.friendshipState.blocked);
                    abp.notify.info(app.localize('UserBlocked'));

                    $('#ChatMessage').attr('disabled', 'disabled');
                    $('#SendChatMessageButton').attr('disabled', 'disabled');
                    $('#liBanChatUser, #ChatMessageWrapper').hide();
                    $('#liUnbanChatUser, #UnblockUserButton').show();
                });
        },

        unblock: function (user) {
            friendshipService
                .unblockUser({
                    userId: user.friendUserId,
                    tenantId: user.friendTenantId,
                })
                .done(function () {
                    chat.changeFriendState(user, app.consts.friendshipState.accepted);
                    abp.notify.info(app.localize('UserUnblocked'));

                    $('#ChatMessage').removeAttr('disabled');
                    $('#ChatMessage').focus();
                    $('#SendChatMessageButton').removeAttr('disabled');
                    $('#liBanChatUser, #ChatMessageWrapper').show();
                    $('#liUnbanChatUser, #UnblockUserButton').hide();
                });
        },

        removeFriend: function (user) {
            friendshipService
                .removeFriend({
                    userId: user.friendUserId,
                    tenantId: user.friendTenantId,
                })
                .done(function () {
                    abp.notify.info(app.localize('FriendRemoved'));
                    chat.removeFriendUI(user);
                    $('#ChatMessage').removeAttr('disabled');
                    $('#ChatMessage').focus();
                    $('#SendChatMessageButton').removeAttr('disabled');
                    $('#liBanChatUser, #ChatMessageWrapper').show();
                    $('#liUnbanChatUser, #UnblockUserButton').hide();
                });
        },

        markAllUnreadMessagesOfUserAsRead: function (user) {
            if (!user || !chat.isOpen) {
                return;
            }

            var unreadMessages = _.where(user.messages, { readState: app.chat.readState.unread });
            var unreadMessageIds = _.pluck(unreadMessages, 'id');

            if (!unreadMessageIds.length) {
                return;
            }

            chatService
                .markAllUnreadMessagesOfUserAsRead({
                    tenantId: user.friendTenantId,
                    userId: user.friendUserId,
                })
                .done(function () {
                    $.each(user.messages, function (index, message) {
                        if (unreadMessageIds.indexOf(message.id) >= 0) {
                            message.readState = app.chat.readState.read;
                        }
                    });

                    chat.renderFriendLists(chat.friends);
                });
        },

        changeUnreadMessageCount: function (tenantId, userId, messageCount) {
            if (!tenantId) {
                tenantId = '';
            }
            var $userItems = $(
                '#friendListFriends div.message-count[data-friend-tenant-id="' +
                tenantId +
                '"][data-friend-user-id="' +
                userId +
                '"]',
            );

            if ($userItems) {
                var $item = $($userItems[0]).find('span.badge');
                $item.html(messageCount);

                if (messageCount) {
                    $item.removeClass('d-none');
                } else {
                    $item.addClass('d-none');
                }
            }
        },

        loadMessages: function (user, callback) {
            chat.user.allPreviousMessagesLoaded = false;

            var minMessageId = null;
            if (user.messages && user.messages.length) {
                minMessageId = _.min(user.messages, function (message) {
                    return message.id;
                }).id;
            }

            chatService
                .getUserChatMessages({
                    minMessageId: minMessageId,
                    tenantId: user.friendTenantId,
                    userId: user.friendUserId,
                })
                .done(function (result) {
                    if (!user.messages) {
                        user.messages = [];
                    }

                    user.messages = result.items.concat(user.messages);

                    chat.user.markAllUnreadMessagesOfUserAsRead(user);

                    if (!result.items.length) {
                        chat.user.allPreviousMessagesLoaded = true;
                    }

                    var renderedMessages = chat.renderMessages(user.messages);
                    $('#UserChatMessages').html(renderedMessages);

                    $('.timeago').timeago();

                    if (callback) {
                        callback();
                    }
                });
        },

        openSearchModal: function () {
            var lookupModal = app.modals.AddFriendModal.create({
                title: app.localize('SelectAUser'),
                serviceMethod: abp.services.app.commonLookup.findUsers,
                filterText: $('#ChatUserSearchUserName').val(),
                excludeCurrentUser: true,
            });

            // Checking add friend modal is open
            chat.isModalOpen = true;
            lookupModal.onClose(() => (chat.isModalOpen = false));

            lookupModal.open({}, function (selectedItem) {
                var userId = selectedItem.id;
                friendshipService
                    .createFriendshipRequest({
                        userId: userId,
                        tenantId: app.session.tenant !== null ? app.session.tenant.id : null,
                    })
                    .done(function () {
                        $('#ChatUserSearchUserName').val('');
                    });
            });
        },

        setFriendOnlineStatus: function (userId, tenantId, isOnline) {
            var user = chat.getFriendOrNull(userId, tenantId);
            if (!user) {
                return;
            }

            user.isOnline = isOnline;

            var statusClass =
                'symbol-badge bg-' +
                (isOnline ? 'success' : 'secondary') +
                ' start-100 top-100 border-4 h-15px w-15px ms-n2 mt-n2';

            var $userItems = $(
                '#friendListFriends div.chat-user[data-friend-tenant-id="' +
                (tenantId ? tenantId : '') +
                '"][data-friend-user-id="' +
                userId +
                '"]',
            );
            if ($userItems) {
                $($userItems[0]).find('.symbol-badge').attr('class', statusClass);
            }

            if (
                chat.selectedUser &&
                tenantId === chat.selectedUser.friendTenantId &&
                userId === chat.selectedUser.friendUserId
            ) {
                chat.user.setSelectedUserOnlineStatus(isOnline);
            }
        },

        setSelectedUserOnlineStatus: function (isOnline) {
            if (chat.selectedUser) {
                var statusClass = 'badge badge-circle w-10px h-10px me-1 badge-' + (isOnline ? 'success' : 'secondary');
                $('#selectedChatUserStatus span:eq(0)').attr('class', statusClass);
                $('#selectedChatUserStatus span:eq(1)').text(app.localize(isOnline ? 'Online' : 'Offline'));
            }
        },
    },
};

chat.init();

(function ($) {
    $(function () {
        // Change this to the location of your server-side upload handler:
        var url = abp.appPath + 'App/Chat/UploadFile';

        //Image Upload
        const imageId = '#chatImageUpload';

        var dropZone = new Dropzone(imageId, {
            url: url,
            method: 'post',
            paramName: 'file',
            maxFilesize: 10,
            maxFiles: 10,
            autoProcessQueue: true,
            previewsContainer: false,
            acceptedFiles: 'image/jpeg, image/jpg, image/png, image/webp, image/gif',
            clickable: imageId + ' .dropzone-select-image',
        });

        dropZone.on('sending', function (file, xhr, formData) {
            var token = abp.security.antiForgery.getToken();
            formData.append('__RequestVerificationToken', token);
        });

        dropZone.on('totaluploadprogress', function (data) {
            var progress = parseInt((data.loaded / data.total) * 100, 10);
            $('.chat-progress-bar').show();
            $('#chatFileUploadProgress .progress-bar').css('width', progress + '%');
        });

        dropZone.on('success', function (file, response) {
            var jsonResult = response.result;
            $('#ChatMessage').val(
                '[image]{"id":"' +
                jsonResult.id +
                '", "name":"' +
                jsonResult.name +
                '", "contentType":"' +
                jsonResult.contentType +
                '"}',
            );
            chat.sendMessage();
            $('.chat-progress-bar').hide();

            $(imageId)
                .prop('disabled', !$.support.fileInput)
                .parent()
                .addClass($.support.fileInput ? undefined : 'disabled');


            hideTooltip('#chatImageUpload .btn[data-bs-toggle="tooltip"]');
        });

        dropZone.on('error', function (file, response) {
            abp.message.error(response.__abp ? response.error.message : response);
        });

        dropZone.on('complete', function (file) {
            dropZone.removeFile(file);
        });

        //File Upload
        const fileId = '#chatFileUpload';

        var dropZone = new Dropzone(fileId, {
            url: url,
            method: 'post',
            paramName: 'file',
            maxFilesize: 10,
            maxFiles: 1,
            autoProcessQueue: true,
            previewsContainer: false,
            clickable: fileId + ' .dropzone-select-file',
            acceptedFiles: '.pdf, .docx, .xlsx, .txt, .csv, .md',
        });

        dropZone.on('sending', function (file, xhr, formData) {
            var token = abp.security.antiForgery.getToken();
            formData.append('__RequestVerificationToken', token);
        });

        dropZone.on('totaluploadprogress', function (data) {
            var progress = parseInt((data.loaded / data.total) * 100, 10);
            $('.chat-progress-bar').show();
            $('#chatFileUploadProgress .progress-bar').css('width', progress + '%');
        });

        dropZone.on('success', function (file, response) {
            var jsonResult = response.result;
            $('#ChatMessage').val(
                '[file]{"id":"' +
                jsonResult.id +
                '", "name":"' +
                jsonResult.name +
                '", "contentType":"' +
                jsonResult.contentType +
                '"}',
            );
            chat.sendMessage();
            $('.chat-progress-bar').hide();

            $(fileId)
                .prop('disabled', !$.support.fileInput)
                .parent()
                .addClass($.support.fileInput ? undefined : 'disabled');

            hideTooltip('#chatFileUpload .btn[data-bs-toggle="tooltip"]');
        });

        dropZone.on('error', function (file, response) {
            abp.message.error(response.__abp ? response.error.message : response);
        });

        dropZone.on('complete', function (file) {
            dropZone.removeFile(file);
        });

        $('#btnLinkShare').click(function () {
            $('#chatDropdownToggle').dropdown('toggle');
            $('#ChatMessage').val('[link]{"message":"' + window.location.href + '"}');
            chat.sendMessage();
        });

        $('.fileinput-button').click(function () {
            $('#chatDropdownToggle').dropdown('toggle');
        });

        function hideTooltip(selector) {
            var tooltipTrigger = document.querySelector(selector);
            if (!tooltipTrigger) return;

            var tooltipInstance = bootstrap.Tooltip.getInstance(tooltipTrigger);
            if (tooltipInstance) {
                tooltipInstance.hide();
            }
        }
    });
})(jQuery);
