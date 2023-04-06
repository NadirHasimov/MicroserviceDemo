using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
                Console.WriteLine("---> Connected to the Message bus");

            }
            catch (System.Exception exc)
            {
                Console.WriteLine($"--->Could not connect the Message Bus: {exc.Message}");
            }
        }
        public void PublishNewPlatform(PLatformPublishedDto pLatformPublishDto)
        {
            var message = JsonSerializer.Serialize(pLatformPublishDto);
            if (_connection.IsOpen)
            {
                Console.WriteLine($"--->RabbitMQ connection open, sending message");
                SendMessage(message);
            }
            else
            {
                Console.WriteLine($"--->RabbitMQ connections closed, not sending message");
            }
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: "trigger",
                routingKey: "",
                basicProperties: null,
                body: body);

            Console.WriteLine($"--->We have sent {message}");
        }

        public void Dispose()
        {
            Console.WriteLine("--->MessageBus disposed");
            if (_connection.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("---> RabbitMQ connection shutdown");
        }
    }
}