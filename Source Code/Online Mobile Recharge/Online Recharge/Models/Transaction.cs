namespace MobileRecharge.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string UserId { get; set; } // Yeh zaroori hai
    }
}