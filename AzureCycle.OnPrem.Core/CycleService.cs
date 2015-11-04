using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace AzureCycle.OnPrem.Core
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class CycleService : ICycleService
    {
        public void CompleteCycle(DateTime startedAt)
        {
            TimeSpan duration = DateTime.Now.Subtract(startedAt);

            Console.WriteLine("Completed cycle in {0} ms", duration.TotalMilliseconds);

            // start another cycle
            ServiceBusUtils.SendCurrentDateTime();
        }
    }
}
