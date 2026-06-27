using BlockChane.Models;
using System.Text.Json;

namespace BlockChane.Service
{
    public class ColdWalletService
    {
        private readonly CryptoService _cryptoService;

        public ColdWalletService()
        {
            _cryptoService = new CryptoService();
        }

        public void GenerateOfflineTransaction(
            string from,
            string to,
            decimal amount,
            decimal fee,
            string privateKey,
            string filePath,
            string tokenSymbol = "MAIN")
        {
            var tx = new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                From = from,
                To = to,
                Amount = amount,
                Fee = fee,
                TokenSymbol = tokenSymbol,
                TimeStamp = DateTime.UtcNow
            };

            tx.Signature = _cryptoService.SignData(
                tx.ToRawString(),
                privateKey
            );

            var json = JsonSerializer.Serialize(
                tx,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(filePath, json);

            Console.WriteLine();
            Console.WriteLine("Offline transaction created.");
            Console.WriteLine($"Saved to: {filePath}");
        }

        public Transaction? LoadOfflineTransaction(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found.");
                return null;
            }

            try
            {
                var json = File.ReadAllText(filePath);

                return JsonSerializer.Deserialize<Transaction>(json);
            }
            catch
            {
                Console.WriteLine("Invalid transaction file.");

                return null;
            }
        }

        public bool VerifyOfflineTransaction(Transaction tx)
        {
            return _cryptoService.VerifyData(
                tx.ToRawString(),
                tx.Signature,
                tx.From
            );
        }
    }
}