using IoT.Api.Data;
using IoT.Shared.Models;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IoT.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPublishEndpoint _publishEndpoint;

        public CommandController(AppDbContext context, IPublishEndpoint publishEndpoint)
        {
            _db = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> PostCommand([FromBody] ActionCommand command)
        {
            // Save the command to the database
            _db.Commands.Add(command);
            await _db.SaveChangesAsync();

            //Publish the command to the RabbitMQ exchange
            await _publishEndpoint.Publish(command);

            // Return a response indicating the command was received
            return Accepted(new { message = "Command received and published to RabbitMQ", commandId = command.Id });

        }
    }
}
