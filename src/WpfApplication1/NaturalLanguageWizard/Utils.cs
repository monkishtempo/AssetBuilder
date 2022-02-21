using System;

namespace NaturalLanguageWizard
{
    public static class Enums<T> where T : struct
    {
        static Enums()
        {
            if (!typeof(T).IsEnum)
            {
                var message = string.Format("Non-enum type {0} used with Enums<T>", typeof(T).FullName);  
              
                throw new ArgumentException(message);
            }
        }

        public static T[] Values
        { 
            get 
            { 
                return Enum.GetValues(typeof(T)) as T[]; 
            } 
        }

        public static T Parse(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        public static T Parse(string value, bool ignoreCase)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }
    }
}
