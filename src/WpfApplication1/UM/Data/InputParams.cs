namespace AssetBuilder.UM.Data
{
    public class InputParams
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public InputParams(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}