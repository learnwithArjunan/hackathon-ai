using Hackathon.Models;

namespace Hackathon.Services
{
    public class ValidationService
    {
        private readonly List<string> _approvedHospitals = new() { "Apollo", "Fortis", "AIIMS" };
        private readonly List<string> _coveredDiagnoses = new() { "Bronchitis", "Malaria", "Dengue" };

        public ValidationResult Validate(ClaimData data)
        {
            var isDateValid = DateTime.TryParse(data.Date, out var date) && (DateTime.Now - date).TotalDays <= 90;
            var isHospitalValid = _approvedHospitals.Any(h => data.Hospital?.Contains(h, StringComparison.OrdinalIgnoreCase) == true);
            var isDiagnosisValid = _coveredDiagnoses.Any(d => data.Diagnosis?.Contains(d, StringComparison.OrdinalIgnoreCase) == true);

            return new ValidationResult
            {
                IsDateValid = isDateValid,
                IsHospitalApproved = isHospitalValid,
                IsDiagnosisCovered = isDiagnosisValid
            };
        }
    }
}