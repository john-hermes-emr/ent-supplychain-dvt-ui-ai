namespace DVT.Api.Models
{
    public class StartUpLog
    {
        public List<string> LogEntries = new List<string>();
        public bool StartupSuccess { get; private set; } = true;
        public string EnvironmentName { get; set; } = string.Empty;

        public void Add(string logEntry)
        {
            LogEntries.Add(logEntry);
        }

        public void SetStatusSuccess(string message)
        {
            // If StartupSuccess is already false, it should remain false
            if (!StartupSuccess)
            {
                Add("Startup Failed");
                return;
            }
            else
            {
                StartupSuccess = true;
                Add(message);
            }
        }

        public void SetStatusFailed()
        {
            StartupSuccess = false;
        }   
    }
}
