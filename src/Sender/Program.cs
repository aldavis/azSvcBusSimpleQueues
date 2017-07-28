using System;
using System.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Sender
{
    internal class Program
    {
        private static NamespaceManager _namespaceManager;

        private static void Main(string[] args)
        {

            _namespaceManager = NamespaceManager.Create();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Enter a Queue to send to.");

            var queueName = Console.ReadLine();

            if (!_namespaceManager.QueueExists(queueName))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{queueName} does not exist, creating it.");
                _namespaceManager.CreateQueue(queueName);
                Console.WriteLine("Done");
                Console.ForegroundColor = ConsoleColor.Green;
            }

            var serviceBusNamespace = ConfigurationManager.AppSettings["ServiceBusNamespace"];
            var name = ConfigurationManager.AppSettings["SenderKeyName"];
            var key = ConfigurationManager.AppSettings["SenderKey"];

            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(name, key);

            Console.WriteLine("Creating Sender");

            var senderFactory = MessagingFactory.Create(
                new Uri(serviceBusNamespace),
                new MessagingFactorySettings
                {
                    TransportType = TransportType.Amqp,
                    TokenProvider = tokenProvider
                });

            var sender = senderFactory.CreateMessageSender(queueName);
            Console.WriteLine("Done");

            Console.WriteLine("Hit enter to send the messages.");
            Console.ReadLine();

            dynamic data = new[]
            {
                new {name = "Banner", firstName = "Bruce"},
                new {name = "Banner", firstName = "David"},
                new {name = "Stark", firstName = "Tony"},
                new {name = "Rogers", firstName = "Steven"},
                new {name = "Parker", firstName = "Peter"},
                new {name = "Wayne", firstName = "Bruce"},
                new {name = "Allen", firstName = "Barry"},
                new {name = "Kent", firstName = "Clark"},
                new {name = "Lane", firstName = "Lois"},
                new {name = "Stacey", firstName = "Gwen"}
            };

            for (int i = 0; i < data.Length; i++)
            {
                var message = new BrokeredMessage(JsonConvert.SerializeObject(data[i]))
                {
                    ContentType = "application/json",
                    Label = "mylabel",
                    MessageId = i.ToString(),
                    TimeToLive = TimeSpan.FromMinutes(2)
                };

                sender.Send(message);
                lock (Console.Out)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Message sent: Id = {0}", message.MessageId);
                    Console.ResetColor();
                }
            }
            sender.Close();

        }
    }
}