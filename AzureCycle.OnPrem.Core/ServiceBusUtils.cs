using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCycle.OnPrem.Core
{
    public static class ServiceBusUtils
    {
        private const string _queueName = "mainqueue";
        private static TokenProvider _tokenProvider;
        private static QueueClient _queueClient;
        private static NamespaceManager _namespaceManager;
        
        public static TokenProvider TokenProvider {
            get
            {
                if(_tokenProvider == null) _tokenProvider = TokenProvider.CreateSharedSecretTokenProvider("RootManageSharedAccessKey", "BbvgX3QHGCOUWmtrTs6UdGD0bs1vQTWSUduZP2ZEqgk=");
                return _tokenProvider;
            }
        }

        public static NamespaceManager NamespaceManager
        {
            get
            {
                if(_namespaceManager == null) _namespaceManager = NamespaceManager.Create();
                return _namespaceManager;
            }
        }

        public static QueueClient QueueClient
        {
            get
            {
                if (_queueClient == null)
                {                    
                    if (!ServiceBusUtils.NamespaceManager.QueueExists(_queueName))
                    {
                        ServiceBusUtils.NamespaceManager.CreateQueue(_queueName);
                    }
                    _queueClient = QueueClient.Create(_queueName);
                }
                return _queueClient;
            }
        }

        public static void SendCurrentDateTime()
        {
            ServiceBusUtils.SendMessage(DateTime.Now);
        }

        public static void SendMessage(object body)
        {
            BrokeredMessage msg = new BrokeredMessage(body);

            ServiceBusUtils.QueueClient.Send(msg);
        }
    }
}
