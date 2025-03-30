namespace New_Tradegy.Library
{
    

    internal class TestNaming
    {
        private int myfield;
        private int testField = 123;       // Should trigger RCS1110

        private void Test_method() { }

        private int MyWrongField = 123; // Should now show rename suggestion
    }

    internal class FieldRuleTest
    {
        private static int OrderCount = 0;

        public void DoSomething()
        {
            if (true)
                OrderCount = 1;
        }
    }

    internal class by
    {
        static int OrderCount = 0;
        public static string buys(g.stock_data o)
        {
            string t = "";

            t += buys_물타기금지(o);

            if (true)
                OrderCount = 1;

            return " ";
        }
        public static string buys_물타기금지(g.stock_data o)
        {
            string t = "";

            return t;
        }
        internal class TestNaming
        {
            private int MyWrongField = 123; // <- should get squiggled and suggestion
        }
    }

    
}
