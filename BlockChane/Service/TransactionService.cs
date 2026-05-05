using System.ComponentModel.DataAnnotations;
using BlockChane.Models;

namespace BlockChane.Service
{
    public static class TransactionService
    {
        private static readonly CryptoService cryptoService;

        static TransactionService()
        {
            cryptoService = new CryptoService();
        }

        public static Transaction CreateTransaction(string from, string to, decimal amount, string privateKey)
        {
            var tx = new Transaction(from, to, amount);

            SignTransaction(tx, privateKey);

            var validation = ValidateTransaction(tx);

            if (!validation.isValid)
            {
                throw new ValidationException(validation.error);
            }

            return tx;
        }

        public static (bool isValid, string error) ValidateTransaction(Transaction transaction)
        {
            if (transaction == null) return (false, "Transaction is null.");
            if (string.IsNullOrEmpty(transaction.From)) return (false, "Sender address is required.");
            if (string.IsNullOrEmpty(transaction.To)) return (false, "Recipient address is required.");
            if (transaction.Amount <= 0) return (false, "Amount must be greater than zero.");
            if (transaction.Signature == null || transaction.Signature.Length == 0) return (false, "Transaction must be signed.");

            if (transaction.From == "COINBASE")
                return (true, null);

            if (!cryptoService.VerifyData(transaction.ToRawString(), transaction.Signature, transaction.From))
                return (false, "Invalid transaction signature.");

            return (true, null);
        }

        public static void SignTransaction(Transaction transaction, string privateKey)
        {
            var signature = cryptoService.SignData(transaction.ToRawString(), privateKey);
            transaction.Signature = signature;
        }
    }
}