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

        public static Transaction CreateTransaction(string from, string to, decimal amount, decimal fee, string token, string privateKey)
        {
            var tx = new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                From = from,
                To = to,
                Amount = amount,
                Fee = fee,
                TokenSymbol = token,
                TimeStamp = DateTime.UtcNow
            };

            SignTransaction(tx, privateKey);

            var validation = ValidateTransaction(tx);

            if (!validation.isValid)
                throw new ValidationException(validation.error);

            return tx;
        }

        public static (bool isValid, string error)
        ValidateTransaction(Transaction transaction)
        {
            if (transaction == null)
                return (false, "Transaction is null.");

            if (string.IsNullOrWhiteSpace(transaction.From))
                return (false, "Sender required.");

            if (string.IsNullOrWhiteSpace(transaction.To))
                return (false, "Receiver required.");

            if (transaction.Amount <= 0)
                return (false, "Amount must be >0.");

            if (transaction.From == "COINBASE")
                return (true, "");

            if (transaction.From == "MINT")
                return (true, "");

            if (transaction.Signature == null)
                return (false, "No signature.");

            bool ok = cryptoService.VerifyData(
                transaction.ToRawString(),
                transaction.Signature,
                transaction.From);

            if (!ok)
                return (false, "Bad signature.");

            return (true, "");
        }

        public static void SignTransaction(Transaction transaction, string privateKey)
        {
            var signature = cryptoService.SignData(transaction.ToRawString(), privateKey);
            transaction.Signature = signature;
        }
    }
}