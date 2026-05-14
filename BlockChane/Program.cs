using System.Text;
using System.Diagnostics;
using BlockChane.Models;
using BlockChane.Service;

Console.OutputEncoding = Encoding.UTF8;

var displayService = new DisplayService();
var blockchain = new BlockChainService();

var crypto = new CryptoService();

var walletAlice = new Wallet(crypto);
var walletBob = new Wallet(crypto);

while (true)
{
    Console.WriteLine("\n===== МЕНЮ =====");
    Console.WriteLine("1 - Додати транзакцію");
    Console.WriteLine("2 - Змайнити блок");
    Console.WriteLine("3 - Показати блокчейн");
    Console.WriteLine("4 - Перевірити валідність");
    Console.WriteLine("5 - Баланси + TotalSupply");
    Console.WriteLine("6 - ClearState");
    Console.WriteLine("7 - RebuildState");
    Console.WriteLine("8 - SaveStateSnapshot");
    Console.WriteLine("9 - LoadStateSnapshot");
    Console.WriteLine("10 - Benchmark");
    Console.WriteLine("0 - Вихід");

    Console.Write("Вибір: ");
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            try
            {
                Console.WriteLine("\nВідправник:");
                Console.WriteLine("1 - Alice");
                Console.WriteLine("2 - Bob");
                var fromChoice = Console.ReadLine();

                Console.WriteLine("Отримувач:");
                Console.WriteLine("1 - Alice");
                Console.WriteLine("2 - Bob");
                var toChoice = Console.ReadLine();

                Console.Write("Сума: ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
                {
                    Console.WriteLine("Невірна сума");
                    break;
                }

                var fromWallet = fromChoice == "1" ? walletAlice : walletBob;
                var toWallet = toChoice == "1" ? walletAlice : walletBob;

                var tx = TransactionService.CreateTransaction(
                    fromWallet.PublicKey,
                    toWallet.PublicKey,
                    amount,
                    fromWallet.PrivateKey
                );

                blockchain.AddTransaction(tx);

                Console.WriteLine("Транзакцію додано в Mempool");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }

            break;

        case "2":
            Console.WriteLine("\nХто майнить?");
            Console.WriteLine("1 - Alice");
            Console.WriteLine("2 - Bob");

            var minerChoice = Console.ReadLine();
            var miner = minerChoice == "1" ? walletAlice : walletBob;

            blockchain.MineBlock(miner.PublicKey);

            Console.WriteLine("Блок змайнено");
            break;

        case "3":
            displayService.DisplayBlockChain(blockchain.Chain);
            break;

        case "4":
            if (blockchain.IsValid())
                Console.WriteLine("Blockchain валідний");
            else
                Console.WriteLine("Blockchain невалідний");

            break;

        case "5":
            Console.WriteLine($"Alice balance: {blockchain.GetBalance(walletAlice.PublicKey)}");
            Console.WriteLine($"Bob balance: {blockchain.GetBalance(walletBob.PublicKey)}");
            Console.WriteLine($"Total Supply: {blockchain.GetTotalSupply()}");
            break;

        case "6":
            blockchain.ClearState();
            Console.WriteLine("State очищено");
            break;

        case "7":
            blockchain.RebuildState();
            Console.WriteLine("State відновлено з блоків");
            break;

        case "8":
            blockchain.SaveStateSnapshot();
            Console.WriteLine("Snapshot збережено");
            break;

        case "9":
            blockchain.LoadStateSnapshot();
            Console.WriteLine("Snapshot завантажено");
            break;

        case "10":
            Console.WriteLine("\nСтворення 10000 транзакцій...");

            for (int i = 0; i < 10000; i++)
            {
                var tx = TransactionService.CreateTransaction(
                    walletAlice.PublicKey,
                    walletBob.PublicKey,
                    1,
                    walletAlice.PrivateKey
                );

                blockchain.AddTransaction(tx);
                blockchain.MineBlock(walletAlice.PublicKey);
            }

            Console.WriteLine("Benchmark started...");

            var oldWatch = Stopwatch.StartNew();

            decimal oldBalance = 0;

            foreach (var block in blockchain.Chain)
            {
                if (block.Transactions == null)
                    continue;

                foreach (var tx in block.Transactions)
                {
                    if (tx.To == walletBob.PublicKey)
                        oldBalance += tx.Amount;

                    if (tx.From == walletBob.PublicKey)
                        oldBalance -= tx.Amount;
                }
            }

            oldWatch.Stop();

            var newWatch = Stopwatch.StartNew();

            decimal newBalance = blockchain.GetBalance(walletBob.PublicKey);

            newWatch.Stop();

            Console.WriteLine($"Old method balance: {oldBalance}");
            Console.WriteLine($"Old method time: {oldWatch.ElapsedMilliseconds} ms");

            Console.WriteLine($"New method balance: {newBalance}");
            Console.WriteLine($"New method time: {newWatch.ElapsedMilliseconds} ms");

            break;

        case "0":
            return;

        default:
            Console.WriteLine("Невірний вибір");
            break;
    }
}