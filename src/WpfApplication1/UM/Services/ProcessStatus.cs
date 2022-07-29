namespace AssetBuilder.UM.Services
{
    public class ProcessStatus
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ReturnKeyValue { get; set; }

        public ProcessStatusValue Value { get; set; }
    }
}