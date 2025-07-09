using SignalR.Models;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace SignalR.Services
{
    public class WebSocketServices
    {
        private ClientWebSocket _socket = new();
        private readonly Dictionary<string, List<Action<CandleData>>> _subscribersBySymbol = new();
        private readonly HashSet<string> _subscribedSymbols = new();
        private bool _receiveLoopStarted = false;

        public async Task ConnectAsync(string symbol, string timeframe)
        {
            symbol = symbol.ToUpperInvariant();

            if (_socket.State != WebSocketState.Open)
            {
                _socket = new ClientWebSocket();
                await _socket.ConnectAsync(new Uri("wss://ws.okx.com:8443/ws/v5/business"), CancellationToken.None);
            }

            // Đảm bảo không subscribe trùng symbol
            if (_subscribedSymbols.Contains(symbol)) return;

            var subscribe = new
            {
                op = "subscribe",
                args = new[]
                {
                    new { channel = "candle" + timeframe, instId = symbol }
                }
            };

            var json = JsonSerializer.Serialize(subscribe);
            await _socket.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, CancellationToken.None);
            _subscribedSymbols.Add(symbol);

            // Chạy ReceiveLoop 1 lần duy nhất
            if (!_receiveLoopStarted)
            {
                _receiveLoopStarted = true;
                _ = Task.Run(ReceiveLoop);
            }
        }



        private async Task ReceiveLoop()
        {
            var buffer = new byte[4096];
            while (_socket.State == WebSocketState.Open)
            {
                var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    try
                    {
                        var candleMsg = JsonSerializer.Deserialize<CandleMessage>(msg);
                        if (candleMsg?.data?.Count > 0 && candleMsg.arg is JsonElement argElem)
                        {
                            var instId = argElem.GetProperty("instId").GetString()?.ToUpperInvariant();
                            var candle = CandleData.FromRaw(candleMsg.data[0]);

                            if (!string.IsNullOrWhiteSpace(instId) && _subscribersBySymbol.TryGetValue(instId, out var handlers))
                            {
                                foreach (var cb in handlers)
                                {
                                    cb(candle);
                                }
                            }
                        }
                    }
                    catch { /* Ignored */ }
                }
            }
        }

        public void Subscribe(string symbol, Action<CandleData> handler)
        {
            symbol = symbol.ToUpperInvariant();

            if (!_subscribersBySymbol.ContainsKey(symbol))
                _subscribersBySymbol[symbol] = new List<Action<CandleData>>();

            _subscribersBySymbol[symbol].Add(handler);
        }

        public async Task DisconnectAsync()
        {
            if (_socket != null && _socket.State == WebSocketState.Open)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "User disconnect", CancellationToken.None);
                _socket.Dispose();
                _socket = null!;
            }

            _receiveLoopStarted = false;
            _subscribedSymbols.Clear();
            _subscribersBySymbol.Clear();
        }
        public async Task DisconnectAllAsync()
        {
            if (_socket != null && _socket.State == WebSocketState.Open)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "User disconnect", CancellationToken.None);
                _socket.Dispose();
                _socket = new ClientWebSocket(); // chuẩn bị lại socket để tái sử dụng
            }

            _receiveLoopStarted = false;
            _subscribedSymbols.Clear();
            _subscribersBySymbol.Clear();
        }


        private class CandleMessage
        {
            public object arg { get; set; }
            public List<List<string>> data { get; set; }
        }
    }
}
