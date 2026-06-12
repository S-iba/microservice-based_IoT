using System;
using System.Collections.Generic;
using System.Text;
using MassTransit;
using IoT.Shared;
using IoT.Shared.Models;

namespace IoT.Worker
{
    public class CommandComsumer : IConsumer<ActionCommand>
    {
        private readonly ILogger<CommandComsumer> _logger;

        public CommandComsumer(ILogger<CommandComsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ActionCommand> context)
        {
            var command = context.Message;
            _logger.LogInformation("Received command ID {Id}: Execute Action -> {ActionType}",
                command.Id, command.ActionType);

            //TODO: In Phase 4 - add httpClient call
            //trigger ESP32 over the network to execute the command


        }

    }
    }

