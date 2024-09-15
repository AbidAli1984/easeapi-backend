namespace BOL.DTO
{
    public class APIConfigurationResponse
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public List<string> Fields { get; set; } = new List<string>();
    }
}