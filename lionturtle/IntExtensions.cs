namespace lionturtle
{
    public static class IntExtensions
    {
        public static int Mod(this int a, int b)
        {
            if (b == 0)
                throw new DivideByZeroException();

            return ((a % b) + b) % b;
        }
    }
}

