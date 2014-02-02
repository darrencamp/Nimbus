﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure
{
    internal class MessagePump : IMessagePump
    {
        private bool _haveBeenToldToStop;

        private readonly INimbusMessageReceiver _receiver;
        private readonly IMessageDispatcher _dispatcher;
        private readonly ILogger _logger;

        private Task _internalMessagePump;

        public MessagePump(INimbusMessageReceiver receiver, IMessageDispatcher dispatcher, ILogger logger)
        {
            _receiver = receiver;
            _dispatcher = dispatcher;
            _logger = logger;
        }

        public async Task Start()
        {
            if (_internalMessagePump != null)
                throw new InvalidOperationException("Message pump either is already running or was previously running and has not completed shutting down.");

            _logger.Debug("Message pump for {0} starting...", _receiver);
            _internalMessagePump = Task.Run(() => InternalMessagePump());
            await _receiver.WaitUntilReady();
            _logger.Debug("Message pump for {0} started", _receiver);
        }

        public async Task Stop()
        {
            _logger.Debug("Message pump for {0} stopping...", _receiver);
            _haveBeenToldToStop = true;
            var internalMessagePump = _internalMessagePump;
            if (internalMessagePump == null) return;

            await internalMessagePump;
            _logger.Debug("Message pump for {0} stopped.", _receiver);
        }

        private async Task InternalMessagePump()
        {
            while (!_haveBeenToldToStop)
            {
                try
                {
                    BrokeredMessage message;
                    Exception exception = null;

                    try
                    {
                        message = await _receiver.Receive();
                        if (message == null) continue;
                    }
                    catch (TimeoutException)
                    {
                        continue;
                    }
                    catch (MessagingException exc)
                    {
                        _logger.Error(exc.Message, exc);
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        continue;
                    }

                    try
                    {
                        _logger.Debug("Dispatching message: {0} from {1}", message, message.ReplyTo);
                        await _dispatcher.Dispatch(message);
                        _logger.Debug("Dispatched message: {0} from {1}", message, message.ReplyTo);

                        _logger.Debug("Completing message {0}", message);
                        await message.CompleteAsync();
                        _logger.Debug("Completed message {0}", message);

                        continue;
                    }
                    catch (Exception exc)
                    {
                        exception = exc;
                    }

                    _logger.Error(exception, "Message dispatch failed");

                    _logger.Debug("Abandoning message {0} from {1}", message, message.ReplyTo);
                    await message.AbandonAsync(exception.ExceptionDetailsAsProperties());
                    _logger.Debug("Abandoned message {0} from {1}", message, message.ReplyTo);
                }
                catch (Exception exc)
                {
                    _logger.Error(exc, "Unhandled exception in message pump");
                }
            }
        }
    }
}