using asbe.send;
using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Text.Json;

namespace asbe.send
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
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);

                var asbeConfig = _config.GetRequiredSection("ASBEConfig").Get<ASBEConfig>();

                foreach (var item in _config.AsEnumerable())
                {
                    _logger.LogInformation($"{item.Key}: {item.Value}");
                }

                string hostName = _config["HOSTNAME"];
                var client = new ServiceBusClient(asbeConfig.SBConnectionString);
                var sender = client.CreateSender(asbeConfig.QueueName);

                var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = asbeConfig.ParallelDegrees };
                var iteration = 0;
                var instanceName = Guid.NewGuid().ToString();
                while (true)
                {
                    Parallel.For(asbeConfig.Start, asbeConfig.End, parallelOptions, async i =>
                    {
                        var mD = new MessageDetails()
                        {
                            IterationId = $"{instanceName}-{iteration}-{i}",
                            Type = "simple",
                            Thread = Thread.CurrentThread.ManagedThreadId,
                            Sent = DateTimeOffset.Now,
                        };
                        var message = new ServiceBusMessage("hi")
                        {
                            Subject = "sent from host " + hostName
                        };
                        message.ApplicationProperties.Add("Type", mD.Type);
                        message.ApplicationProperties.Add("IterationId", mD.IterationId);
                        message.ApplicationProperties.Add("ManagedThreadId", mD.Thread);
                        message.ApplicationProperties.Add("Sent", mD.Sent);

                        using (_telemetryClient.StartOperation<RequestTelemetry>("SendMessage"))
                        {
                            await sender.SendMessageAsync(message);
                            _logger.LogInformation(JsonSerializer.Serialize(mD));
                        }

                    });
                    if (!asbeConfig.RunConstant) break;
                    iteration++;
                }



            }
        }
    }
}