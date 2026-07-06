using IoT.Api.Hubs;
using IoT.Shared.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace IoT.Api.Consumers
{
    public class CommandCompletedConsumer : IConsumer<CommandCompletedEvent>
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public CommandCompletedConsumer(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<CommandCompletedEvent> context)
        {
            string message = $"Hardware confirmed: {context.Message.ActionType} completed with status: {context.Message.Status} at {context.Message.CompletedAt}";
            
            await _hubContext.Clients.All.SendAsync("ReceiveStatusUpdate", message);
        }
    }
}
