using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PracticeTool
{
    public static class I18N
    {
        public static string locale = "de";

        public static string Get(string s)
        {
            switch (locale)
            {
                case "de":
                    switch (s)
                    {
                        //case "
                        default: return s; 
                    }
               default: return s; 
            }
        }
    }
}
