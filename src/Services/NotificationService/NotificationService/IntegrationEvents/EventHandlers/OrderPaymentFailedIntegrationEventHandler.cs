using EventBus.Base.Abstraction;
using Microsoft.Extensions.Logging;
using NotificationService.IntegrationEvents.Events;
using System.Threading.Tasks;

namespace NotificationService.IntegrationEvents.EventHandlers
{
    public class OrderPaymentFailedIntegrationEventHandler : IIntegrationEventHandler<OrderPaymentFailedIntegrationEvent>
    {
        private readonly ILogger<OrderPaymentFailedIntegrationEventHandler> logger;

        public OrderPaymentFailedIntegrationEventHandler(ILogger<OrderPaymentFailedIntegrationEventHandler> logger)
        {
            this.logger = logger;
        }

        public Task Handle(OrderPaymentFailedIntegrationEvent @event)
        {
            logger.LogInformation($"Order Payment failed with orderId: {@event.OrderId}, ErrorMessage: {@event.ErrorMessage}");

            return Task.CompletedTask;
        }
    }
}
