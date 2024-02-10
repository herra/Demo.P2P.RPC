using Demo.P2P.RPC.Services.Contract;

namespace Demo.P2P.RPC.Services
{
    public interface IAuctionService
    {
        /// <summary>
        /// Notify everyone for Auction
        /// </summary>
        /// <param name="auctionItem"></param>
        /// <returns></returns>
        Task StartAuction(AuctionItem auctionItem);

        /// <summary>
        /// Notify everyone for new bid
        /// </summary>
        /// <param name="itemIdentifier"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task<AuctionBid> PlaceBid(string itemIdentifier, decimal amount);

        /// <summary>
        /// Notify everyone for closing
        /// </summary>
        /// <param name="auctionItem"></param>
        /// <param name="winningBid"></param>
        /// <returns></returns>
        Task CloseAuction(AuctionItem auctionItem, AuctionBid winningBid);
    }
}
