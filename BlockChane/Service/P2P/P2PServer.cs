using BlockChane.Models;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace BlockChane.Service.P2P
{
    public class P2PServer
    {
        private readonly BlockChainService blockchain;
        private readonly P2PClient p2pClient;

        public P2PServer(
            BlockChainService blockchain,
            P2PClient p2pClient)
        {
            this.blockchain = blockchain;
            this.p2pClient = p2pClient;
        }

        public void Start(int port)
        {
            var listener = new TcpListener(
                IPAddress.Any,
                port
            );

            listener.Start();

            Console.WriteLine($"P2P Server started on port {port}");

            Task.Run(async () =>
            {
                while (true)
                {
                    var client =
                        await listener.AcceptTcpClientAsync();

                    _ = HandleClientAsync(client);
                }
            });
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using var stream = client.GetStream();

                using var reader = new StreamReader(stream);

                var jsonLine =
                    await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(jsonLine))
                    return;

                if (jsonLine.Contains("\"Type\""))
                {
                    var message =
                        JsonSerializer.Deserialize<JsonElement>(jsonLine);

                    string type =
                        message.GetProperty("Type").GetString();

                    if (type == "REQUEST_CHAIN")
                    {
                        await p2pClient.BroadcastChainAsync(
                            blockchain.Chain
                        );
                    }

                    else if (type == "NEW_CHAIN")
                    {
                        var chainJson =
                            message.GetProperty("Data").GetString();

                        var newChain =
                            JsonSerializer.Deserialize<List<Block>>(chainJson);

                        if (newChain != null &&
                            newChain.Count > blockchain.Chain.Count)
                        {
                            blockchain.Chain = newChain;

                            Console.WriteLine(
                                "[P2P] Chain replaced"
                            );
                        }
                    }

                    else if (type == "NEW_BLOCK")
                    {
                        var blockJson =
                            message.GetProperty("Data").GetString();

                        var newBlock =
                            JsonSerializer.Deserialize<Block>(blockJson);

                        if (newBlock != null)
                        {
                            blockchain.Chain.Add(newBlock);

                            Console.WriteLine(
                                "[P2P] New block received"
                            );
                        }
                    }

                    else if (type == "REQUEST_MEMPOOL")
                    {
                        var mempoolJson =
                            JsonSerializer.Serialize(
                                blockchain.PendingTransactions
                            );

                        Console.WriteLine(
                            "[P2P] Peer requested mempool"
                        );
                    }

                    else if (type == "SYNC_MEMPOOL")
                    {
                        var mempoolJson =
                            message.GetProperty("Data")
                            .GetString();

                        var txs =
                            JsonSerializer.Deserialize<List<Transaction>>
                            (mempoolJson);

                        if (txs != null)
                        {
                            foreach (var transaction in txs)
                            {
                                if (!blockchain.PendingTransactions
                                    .Any(x => x.Id == transaction.Id))
                                {
                                    blockchain.PendingTransactions.Add(transaction);
                                }
                            }

                            Console.WriteLine(
                                $"[P2P] Mempool synchronized. {txs.Count} transactions loaded."
                            );
                        }
                    }

                    return;
                }

                var tx =
                    JsonSerializer.Deserialize<Transaction>(jsonLine);

                if (tx != null)
                {
                    bool added =
                        blockchain.AddTransactionFromNetwork(tx);

                    if (added)
                    {
                        Console.WriteLine(
                            "[P2P] Transaction added to mempool"
                        );

                        await p2pClient.BroadcastTransactionAsync(tx);
                    }
                }
            }


            catch (Exception ex)
            {
                Console.WriteLine(
                    $"P2P error: {ex.Message}"
                );
            }
            finally
            {
                client.Close();
            }
        }
    }
}