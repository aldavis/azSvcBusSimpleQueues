using System;
using System.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Reciever
{
    class Program
    {
        private static NamespaceManager _namespaceManager;

        static void Main(string[] args)
        {
            _namespaceManager = NamespaceManager.Create();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Enter a Queue to recieve from.");

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
            var name = ConfigurationManager.AppSettings["RecieverKeyName"];
            var key = ConfigurationManager.AppSettings["RecieverKey"];

            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(name, key);

            var recieverFactory = MessagingFactory.Create(
                new Uri(serviceBusNamespace),
                new MessagingFactorySettings
                {
                    TransportType = TransportType.Amqp,
                    TokenProvider = tokenProvider

                });

            Console.WriteLine("Creating Reciever");
            var reciever = recieverFactory.CreateMessageReceiver(queueName, ReceiveMode.PeekLock);
            Console.WriteLine("Reciever Listening");
            reciever.OnMessage(ProcessMessage, new OnMessageOptions { MaxConcurrentCalls = 1, AutoComplete = false });


            Console.ReadLine();
            reciever.Close();
            _namespaceManager.DeleteQueue(queueName);
            
        }

        private static void ProcessMessage(BrokeredMessage message)
        {
            var body = message.GetBody<string>();

            dynamic content = JsonConvert.DeserializeObject(body);

            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(
                    $@"Message received: 
                       MessageId = {message.MessageId},
                       SequenceNumber = {message.SequenceNumber}, 
                       EnqueuedTimeUtc = {message.EnqueuedTimeUtc},
                       ExpiresAtUtc = {message.ExpiresAtUtc}, 
                       ContentType = \{message.ContentType}\, 
                       Size = {message.Size},  
                       Content: [ firstname = {content.firstName}, name = {content.name} ]");

                message.Complete();
            }

        }
    }
}
