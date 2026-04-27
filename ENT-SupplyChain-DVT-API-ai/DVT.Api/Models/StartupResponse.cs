namespace DVT.Api.Models
{
    public class StartupResponse
    {
        public List<string> StartupLog { get; set; } = new List<string>();
        public bool StartupSuccess { get; set; } = false;
    }
}