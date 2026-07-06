using System;
using System.Collections.Generic;
using System.Text;
using MassTransit;
using IoT.Shared;
using IoT.Shared.Models;
using IoT.Shared.Events;

namespace IoT.Worker
{
    public class CommandComsumer : IConsumer<ActionCommand>
    {
        private readonly ILogger<CommandComsumer> _logger;
        private readonly HttpClient _httpClient;

        public CommandComsumer(ILogger<CommandComsumer> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task Consume(ConsumeContext<ActionCommand> context)
        {
            var command = context.Message;
            _logger.LogInformation("Received command ID {Id}: Execute Action -> {ActionType}",
                command.Id, command.ActionType);


            //trigger ESP32 over the network to execute the command

            // string esp32Ip = "192.168.8.123";
            string esp32Ip = Environment.GetEnvironmentVariable("Esp32:Host") ?? "192.168.8.123";
            string url = $"http://{esp32Ip}/control?action={command.ActionType}";

            try
            {
                // Send the trigger request to the microcontroller
                _httpClient.Timeout = TimeSpan.FromSeconds(5);
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    //publish the completion event to the message bus
                    await context.Publish(new CommandCompletedEvent
                    {
                        ActionType = command.ActionType,
                        Status = "Success",

                    });
                    _logger.LogInformation("Successfully triggered ESP32 for Command {Id}.", command.Id);
                }
                else
                {
                    _logger.LogError("ESP32 responded with an error: {StatusCode}", response.StatusCode);
                    // Throwing an exception forces MassTransit to keep the message in the queue and retry later!
                    throw new HttpRequestException($"ESP32 failed with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to reach ESP32: {Message}. Message will remain in queue for retry.", ex.Message);
                throw; // Retain fault-tolerance by letting MassTransit handle the failure
            }

        }

    }
    }

