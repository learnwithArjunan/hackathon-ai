using Newtonsoft.Json;

namespace Hackathon.Models
{
    public class ClaimData
    {
        public string Date { get; set; }
        [JsonProperty("Hospital Name")]
        public string Hospital { get; set; }
        public string Diagnosis { get; set; }
        public List<BillingItem> NonClaimableItems { get; set; }
        public decimal NonClaimableTotal { get; set; }
        public decimal ApprovedAmount { get; set; }
        public decimal TotalAmount => ApprovedAmount + NonClaimableTotal;
    }

    public class BillingItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }      // Previously called "Unit"
        public decimal UnitRate { get; set; }  // Previously called "Rate"
        public decimal Amount { get; set; }    // Now includes direct value + calculation
    }
}