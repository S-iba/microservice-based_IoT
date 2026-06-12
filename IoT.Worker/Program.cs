using IoT.Worker;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMassTransit(x =>
{
    //Register the consumer
    x.AddConsumer<CommandComsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("siba-pie", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Create a Queue and bind it to exchange with the same name as the consumer
        cfg.ReceiveEndpoint("action-commands", e =>
        {
            e.ConfigureConsumer<CommandComsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
