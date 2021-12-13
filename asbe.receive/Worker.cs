using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Text;

namespace asbe.receive
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private TelemetryClient _telemetryClient;

        public Worker(ILogger<Worker> logger, IConfiguration config, TelemetryClient tc)
        {
            _logger = logger;
            _config = config;
            _telemetryClient = tc;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var asbeReceiveConfig = _config.GetRequiredSection("ASBEReceiveConfig").Get<ASBEReceiveConfig>();

            foreach (var item in _config.AsEnumerable())
            {
                _logger.LogInformation($"{item.Key}: {item.Value}");
            }

            string hostName = _config["HOSTNAME"];
            var client = new ServiceBusClient(asbeReceiveConfig.SBConnectionString);

            //var options = new ServiceBusProcessorOptions
            //{
            //    AutoCompleteMessages = false,
            //    MaxConcurrentCalls = asbeReceiveConfig.MaxConcurrentCalls,
            //    PrefetchCount = asbeReceiveConfig.PrefetchCount
            //};

            //// create a processor that we can use to process the messages
            //await using ServiceBusProcessor processor = client.CreateProcessor(asbeReceiveConfig.QueueName, options);

            //// configure the message and error handler to use
            //processor.ProcessMessageAsync += MessageHandler;
            //processor.ProcessErrorAsync += ErrorHandler;

            //await processor.StartProcessingAsync();
            var receiver = client.CreateReceiver(asbeReceiveConfig.QueueName, new ServiceBusReceiverOptions() { PrefetchCount = asbeReceiveConfig.PrefetchCount });
            while (!stoppingToken.IsCancellationRequested)
            {
                using (_telemetryClient.StartOperation<RequestTelemetry>("ReceiveMessage"))
                {
                    var receivedMessages = receiver.ReceiveMessagesAsync(asbeReceiveConfig.MaxConcurrentCalls);

                    foreach (var receivedMessage in await receivedMessages)
                    {
                        string body = receivedMessage.Body.ToString();
                        var msg = new StringBuilder();
                        foreach (var item in receivedMessage.ApplicationProperties)
                        {
                            msg.AppendLine($"{item.Key}={item.Value}");
                        }
                        msg.AppendLine($"body={body}");
                        Console.WriteLine(msg.ToString());
                        _logger.LogInformation(msg.ToString());
                        // we can evaluate application logic and use that to determine how to settle the message.
                        await receiver.CompleteMessageAsync(receivedMessage);
                    }

                }


            }
        }


        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            using (_telemetryClient.StartOperation<RequestTelemetry>("ReceiveMessage"))
            {
                string body = args.Message.Body.ToString();
                var msg = new StringBuilder();
                foreach (var item in args.Message.ApplicationProperties)
                {
                    msg.AppendLine($"{item.Key}={item.Value}");
                }
                msg.AppendLine($"body={body}");
                Console.WriteLine(msg.ToString());
                _logger.LogInformation(msg.ToString());
                // we can evaluate application logic and use that to determine how to settle the message.
                await args.CompleteMessageAsync(args.Message);
            }
        }

        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            using (_telemetryClient.StartOperation<RequestTelemetry>("ErrorHandler"))
            {
                // the error source tells me at what point in the processing an error occurred
                _logger.LogError(args.ErrorSource.ToString());
                // the fully qualified namespace is available
                _logger.LogError(args.FullyQualifiedNamespace);
                // as well as the entity path
                _logger.LogError(args.EntityPath);
                _logger.LogError(args.Exception.ToString());
                return Task.CompletedTask;
            }
        }
    }
}