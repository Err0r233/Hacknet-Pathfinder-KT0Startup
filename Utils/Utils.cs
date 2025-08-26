using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace KT0Mods.Utils
{
    public class Utils
    {
        public static string B64Encode(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }
        
        public static float Ratio(float a1, float b1, float a2, float b2, float a3)
        {
            return b1 + ((a3 - a1) * (b1 - b2) / (a1 - a2));
        }
        
    }
}

