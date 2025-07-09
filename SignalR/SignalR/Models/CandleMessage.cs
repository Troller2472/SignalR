namespace SignalR.Models
{
    public class CandleMessage
    {
        public CandleArg arg { get; set; }
        public List<List<string>> data { get; set; }
    }

    public class CandleArg
    {
        public string channel { get; set; }
        public string instId { get; set; }
    }

    public class CandleData
    {
        public DateTime Timestamp { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal VolumeBase { get; set; }
        public decimal VolumeQuote { get; set; }

        public static CandleData FromRaw(List<string> raw)
        {
            return new CandleData
            {
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(raw[0])).UtcDateTime,
                Open = decimal.Parse(raw[1]),
                High = decimal.Parse(raw[2]),
                Low = decimal.Parse(raw[3]),
                Close = decimal.Parse(raw[4]),
                VolumeBase = decimal.Parse(raw[5]),
                VolumeQuote = decimal.Parse(raw[6])
            };
        }
    }

}
