using BlockChane.Models;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace BlockChane.Service.P2P
{
    public class P2PServer
    {
        private readonly BlockChainService blockchain;

        public P2PServer(BlockChainService blockchain)
        {
            this.blockchain = blockchain;
        }

        public void Start(int port)
        {
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Console.WriteLine($"P2P Server started on port {port}");

            Task.Run(async () =>
            {
                while (true)
                {
                    var client = await listener.AcceptTcpClientAsync();
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

                var jsonLine = await reader.ReadLineAsync();

                if (string.IsNullOrEmpty(jsonLine))
                    return;

                var tx = JsonSerializer.Deserialize<Transaction>(jsonLine);

                if (tx != null)
                {
                    blockchain.AddTransaction(tx);
                    Console.WriteLine("Transaction received and added to mempool.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"P2P error: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }
    }
}