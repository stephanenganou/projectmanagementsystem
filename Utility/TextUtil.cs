using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMSystem.Utility
{
    public static class TextUtil
    {
        public static bool checkIfEmpty(string valueZuCheck)
        {
            return String.IsNullOrEmpty(valueZuCheck);
        }
    }
}