namespace Hackathon.Models
{
    public class ValidationResult
    {
        public bool IsDateValid { get; set; }
        public bool IsHospitalApproved { get; set; }
        public bool IsDiagnosisCovered { get; set; }
        public bool IsEligible => IsDateValid && IsHospitalApproved && IsDiagnosisCovered;

        public List<string> NonPayableItems { get; set; } = new();
        public decimal NonClaimableTotal { get; set; }
        public decimal ApprovedAmount { get; set; }
    }
}