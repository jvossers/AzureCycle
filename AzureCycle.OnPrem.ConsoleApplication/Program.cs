using AzureCycle.OnPrem.Core;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace AzureCycle.OnPrem.ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(CycleService), new Uri("http://localhost:10001")))
            {
                NetTcpRelayBinding binding = new NetTcpRelayBinding();

                Uri uri = ServiceBusEnvironment.CreateServiceUri("sb", "[TODO REPLACE WITH YOUR AZURE SERVICE BUS NAMESPACE]", "cycle");
                ServiceEndpoint ep = host.AddServiceEndpoint(typeof(ICycleService), new NetTcpRelayBinding(), uri);
                TransportClientEndpointBehavior behavior = new TransportClientEndpointBehavior()
                {
                    TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("RootManageSharedAccessKey", "[TODO REPLACE WITH YOUR AZURE SERVICE BUS NAMESPACE SharedAccessKey]")
                };
                ep.Behaviors.Add(behavior);

                host.Open();

                // start the cycle
                ServiceBusUtils.SendCurrentDateTime();

                Console.WriteLine("Host opened. Press any key to exit.");
                Console.Read();
            }
        }
    }
}
