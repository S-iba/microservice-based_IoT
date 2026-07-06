using IoT.Worker;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMassTransit(x =>
{
    //Register the consumer
    x.AddConsumer<CommandComsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], builder.Configuration["RabbitMQ:VirtualHost"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        // Create a Queue and bind it to exchange with the same name as the consumer
        cfg.ReceiveEndpoint("action-commands", e =>
        {
            e.ConcurrentMessageLimit = 1;

            e.ConfigureConsumer<CommandComsumer>(context);
        });
    });
});

builder.Services.AddHttpClient<CommandComsumer>(); // Register HttpClient for CommandComsumer

var host = builder.Build();
host.Run();



