using System;

namespace Core.Utils
{
    public static class BitUtils
    {
        public static void SetBit(ref uint target, uint mask, bool value)
        {
            if (value)
            {
                target |= mask;
            }
            else
            {
                target &= ~mask;
            }
        }

        public static string ToBinaryString<T>(uint value)
        {
            return Convert.ToString(value, 2);
        }
    }
}
