namespace PaymentService.Api.IntegrationEvents.Events
{
    public class OrderStartedIntegrationEvent : EventBus.Base.Events.IntegrationEvent
    {
        public int OrderId { get; set; }

        public OrderStartedIntegrationEvent()
        {

        }

        public OrderStartedIntegrationEvent(int orderId)
        {
            OrderId = orderId;
        }
    }
}
