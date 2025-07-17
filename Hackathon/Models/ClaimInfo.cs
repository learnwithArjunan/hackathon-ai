namespace Hackathon.Models
{
    public class ClaimInfo
    {
        public string Name { get; set; }
        public string FileName { get; set; }

        public string ClaimType { get; set; }
        public List<string> ClaimData { get; set; }
        public string Status { get; set; }
    }
}
