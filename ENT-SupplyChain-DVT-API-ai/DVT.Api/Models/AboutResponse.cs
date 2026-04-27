namespace DVT.Api.Models
{
    public class AboutResponse
    {
        public string AboutTime { get; set; }
        public string AboutMessage { get; set; }

        public override string ToString()
        {
            return AboutTime + " " + AboutMessage;
        }
    }
}
