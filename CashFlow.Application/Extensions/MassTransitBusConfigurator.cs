using CashFlow.Application.Consumer;
using CashFlow.Domain.Events;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using CashFlow.Infrastructure.Settings;
using RabbitMQ.Client;
using Microsoft.Extensions.Hosting;
using RabbitMqSettings = CashFlow.Infrastructure.Settings.RabbitMQ;
using Microsoft.Extensions.Configuration;

namespace CashFlow.Application.Extensions;

public static class MassTransitBusConfigurator
{
    enum BusConfigType
    {
        Publisher, Subscriber
    }

    public static IHostApplicationBuilder AddMassTransitRabbitMqPublisher(this IHostApplicationBuilder builder)
    {
        return AddMassTransitRabbitMq(builder, BusConfigType.Publisher);
    }

    public static IHostApplicationBuilder AddMassTransitRabbitMqSubscribers(this IHostApplicationBuilder builder)
    {
        return AddMassTransitRabbitMq(builder, BusConfigType.Subscriber);
    }
    
    private static IHostApplicationBuilder AddMassTransitRabbitMq(IHostApplicationBuilder builder, BusConfigType busConfigType)
    {
        var rabbitMqSettings = GetRabbitMqSettings(builder.Configuration);

        builder.Services.AddMassTransit(config =>
        {
            if (busConfigType == BusConfigType.Subscriber)
                AddMassTransitRabbitMqConsumers(rabbitMqSettings, config);

            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.VirtualHost, hostConfigurator =>
                {
                    hostConfigurator.Username(rabbitMqSettings.Username);
                    hostConfigurator.Password(rabbitMqSettings.Password);
                    
                    hostConfigurator.ContinuationTimeout(TimeSpan.FromSeconds(3000));
                    hostConfigurator.RequestedConnectionTimeout(TimeSpan.FromSeconds(3000));
                });

                if (busConfigType == BusConfigType.Publisher)
                {
                    // Garante que as exchanges sejam declaradas
                    cfg.Message<InFlowProcessedEvent>(e => e.SetEntityName("InFlowProcessedEvent"));
                    cfg.Message<OutFlowProcessedEvent>(e => e.SetEntityName("OutFlowProcessedEvent"));
                }

                if (busConfigType == BusConfigType.Subscriber)
                    UseMassTransitRabbitMqConsumers(rabbitMqSettings, context, cfg);
            });

        });

        return builder;
    }

    private static void AddMassTransitRabbitMqConsumers(RabbitMqSettings.MainSettings rabbitMqSettings, IBusRegistrationConfigurator busRegistrationConfigurator)
    {
        foreach (var queue in rabbitMqSettings.Queues)
        {
            Type consumerType = GetEventConsumerBy(queue.Value.Name);

            busRegistrationConfigurator.AddConsumer(consumerType);

            Type dlqConsumerType = GetDlqEventConsumerBy(queue.Value.Name);

            busRegistrationConfigurator.AddConsumer(dlqConsumerType);
        }
    }

    private static void UseMassTransitRabbitMqConsumers(RabbitMqSettings.MainSettings rabbitMqSettings, IBusRegistrationContext busRegistrationContext, IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator)
    {
        foreach (var queue in rabbitMqSettings.Queues)
        {
            rabbitMqBusFactoryConfigurator.ReceiveEndpoint(queue.Value.Name, e =>
            {
                e.Bind($"{queue.Value.Name}-Exc");
                // Desabilita a criação da fila de erro padrão
                e.DiscardFaultedMessages();

                e.SetQueueArgument("x-dead-letter-exchange", queue.Value.DLQ.Exchange);
                e.SetQueueArgument("x-dead-letter-routing-key", queue.Value.DLQ.Queue);
                
                Type consumerType = GetEventConsumerBy(queue.Value.Name);
                e.ConfigureConsumer(busRegistrationContext, consumerType); 
            });

            rabbitMqBusFactoryConfigurator.ReceiveEndpoint(queue.Value.DLQ.Queue, e =>
            {
                e.Bind(queue.Value.DLQ.Exchange, x =>
                {
                    x.RoutingKey = queue.Value.DLQ.Queue;
                    x.ExchangeType = ExchangeType.Direct;
                });

                // Tratar mensagens na DLQ aqui
                // Define o consumidor para a DLQ
                Type consumerType = GetDlqEventConsumerBy(queue.Value.Name);
                e.ConfigureConsumer(busRegistrationContext, consumerType);
            });
        }
    }

    private static RabbitMqSettings.MainSettings GetRabbitMqSettings(IConfiguration configuration)
    {
        var rabbitMqSettigns = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>();
        var mainRabbitMqSettings = rabbitMqSettigns?.Main ?? new RabbitMqSettings.MainSettings();

        return mainRabbitMqSettings;
    }

    private static Type GetEventConsumerBy(string queueName)
    {
        var eventType = GetConsumerTypeByQueueName(queueName);

        return typeof(EventConsumer<>).MakeGenericType(eventType);
    }

    private static Type GetDlqEventConsumerBy(string queueName)
    {
        var eventType = GetConsumerTypeByQueueName(queueName);

        return typeof(DeadLetterEventConsumer<>).MakeGenericType(eventType);
    }

    private static Type GetConsumerTypeByQueueName(string queueName)
    {
        var eventType = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == queueName);

        if (eventType == null)
        {
            throw new InvalidOperationException($"Tipo de evento '{queueName}' não encontrado.");
        }

        return eventType;
    }
}