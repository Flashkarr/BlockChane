using BlockChane.Models;
using System.Net.Sockets;
using System.Text.Json;
using System.IO;

namespace BlockChane.Service.P2P
{
    public class P2PClient
    {
        private readonly List<string> _peers = new();

        private const string PeersFile = "peers.json";

        public P2PClient()
        {
            LoadPeers();
        }

        private void SavePeers()
        {
            File.WriteAllText(
                PeersFile,
                JsonSerializer.Serialize(_peers)
            );
        }

        private void LoadPeers()
        {
            if (!File.Exists(PeersFile))
                return;

            var peers = JsonSerializer.Deserialize<List<string>>(
                File.ReadAllText(PeersFile)
            );

            if (peers != null)
                _peers.AddRange(peers);
        }

        public async Task ReconnectSavedPeers()
        {
            foreach (var peer in _peers)
            {
                try
                {
                    var parts = peer.Split(':');

                    await RequestMempoolAsync(
                        parts[0],
                        int.Parse(parts[1])
                    );

                    Console.WriteLine($"Reconnected to {peer}");
                }
                catch
                {
                }
            }
        }

        public async Task ReconnectToKnownPeersAsync()
        {
            foreach (var peer in _peers.ToList())
            {
                try
                {
                    var parts = peer.Split(':');

                    await RequestMempoolAsync(
                        parts[0],
                        int.Parse(parts[1]));

                    Console.WriteLine($"Reconnected to {peer}");
                }
                catch
                {
                    Console.WriteLine($"Peer {peer} offline");
                }
            }
        }

        public void Connect(string peerAddress)
        {
            if (!_peers.Contains(peerAddress))
            {
                _peers.Add(peerAddress);

                SavePeers();

                Console.WriteLine(
                    $"Connected to peer: {peerAddress}"
                );

                try
                {
                    var parts = peerAddress.Split(':');

                    _ = RequestMempoolAsync(
                        parts[0],
                        int.Parse(parts[1]));
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

                    using var client = new TcpClient();

                    await client.ConnectAsync(
                        parts[0],
                        int.Parse(parts[1]));

                    using var stream = client.GetStream();

                    using var writer = new StreamWriter(stream)
                    {
                        AutoFlush = true
                    };

                    await writer.WriteLineAsync(jsonTransaction);
                }
                catch
                {
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

                    await client.ConnectAsync(
                        parts[0],
                        int.Parse(parts[1]));

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

                    await client.ConnectAsync(
                        parts[0],
                        int.Parse(parts[1]));

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
            catch
            {
            }
        }

        public async Task RequestMempoolAsync(string ip, int port)
        {
            try
            {
                var message = new NetworkMessage(
                    "REQUEST_MEMPOOL",
                    "");

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
            catch
            {
            }
        }


    }
}