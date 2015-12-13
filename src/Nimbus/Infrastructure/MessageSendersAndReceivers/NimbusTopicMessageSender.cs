using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusTopicMessageSender : BatchingMessageSender
    {
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly ILogger _logger;

        private TopicClient _topicClient;

        public NimbusTopicMessageSender(IQueueManager queueManager, string topicPath, ILogger logger)
            : base()
        {
            _queueManager = queueManager;
            _topicPath = topicPath;
            _logger = logger;
        }

        protected override async Task SendBatch(NimbusMessage[] toSend)
        {
            var topicClient = GetTopicClient();

            var brokeredMessages = toSend.Select(BrokeredMessageFactory.BuildMessage);


            _logger.Debug("Flushing outbound message queue {0} ({1} messages)", _topicPath, toSend.Length);
            try
            {
                await topicClient.SendBatchAsync(brokeredMessages);
            }
            catch (Exception)
            {
                DiscardTopicClient();
                throw;
            }
        }

        private TopicClient GetTopicClient()
        {
            if (_topicClient != null) return _topicClient;

            _topicClient = _queueManager.CreateTopicSender(_topicPath).Result;
            return _topicClient;
        }

        private void DiscardTopicClient()
        {
            var topicClient = _topicClient;
            _topicClient = null;

            if (topicClient == null) return;
            if (topicClient.IsClosed) return;

            try
            {
                _logger.Info("Discarding message sender for {TopicPath}", _topicPath);
                topicClient.Close();
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Failed to close TopicClient instance before discarding it.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                DiscardTopicClient();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}