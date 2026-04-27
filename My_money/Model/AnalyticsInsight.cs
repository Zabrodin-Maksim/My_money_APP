namespace My_money.Model
{
    public class AnalyticsInsight
    {
        public required string Kind { get; set; }
        public required string Title { get; set; }
        public required string Detected { get; set; }
        public required string WhyItMatters { get; set; }
        public required string SuggestedAction { get; set; }
    }
}
