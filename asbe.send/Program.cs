using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Messaging.ServiceBus;
using asbe.send;

using IHost host = Host.CreateDefaultBuilder(args).Build();
var config = host.Services.GetRequiredService<IConfiguration>();

var asbeConfig = config.GetRequiredSection("ASBEConfig").Get<ASBEConfig>();

foreach (var item in config.AsEnumerable())
{
    Console.WriteLine($"{item.Key}: {item.Value}");
}

string hostName = config["HOSTNAME"];
var client = new ServiceBusClient(asbeConfig.SBConnectionString);
var sender = client.CreateSender(asbeConfig.QueueName);

var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = asbeConfig.ParallelDegrees };
var iteration = 0;
while (true)
{
    Parallel.For(asbeConfig.Start, asbeConfig.End, parallelOptions, async i =>
    {
        var message = new ServiceBusMessage("hi")
        {
            Subject = "sent from host " + hostName
        };
        message.ApplicationProperties.Add("Type", "Hello");
        message.ApplicationProperties.Add("Iteration", $"iteration:{iteration}, run:{i}");
        message.ApplicationProperties.Add("ManagedThreadId", Thread.CurrentThread.ManagedThreadId);

        Console.WriteLine($"Message Type: Hello");
        Console.WriteLine($"Message Iteration:{iteration}, run:{i}");
        Console.WriteLine($"Message ManagedThreadId: {Thread.CurrentThread.ManagedThreadId}");

        await sender.SendMessageAsync(message);
    });
    iteration++;
}




await host.RunAsync();
