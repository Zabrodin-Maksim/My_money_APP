using My_money.Enums;

namespace My_money.ViewModel
{
    public class ContextOption
    {
        public required string Title { get; init; }
        public required CategoryFilterType FilterType { get; init; }
        public required bool UsesHouseholdFinance { get; init; }
    }
}
