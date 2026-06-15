using BlockChane.Models;
using System.Net.Sockets;
using System.Text.Json;

namespace BlockChane.Service.P2P
{
    public class P2PClient
    {
        private readonly List<string> _peers = new();

        public void Connect(string peerAddress)
        {
            if (!_peers.Contains(peerAddress))
            {
                _peers.Add(peerAddress);

                Console.WriteLine(
                    $"Connected to peer: {peerAddress}"
                );

                try
                {
                    var parts =
                        peerAddress.Split(':');

                    _ = RequestMempoolAsync(
                        parts[0],
                        int.Parse(parts[1])
                    );
                }
                catch
                {
                }
            }
        }

        public async Task BroadcastTransactionAsync(Transaction transaction)
        {
            var jsonTransaction = JsonSerializer.Serialize(transaction);

            foreach (var peer in _peers)
            {
                try
                {
                    var parts = peer.Split(':');

                    var ip = parts[0];
                    var port = int.Parse(parts[1]);

                    using var client = new TcpClient();

                    await client.ConnectAsync(ip, port);

                    using var stream = client.GetStream();
                    using var writer = new StreamWriter(stream)
                    {
                        AutoFlush = true
                    };

                    await writer.WriteLineAsync(jsonTransaction);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending transaction: {ex.Message}");
                }
            }
        }

        public async Task BroadcastBlockAsync(Block block)
        {
            var message = JsonSerializer.Serialize(new
            {
                Type = "NEW_BLOCK",
                Data = JsonSerializer.Serialize(block)
            });

            foreach (var peer in _peers)
            {
                try
                {
                    var parts = peer.Split(':');

                    using var client = new TcpClient();

                    await client.ConnectAsync(parts[0], int.Parse(parts[1]));

                    using var stream = client.GetStream();

                    using var writer = new StreamWriter(stream)
                    {
                        AutoFlush = true
                    };

                    await writer.WriteLineAsync(message);
                }
                catch
                {
                }
            }
        }

        public async Task BroadcastChainAsync(List<Block> chain)
        {
            var message = JsonSerializer.Serialize(new
            {
                Type = "NEW_CHAIN",
                Data = JsonSerializer.Serialize(chain)
            });

            foreach (var peer in _peers)
            {
                try
                {
                    var parts = peer.Split(':');

                    using var client = new TcpClient();

                    await client.ConnectAsync(parts[0], int.Parse(parts[1]));

                    using var stream = client.GetStream();

                    using var writer = new StreamWriter(stream)
                    {
                        AutoFlush = true
                    };

                    await writer.WriteLineAsync(message);
                }
                catch
                {
                }
            }
        }

        public async Task RequestChainAsync(string ip, int port)
        {
            try
            {
                var message = JsonSerializer.Serialize(new
                {
                    Type = "REQUEST_CHAIN",
                    Data = ""
                });

                using var client = new TcpClient();

                await client.ConnectAsync(ip, port);

                using var stream = client.GetStream();

                using var writer = new StreamWriter(stream)
                {
                    AutoFlush = true
                };

                await writer.WriteLineAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task RequestMempoolAsync(string ip, int port)
        {
            try
            {
                var message = new NetworkMessage(
                    "REQUEST_MEMPOOL",
                    ""
                );

                var json =
                    JsonSerializer.Serialize(message);

                using var client = new TcpClient();

                await client.ConnectAsync(ip, port);

                using var stream = client.GetStream();

                using var writer =
                    new StreamWriter(stream)
                    {
                        AutoFlush = true
                    };

                await writer.WriteLineAsync(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}