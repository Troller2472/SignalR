namespace SignalR.Models
{
    public class SubscribeMessage
    {
        public string op { get; set; } = "subscribe";
        public List<SubscribeArg> args { get; set; }
    }

    public class SubscribeArg
    {
        public string channel { get; set; }
        public string instId { get; set; }
    }

    public enum CandleTimeframe
    {
        _3M, _1M, _1W, _1D, _2D, _3D, _5D,
        _12H, _6H, _4H, _2H, _1H,
        _30m, _15m, _5m, _3m, _1m, _1s
    }

    public static class CandleTimeframeExtensions
    {
        public static string ToApiString(this CandleTimeframe tf)
        {
            return tf.ToString().TrimStart('_');
        }
    }
}
