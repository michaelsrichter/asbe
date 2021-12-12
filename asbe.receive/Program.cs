using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Messaging.ServiceBus;
using System.Text;
using asbe.receive;

using IHost host = Host.CreateDefaultBuilder(args).Build();
var config = host.Services.GetRequiredService<IConfiguration>();

var asbeReceiveConfig = config.GetRequiredSection("ASBEReceiveConfig").Get<ASBEReceiveConfig>();

foreach (var item in config.AsEnumerable())
{
    Console.WriteLine($"{item.Key}: {item.Value}");
}

string hostName = config["HOSTNAME"];
var client = new ServiceBusClient(asbeReceiveConfig.SBConnectionString);



// create the options to use for configuring the processor
var options = new ServiceBusProcessorOptions
{
    // By default or when AutoCompleteMessages is set to true, the processor will complete the message after executing the message handler
    // Set AutoCompleteMessages to false to [settle messages](https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-transfers-locks-settlement#peeklock) on your own.
    // In both cases, if the message handler throws an exception without settling the message, the processor will abandon the message.
    AutoCompleteMessages = false,

    // I can also allow for multi-threading
    MaxConcurrentCalls = asbeReceiveConfig.MaxConcurrentCalls,
    PrefetchCount = asbeReceiveConfig.PrefetchCount

};

// create a processor that we can use to process the messages
await using ServiceBusProcessor processor = client.CreateProcessor(asbeReceiveConfig.QueueName, options);

// configure the message and error handler to use
processor.ProcessMessageAsync += MessageHandler;
processor.ProcessErrorAsync += ErrorHandler;

async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    var msg = new StringBuilder();
    foreach (var item in args.Message.ApplicationProperties)
    {
        msg.AppendLine($"{item.Key}={item.Value}");
    }
    msg.AppendLine($"body={body}");
    Console.WriteLine(msg.ToString());
    // we can evaluate application logic and use that to determine how to settle the message.
    await args.CompleteMessageAsync(args.Message);
}

Task ErrorHandler(ProcessErrorEventArgs args)
{
    // the error source tells me at what point in the processing an error occurred
    Console.WriteLine(args.ErrorSource);
    // the fully qualified namespace is available
    Console.WriteLine(args.FullyQualifiedNamespace);
    // as well as the entity path
    Console.WriteLine(args.EntityPath);
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}

// start processing
await processor.StartProcessingAsync();

// since the processing happens in the background, we add a Console.ReadKey to allow the processing to continue until a key is pressed.
Console.Read();

