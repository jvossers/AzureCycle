using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.ServiceModel;
using AzureCycle.OnPrem.Core;

namespace AzureCycle.Cloud.WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        // The name of your queue
        const string QueueName = "mainqueue";

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        QueueClient Client;
        ManualResetEvent CompletedEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");
            
            // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            Client.OnMessage((receivedMessage) =>
                {
                    try
                    {
                        // Process the message
                        Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber.ToString());

                        ICycleService svc = this.GetChannel();

                        svc.CompleteCycle(receivedMessage.GetBody<DateTime>());
                    }
                    catch
                    {
                        // Handle any message processing specific exceptions here
                    }
                });

            CompletedEvent.WaitOne();
        }

        protected ICycleService GetChannel()
        {
            EndpointAddress address = new EndpointAddress(ServiceBusEnvironment.CreateServiceUri("sb", "TODO REPLACE WITH YOUR AZURE SERVICE BUS NAMESPACE", "cycle"));
            ChannelFactory<ICycleService> factory = new ChannelFactory<ICycleService>(new NetTcpRelayBinding(), address);            

            TransportClientEndpointBehavior behavior = new TransportClientEndpointBehavior()
            {
                TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("RootManageSharedAccessKey", "TODO REPLACE WITH YOUR AZURE SERVICE BUS NAMESPACE SharedAccessKey")
            };

            factory.Endpoint.EndpointBehaviors.Add(behavior);

            return factory.CreateChannel();
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Create the queue if it does not exist already
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.QueueExists(QueueName))
            {
                namespaceManager.CreateQueue(QueueName);
            }

            //// Initialize the connection to Service Bus Queue
            Client = QueueClient.CreateFromConnectionString(connectionString, QueueName);
            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            Client.Close();
            CompletedEvent.Set();
            base.OnStop();
        }
    }
}
