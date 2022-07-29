using System;

namespace AssetBuilder.UM.Services
{
    public class ProcessStatusValue
    {
        private int _intValue;

        public int IntValue
        {
            get => _intValue;
            set
            {
                try
                {
                    _intValue = Convert.ToInt32(value);
                }
                catch(Exception)
                {
                    _intValue = 0;
                }
            }
        }
    }
}