﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    public class Bus : IBus
    {
        private readonly string _connectionString;
        private readonly IEventBroker _eventBroker;
        private readonly Type[] _commandTypes;
        private MessagingFactory _messagingFactory;
        private readonly IList<IMessagePump> _messagePumps = new List<IMessagePump>();
        private IRequestResponseCorrelator _correlator;

        public Bus(string connectionString, IEventBroker eventBroker, Type[] commandTypes)
        {
            _connectionString = connectionString;
            _eventBroker = eventBroker;
            _commandTypes = commandTypes;
        }

        public void Send(object busCommand)
        {
            var sender = _messagingFactory.CreateMessageSender(busCommand.GetType().FullName);
            var message = new BrokeredMessage(busCommand);
            sender.Send(message);
        }

        public async Task<TResponse> Request<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest)
        {
            var response = await _correlator.MakeCorrelatedRequest(busRequest);
            return response;
        }

        public void Start()
        {
            _messagingFactory = MessagingFactory.CreateFromConnectionString(_connectionString);
            _correlator = new RequestResponseCorrelator(_messagingFactory);

            foreach (var commandType in _commandTypes)
            {
                EnsureQueueExists(commandType);

                var pump = new MessagePump(_messagingFactory, _eventBroker, commandType);
                _messagePumps.Add(pump);
                pump.Start();
            }

            _correlator.Start();
        }

        private void EnsureQueueExists(Type commandType)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(_connectionString);
            var queueName = commandType.FullName;

            if (! namespaceManager.QueueExists(queueName))
            {
                namespaceManager.CreateQueue(queueName);
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}