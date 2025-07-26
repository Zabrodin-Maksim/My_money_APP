using System;

namespace My_money.Model
{
    public class Record 
    {
        public int Id { get; set; }
        public float Cost { get; set; }
        public int CategoryId { get; set; }
        public DateTime? DateTimeOccurred { get; set; }
        public string Description { get; set; }
    }
}
