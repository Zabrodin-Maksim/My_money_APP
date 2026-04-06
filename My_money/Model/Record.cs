using System;

namespace My_money.Model
{
    public class Record
    {
        public int Id { get; set; }
        public required decimal Amount { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public DateTime? DateTimeOccurred { get; set; }
        public string? Description { get; set; }
        public int HouseholdId { get; set; }
        public int? OwnerUserId { get; set; }
        public int CreatedByUserId { get; set; }
        public required string Scope { get; set; }
        public required string Type { get; set; }
        public string? IncomeTarget { get; set; }

        public Record() { }
        public Record(decimal amount, int categoryId, DateTime? occurredAt, string? description, string scope, string type) 
        {
            Amount = amount;
            CategoryId = categoryId;
            DateTimeOccurred = occurredAt;
            Description = description;
            Scope = scope;
            Type = type;
        }
    }
}
