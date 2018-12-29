using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Util
{
    public static class StringUtil
    {
        public static string GenerateName(int len)
        {
            string[] c = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
            string[] v = { "a", "e", "i", "o", "u", "ae", "y" };
            string output = c[MathUtil.Random.Next(c.Length)].ToUpper() + v[MathUtil.Random.Next(v.Length)];
            int b = 2;
            while (b < len)
            {
                output += c[MathUtil.Random.Next(c.Length)];
                output += v[MathUtil.Random.Next(v.Length)];
                b+= 2;
            }
            return output;
        }
    }
}
