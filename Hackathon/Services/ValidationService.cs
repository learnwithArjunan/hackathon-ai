using Hackathon.Models;
using System.Security.Claims;

namespace Hackathon.Services
{
    public class ValidationService
    {
        private readonly List<string> _approvedHospitals = new() { "Apollo", "Fortis", "AIIMS", "Sunrise Multispeciality" };
        private readonly List<string> _coveredDiagnoses = new() { "Bronchitis", "Malaria", "Dengue", "Minor Surgical Procedure","Laceration Repair" };
        private readonly List<string> _nonPayableKeywords = new() {"gloves", "gown", "cap", "mask", "drapes" };

        public ValidationResult Validate(ClaimData data)
        {
            var isDateValid = DateTime.TryParse(data.Date, out var date) && (DateTime.Now - date).TotalDays <= 90;
            var isHospitalValid = _approvedHospitals.Any(h => data.Hospital?.Contains(h, StringComparison.OrdinalIgnoreCase) == true);
            var isDiagnosisValid = _coveredDiagnoses.Any(d => data.Diagnosis?.Contains(d, StringComparison.OrdinalIgnoreCase) == true);

            var result =  new ValidationResult
            {
                IsDateValid = isDateValid,
                IsHospitalApproved = isHospitalValid,
                IsDiagnosisCovered = isDiagnosisValid
            };

            if (data.NonClaimableItems?.Count > 0)
            {
                foreach (var item in data.NonClaimableItems)
                {
                    if (_nonPayableKeywords.Any(k => item.Name.ToLower().Contains(k)))
                    {
                        result.NonPayableItems.Add(item.Name);
                        result.NonClaimableTotal += item.Amount;
                    }
                }

                result.ApprovedAmount = data.TotalAmount - result.NonClaimableTotal;
            }

            return result;  
        }
    }
}