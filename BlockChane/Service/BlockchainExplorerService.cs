using BlockChane.Models;

namespace BlockChane.Service
{
    public class BlockchainExplorerService
    {
        private readonly BlockChainService _blockchain;

        public BlockchainExplorerService(BlockChainService blockchain)
        {
            _blockchain = blockchain;
        }

        public Transaction? FindTransactionById(string txId)
        {
            var tx = _blockchain.PendingTransactions
                .FirstOrDefault(t => t.Id == txId);

            if (tx != null)
                return tx;

            return _blockchain.Chain
                .Where(b => b.Transactions != null)
                .SelectMany(b => b.Transactions)
                .FirstOrDefault(t => t.Id == txId);
        }

        public Block? FindBlockByTransactionId(string txId)
        {
            return _blockchain.Chain.FirstOrDefault(block =>
                block.Transactions.Any(tx => tx.Id == txId));
        }

        public List<Transaction> GetTransactionHistory(string address)
        {
            return _blockchain.Chain
                .SelectMany(block => block.Transactions)
                .Where(tx =>
                    tx.From == address ||
                    tx.To == address)
                .OrderByDescending(tx => tx.TimeStamp)
                .ToList();
        }

        public decimal GetTotalFeesEarned(string minerAddress)
        {
            decimal total = 0;

            foreach (var block in _blockchain.Chain)
            {
                if (block.Author != minerAddress)
                    continue;

                foreach (var tx in block.Transactions)
                {
                    total += tx.Fee;
                }
            }

            return total;
        }

        public List<Transaction> GetAllTransactions()
        {
            return _blockchain.Chain
                .SelectMany(x => x.Transactions)
                .OrderByDescending(x => x.TimeStamp)
                .ToList();
        }

        public List<Block> GetBlocksByMiner(string minerAddress)
        {
            return _blockchain.Chain
                .Where(x => x.Author == minerAddress)
                .ToList();
        }

        public List<Transaction> GetLatestTransactions(int count)
        {
            return _blockchain.Chain
                .SelectMany(x => x.Transactions)
                .OrderByDescending(x => x.TimeStamp)
                .Take(count)
                .ToList();
        }
    }
}