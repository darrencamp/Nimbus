﻿using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Transport;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;
using Nimbus.Transports.InProcess.MessageSendersAndReceivers;

namespace Nimbus.Transports.InProcess
{
    public class InProcessTransportConfiguration : TransportConfiguration
    {
        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.RegisterType<InProcessMessageStore>(ComponentLifetime.SingleInstance);

            container.RegisterType<InProcessQueueSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<InProcessTopicSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<InProcessQueueReceiver>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<InProcessDelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof(IDelayedDeliveryService));

            //FIXME The transport itself should have an opinion on this, not the NimbusMessageFactory. We shouldn't know about this here.
            container.RegisterType<UnsupportedLargeMessageBodyStore>(ComponentLifetime.SingleInstance, typeof(ILargeMessageBodyStore));

            container.RegisterType<InProcessTransport>(ComponentLifetime.SingleInstance, typeof (INimbusTransport));
        }
    }
}