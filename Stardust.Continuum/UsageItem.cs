using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Stardust.Continuum
{
    internal class UsageItem
    {
        private static Timer timer;
        private long _itemsReceived;

        static UsageItem()
        {
            timer = new Timer(1000) { AutoReset = true };
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        public UsageItem()
        {

        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                List<UsageItem> messages = new List<UsageItem> { StreamServiceImp.Total.MakeUpdateMessage(), StreamServiceImp.Errors.MakeUpdateMessage() };
                foreach (var service in StreamServiceImp.ActivityPrLocation)
                {
                    foreach (var item in service.Value)
                    {
                        messages.Add(item.Value.MakeUpdateMessage());
                    }
                }
                Startup.hub.Clients.All.usageUpdate(messages);
            }
            catch (Exception ex)
            {
            }
        }

        private UsageItem MakeUpdateMessage()
        {
            var message = new UsageItem { ItemsReceived = Interlocked.Read(ref _itemsReceived), Name = Name, Location = Location, TimeStamp = DateTime.UtcNow.Truncate(TimeSpan.FromSeconds(1)) };
            Interlocked.Exchange(ref _itemsReceived, 0);
            return message;
        }

        public string Name { get; set; }

        public long ItemsReceived
        {
            get { return _itemsReceived; }
            set { _itemsReceived = value; }
        }

        public string Location { get; set; }

        public DateTime TimeStamp { get; set; }

        public void Increment()
        {
            Interlocked.Increment(ref _itemsReceived);
        }
    }
}