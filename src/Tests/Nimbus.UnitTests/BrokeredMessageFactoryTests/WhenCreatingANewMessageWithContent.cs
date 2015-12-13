using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.UnitTests.BrokeredMessageFactoryTests
{
    [TestFixture]
    internal class WhenCreatingANewMessageWithContent : GivenANimbusMessageFactory
    {
        private NimbusMessage _message;
        protected override async Task When()
        {
            _message = await Subject.Create(new TestMessage());
        }


        [Test]
        public void ThenTheCorrelationIdShouldBeTheMessageId()
        {
            _message.CorrelationId.ShouldBe(_message.MessageId);
        }

        [Test]
        public void ThenTheMessageTypeShouldBeSetToTheFullNameOfTheSerializedContent()
        {
            _message.SafelyGetBodyTypeNameOrDefault().ShouldBe(typeof (TestMessage).FullName);
        }

        [Test]
        public void ThenTheReplyToAddressShouldBeSetToTheSenderAddress()
        {
            _message.ReplyTo.ShouldBe(ReplyQueueNameSetting.Value);
        }

        [DataContract]
        public class TestMessage
        {
        }
    }
}