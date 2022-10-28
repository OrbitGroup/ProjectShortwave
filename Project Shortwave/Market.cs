using System;
using System.Collections.Generic;
using System.Text;

namespace Project_Shortwave
{
    public class Market
    {
        public string symbol { get; set; }
        public string exchange { get; set; }
        public decimal lastPrice { get; set; }
        public decimal bidPrice { get; set; }
        public decimal bidSize { get; set; }
        public decimal askPrice {get; set; }
        public decimal askSize { get; set; }
        public decimal dailyVolume { get; set; } //24hr volume expressed in BTC
        public DateTime timeStamp { get; set; }

        public override string ToString()
        {
            return $"{symbol},{exchange},{lastPrice},{bidPrice},{bidSize},{askPrice},{askSize},{dailyVolume},{timeStamp}";
        }
    }
}
