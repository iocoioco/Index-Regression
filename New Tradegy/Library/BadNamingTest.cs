using System;

namespace NamingConventionTest
{
    public class OrderProcessor // ❌ Should be PascalCase: OrderProcessor
    {
        public int MyPublicField; //  Should be PascalCase: MyPublicField or (better) a property

        private int myfield; // ❌ Should be: _myField
        private int my;

        private const int max_size = 100; // ❌ Should be: MaxSize

        private static readonly int default_timeout = 30; // ❌ Should be: DefaultTimeout

        public string user_name { get; set; } // ❌ Should be: UserName

        public void calculate_total() // ❌ Should be: CalculateTotal
        {
            int Local_Variable = 5; // ❌ Should be: localVariable

            Process_data(Local_Variable); // ❌ Parameter should be camelCase
        }

        private void Process_data(int Input_Value) // ❌ Should be: inputValue
        {
            Console.WriteLine($"Value = {Input_Value}");
        }

        public interface streamReader // ❌ Should be: IStreamReader
        {
            void read();
        }

        public enum log_level // ❌ Should be: LogLevel
        {
            error, // ❌ Should be: Error
            warning // ❌ Should be: Warning
        }
    }
}
