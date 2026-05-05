using System.Text;
using BlockChane.Models;
using BlockChane.Service;

Console.OutputEncoding = Encoding.UTF8;

var displayService = new DisplayService();
var blockchain = new BlockChainService();

var crypto = new CryptoService();

var walletAlice = new Wallet(crypto);
var walletBob = new Wallet(crypto);

var pendingTransactions = new List<Transaction>();

while (true)
{
    Console.WriteLine("\n===== МЕНЮ =====");
    Console.WriteLine("1 додати транзакцію");
    Console.WriteLine("2 змайнити блок");
    Console.WriteLine("3 показати блокчейн");
    Console.WriteLine("4 перевірити валідність");
    Console.WriteLine("5 баланси + TotalSupply");
    Console.WriteLine("6 симуляція падіння (ClearState)");
    Console.WriteLine("7 відновлення (RebuildState)");
    Console.WriteLine("0 вихід");

    Console.Write("Вибір: ");
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            try
            {
                Console.WriteLine("\nдодати транзакцию");

                Console.WriteLine("Відправник:");
                Console.WriteLine("1 Alice");
                Console.WriteLine("2 Bob");
                var fromChoice = Console.ReadLine();

                Console.WriteLine("Отримувач:");
                Console.WriteLine("1 Alice");
                Console.WriteLine("2 Bob");
                var toChoice = Console.ReadLine();

                Console.Write("Сума: ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
                {
                    Console.WriteLine("невірна сума");
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

                pendingTransactions.Add(tx);

                Console.WriteLine("транзакцію додано");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"помилка: {ex.Message}");
            }
            break;

        case "2":
            Console.WriteLine("\nмайнинг");

            Console.WriteLine("хто майнить");
            Console.WriteLine("1 Alice");
            Console.WriteLine("2 Bob");

            var minerChoice = Console.ReadLine();
            var miner = minerChoice == "1" ? walletAlice : walletBob;

            blockchain.MineBlock(miner.PublicKey, new List<Transaction>(pendingTransactions));

            pendingTransactions.Clear();

            Console.WriteLine("блок змайнено");
            break;

        case "3":
            Console.WriteLine("\nблокчейн");
            displayService.DisplayBlockChain(blockchain.Chain);
            break;

        case "4":
            Console.WriteLine("\nвалидация");

            if (blockchain.IsValid())
                Console.WriteLine("валідний +");
            else
                Console.WriteLine("невалідний -");

            break;

        case "5":
            Console.WriteLine("\nбаланс");

            Console.WriteLine($"Alice: {blockchain.GetBalance(walletAlice.PublicKey)}");
            Console.WriteLine($"Bob: {blockchain.GetBalance(walletBob.PublicKey)}");
            Console.WriteLine($"Total Supply: {blockchain.GetTotalSupply()}");

            break;

        case "6":
            Console.WriteLine("\nпадіння");
            blockchain.ClearState();
            Console.WriteLine("баланс скинутий");
            break;

        case "7":
            Console.WriteLine("\nвідновлення стану");
            blockchain.RebuildState();
            Console.WriteLine("стан відновлено");
            break;

        case "0":
            return;

        default:
            Console.WriteLine("невірний вибір");
            break;
    }
}