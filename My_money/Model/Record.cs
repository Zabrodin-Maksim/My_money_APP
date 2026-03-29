using System;

namespace My_money.Model
{
    public class Record
    {
        public int Id { get; set; }
        public decimal Cost { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public DateTime? DateTimeOccurred { get; set; }
        public string? Description { get; set; }

        public Record() { }
        public Record(decimal cost, int categoryId, DateTime? occurredAt, string? description) 
        {
            Cost = cost;
            CategoryId = categoryId;
            DateTimeOccurred = occurredAt;
            Description = description;
        }
    }
}
