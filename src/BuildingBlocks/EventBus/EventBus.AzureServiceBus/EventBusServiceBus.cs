﻿using EventBus.Base;
using EventBus.Base.Events;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.AzureServiceBus
{
    public class EventBusServiceBus : BaseEventBus
    {
        private ITopicClient topicClient;
        private ManagementClient managementClient;
        private ILogger logger;
        public EventBusServiceBus(EventBusConfig config, IServiceProvider serviceProvider) : base(config, serviceProvider)
        {
            managementClient = new ManagementClient(config.EventBusConnectionString);
            topicClient = CreateTopicClient();
            logger = serviceProvider.GetService(typeof(ILogger<EventBusServiceBus>)) as ILogger<EventBusServiceBus>;
        }

        private ITopicClient CreateTopicClient()
        {
            if (topicClient == null || topicClient.IsClosedOrClosing)
                topicClient = new TopicClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, RetryPolicy.Default);

            // Ensure the topic already exists
            if (!managementClient.TopicExistsAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult())
                managementClient.CreateTopicAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult();

            return topicClient;
        }

        public override void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name; // Example: OrderCreatedIntegrationEvent

            eventName = ProcessEventName(eventName); // Trim: OrderCreatedIntegration

            var eventString = JsonConvert.SerializeObject(@event);

            var bodyArray = Encoding.UTF8.GetBytes(eventString);

            Message message = new Message()
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = bodyArray,
                Label = eventName
            };

            topicClient.SendAsync(message).GetAwaiter().GetResult();
        }

        public override void Subscribe<T, TH>()
        {
            var eventName = typeof(T).Name;

            eventName = ProcessEventName(eventName);

            if (!SubsManager.HasSubscriptionsForEvent(eventName))
            {
                var subscriptionClient = CreateSubscriptionClientIfNotExists(eventName);

                RegisterSubscriptionClientMessageHandler(subscriptionClient);
            }

            logger.LogInformation($"Subscribing to event {eventName} with {typeof(TH).Name}");

            SubsManager.AddSubscription<T, TH>();
        }

        public override void UnSubscribe<T, TH>()
        {
            var eventName = typeof(T).Name;

            try
            {
                // Subscription will be there but we don't subscribe
                var subscriptionClient = CreateSubscriptionClient(eventName);

                subscriptionClient
                    .RemoveRuleAsync(eventName)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {
                logger.LogWarning($"The messaging entity {eventName} coult not be found.");

                SubsManager.RemoveSubscription<T, TH>();
            }
        }

        private void RegisterSubscriptionClientMessageHandler(ISubscriptionClient subscriptionClient)
        {
            subscriptionClient.RegisterMessageHandler(
                async (message, token) =>
                {
                    var eventName = $"{message.Label}";
                    var messageData = Encoding.UTF8.GetString(message.Body);

                    // Complete the message so that is not received again
                    if (await ProcessEvent(ProcessEventName(eventName), messageData))
                        await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);

                },
                new MessageHandlerOptions(ExceptionReceivedHandler)
                {
                    MaxConcurrentCalls = 10,
                    AutoComplete = false,
                });
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var ex = exceptionReceivedEventArgs.Exception;
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            logger.LogError($"ERROR handling message: {ex.Message} - Context: {context}");

            return Task.CompletedTask;
        }
       
        private ISubscriptionClient CreateSubscriptionClientIfNotExists(string eventName)
        {
            var subscriptionClient = CreateSubscriptionClient(eventName);

            if (!managementClient.SubscriptionExistsAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult())
            {
                managementClient.CreateSubscriptionAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult();
                RemoveDefaultRule(subscriptionClient);
            }

            CreateRuleIfNotExists(ProcessEventName(eventName), subscriptionClient);

            return subscriptionClient;
        }
      
        private void CreateRuleIfNotExists(string eventName, ISubscriptionClient subscriptionClient)
        {
            bool ruleExits;

            try
            {
                var rule = managementClient.GetRuleAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName), eventName).GetAwaiter().GetResult();
                ruleExits = rule != null;
            }
            catch (MessagingEntityNotFoundException)
            {
                // Azure Management Client doesn't have RuleExists method
                ruleExits = false;
            }

            if (!ruleExits)
            {
                subscriptionClient.AddRuleAsync(new RuleDescription
                {
                    Filter = new CorrelationFilter { Label = eventName },
                    Name = eventName
                }).GetAwaiter().GetResult();
            }
        }
      
        private void RemoveDefaultRule(SubscriptionClient subscriptionClient)
        {
            try
            {
                subscriptionClient
                    .RemoveRuleAsync(RuleDescription.DefaultRuleName)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {
                logger.LogWarning($"The messaging entity {RuleDescription.DefaultRuleName} Could not be found.");
            }
        }
       
        private SubscriptionClient CreateSubscriptionClient(string eventName)
        {
            return new SubscriptionClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, GetSubName(eventName));
        }

        public override void Dispose()
        {
            base.Dispose();

            topicClient.CloseAsync().GetAwaiter().GetResult();
            managementClient.CloseAsync().GetAwaiter().GetResult();

            topicClient = null;
            managementClient = null;
        }
    }
}