using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Messaging.ServiceBus;
using asbe.send;

using IHost host = Host.CreateDefaultBuilder(args).Build();
var config = host.Services.GetRequiredService<IConfiguration>();

var asbeConfig = config.GetRequiredSection("ASBEConfig").Get<ASBEConfig>();

string hostName = config["HOSTNAME"];
var client = new ServiceBusClient(asbeConfig.SBConnectionString);
var sender = client.CreateSender(asbeConfig.QueueName);

var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = asbeConfig.ParallelDegrees };

foreach (var item in config.AsEnumerable())
{
    Console.WriteLine($"{item.Key}: {item.Value}");
}

Parallel.For(asbeConfig.Start, asbeConfig.End, parallelOptions, async i =>
{
    var message = new ServiceBusMessage("hi")
    {
        Subject = "sent from host " + hostName
    };
    message.ApplicationProperties.Add("Type", "Hello");
    message.ApplicationProperties.Add("Iteration", i);
    message.ApplicationProperties.Add("ManagedThreadId", Thread.CurrentThread.ManagedThreadId);

    Console.WriteLine($"Message Type: Hello");
    Console.WriteLine($"Message Iteration: {i}");
    Console.WriteLine($"Message ManagedThreadId: {Thread.CurrentThread.ManagedThreadId}");

    await sender.SendMessageAsync(message);
});


await host.RunAsync();
