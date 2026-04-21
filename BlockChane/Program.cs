using System.Diagnostics;
using BlockChane.Models;
using BlockChane.Service;

var displayService = new DisplayService();
var blockChainService = new BlockChainServise();

string prefix = "c0ffee";

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();

blockChainService.AddBlock("Alice pays Bob 10 BTC", "Alice", prefix);
blockChainService.AddBlock("Bob pays Charlie 5 BTC", "Bob", prefix);
blockChainService.AddBlock("Charlie pays Dave 2 BTC", "Charlie", prefix);
blockChainService.AddBlock("Dave pays Eve 1 BTC", "Dave", prefix);

stopwatch.Stop();

displayService.DisplayBlockChain(blockChainService.Chain);

Console.WriteLine($"Vanity prefix: {prefix}");
Console.WriteLine($"Time for 4 blocks: {stopwatch.ElapsedMilliseconds} ms");

if (blockChainService.IsValid())
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Blockchain is valid.");
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Blockchain is invalid.");
}

Console.ResetColor();