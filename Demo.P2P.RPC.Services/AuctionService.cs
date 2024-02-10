using Demo.P2P.RPC.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.P2P.RPC.Services
{
    public class AuctionService : IAuctionService
    {
        public Task CloseAuction(AuctionItem auctionItem, AuctionBid winningBid)
        {
            throw new NotImplementedException();
        }

        public Task<AuctionBid> PlaceBid(string itemIdentifier, decimal amount)
        {
            throw new NotImplementedException();
        }

        public Task StartAuction(AuctionItem auctionItem)
        {
            throw new NotImplementedException();
        }
    }
}
