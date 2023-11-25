namespace NotificationService.IntegrationEvents.Events
{
    public class OrderPaymentSuccessIntegrationEvent : EventBus.Base.Events.IntegrationEvent
    {
        public int OrderId { get; }

        public OrderPaymentSuccessIntegrationEvent(int orderId) => OrderId = orderId;
    }
}
