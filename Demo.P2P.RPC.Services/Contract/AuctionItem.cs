using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.P2P.RPC.Services.Contract
{
    public class AuctionItem
    {
        public string Identifier { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }
}
