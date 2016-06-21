using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialDemo.Model
{
    public class House
    {
        private string id;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private double count;

        public double Count
        {
            get { return count; }
            set { count = value; }
        }

        private string tiempo;

        public string Tiempo
        {
            get { return tiempo; }
            set { tiempo = value; }
        }

        private DateTime eventProcessedUtcTime;

        public DateTime EventProcessedUtcTime
        {
            get { return eventProcessedUtcTime; }
            set { eventProcessedUtcTime = value; }
        }

        private int partitionId;

        public int PartitionId
        {
            get { return partitionId; }
            set { partitionId = value; }
        }

        private DateTime eventEnqueuedUtcTime;

        public DateTime EventEnqueuedUtcTime
        {
            get { return eventEnqueuedUtcTime; }
            set { eventEnqueuedUtcTime = value; }
        }

    }
}
