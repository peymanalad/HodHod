﻿var app = app || {};
(function () {
  //Check if SignalR is defined
  if (!signalR) {
    return;
  }

  //Create namespaces
  app.signalr = app.signalr || {};
  app.signalr.hubs = app.signalr.hubs || {};

  var chatHub = null;

  // Configure the connection
  function configureConnection(connection) {
    // Set the common hub
    abp.signalr.hubs.chat = connection;
    chatHub = connection;

    let reconnectTime = 5000;
    let maxTries = 8;

    // Register to get notifications
    registerChatEvents(connection);
  }

  // Connect to the server
  abp.signalr.connect = function () {
    // Start the connection.
    startConnection(abp.appPath + 'signalr-chat', configureConnection)
      .then(function (connection) {
        abp.log.debug('Connected to SignalR CHAT server!');
        abp.event.trigger('app.chat.connected');

        // Call the Register method on the hub.
        connection.invoke('register').then(function () {
          abp.log.debug('Registered to the SignalR CHAT server!');
        });
      })
      .catch(function (error) {
        abp.log.debug(error.message);
      });
  };

  // Starts a connection with transport fallback - if the connection cannot be started using
  // the webSockets transport the function will fallback to the serverSentEvents transport and
  // if this does not work it will try longPolling. If the connection cannot be started using
  // any of the available transports the function will return a rejected Promise.
  function startConnection(url, configureConnection) {
    if (abp.signalr.remoteServiceBaseUrl) {
      url = abp.signalr.remoteServiceBaseUrl + url;
    }

    // Add query string: https://github.com/aspnet/SignalR/issues/680
    if (abp.signalr.qs) {
      url += '?' + abp.signalr.qs;
    }

    return (function start(transport) {
      abp.log.debug('Starting connection using ' + signalR.HttpTransportType[transport] + ' transport');

      var connection = new signalR.HubConnectionBuilder()
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            abp.log.debug('Retry to connect to SignalR');
            if (retryContext.previousRetryCount > maxTries) {
              abp.log.debug('Max retries reached');
              return null;
            }
            reconnectTime *= 2;
            abp.log.debug('Waiting ' + reconnectTime + 'ms before retrying');
            return reconnectTime;
          },
        })
        .withUrl(url, transport)
        .build();

      if (configureConnection && typeof configureConnection === 'function') {
        configureConnection(connection);
      }

      return connection
        .start()
        .then(function () {
          return connection;
        })
        .catch(function (error) {
          abp.log.debug(
            'Cannot start the connection using ' +
              signalR.HttpTransportType[transport] +
              ' transport. ' +
              error.message,
          );
          if (transport !== signalR.HttpTransportType.LongPolling) {
            return start(transport + 1);
          }

          return Promise.reject(error);
        });
    })(signalR.HttpTransportType.WebSockets);
  }

  abp.signalr.startConnection = startConnection;

  if (abp.signalr.autoConnect === undefined) {
    abp.signalr.autoConnect = true;
  }

  if (abp.signalr.autoConnect) {
    abp.signalr.connect();
  }

  function registerChatEvents(connection) {
    connection.on('getChatMessage', function (message) {
      abp.event.trigger('app.chat.messageReceived', message);
    });

    connection.on('getAllFriends', function (friends) {
      abp.event.trigger('abp.chat.friendListChanged', friends);
    });

    connection.on('getFriendshipRequest', function (friendData, isOwnRequest) {
      abp.event.trigger('app.chat.friendshipRequestReceived', friendData, isOwnRequest);
    });

    connection.on('getUserConnectNotification', function (friend, isConnected) {
      abp.event.trigger('app.chat.userConnectionStateChanged', {
        friend: friend,
        isConnected: isConnected,
      });
    });

    connection.on('getUserStateChange', function (friend, state) {
      abp.event.trigger('app.chat.userStateChanged', {
        friend: friend,
        state: state,
      });
    });

    connection.on('getallUnreadMessagesOfUserRead', function (friend) {
      abp.event.trigger('app.chat.allUnreadMessagesOfUserRead', {
        friend: friend,
      });
    });

    connection.on('getUserDeleted', function (user) {
      abp.event.trigger('app.chat.userDeleted', {
        user: user,
      });
    });

    connection.on('getReadStateChange', function (friend) {
      abp.event.trigger('app.chat.readStateChange', {
        friend: friend,
      });
    });
  }

  app.chat.sendMessage = function (messageData, callback) {
    if (chatHub.connection._connectionState !== signalR.HubConnectionState.Connected) {
      callback && callback();
      abp.notify.warn(app.localize('ChatIsNotConnectedWarning'));
      return;
    }

    chatHub.invoke('sendMessage', messageData).then(function (result) {
      if (result) {
        abp.notify.warn(result);
      }

      callback && callback();
    });
  };
})();
