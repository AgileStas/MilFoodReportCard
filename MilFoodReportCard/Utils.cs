
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MilFoodReportCard
{
    public class Utils
    {
        public static string Num2Text(int num)
        {
            string[] ones = { "", "одне", "два", "три", "чотири", "п'ять", "шість", "сім", "вісім", "дев'ять" };
            string[] tens = { "десять", "одинадцять", "дванадцять", "тринадцять", "чотирнадцять", "п'ятнадцять", "шістнадцять", "сімнадцять", "вісімнадцять", "дев'ятнадцять" };
            string[] decs = { "", "", "двадцять", "тридцять", "сорок", "п'ятдесят", "шістдесят", "сімндесят", "вісімдесят", "дев'яносто" };

            if (num == 0)
            {
                return "нуль";
            }
            else if (num < 10)
            {
                return ones[num];
            }
            else if (num < 20)
            {
                return tens[num % 10];
            }
            else if (num < 100)
            {
                string ms = decs[num / 10];
                string ls = ones[num % 10];
                return ls == "" ? ms : ms + " " + ls;
            }
            else
            {
                throw new Exception("Not implemented");
            }
        }

        public static bool UpperName(string name, out string initials, out string upper)
        {
            var wbCParts = name.Split(' ');
            if (wbCParts.Length == 2)
            {
                if (wbCParts[0].EndsWith("."))
                {
                    initials = wbCParts[0][0] + " " + wbCParts[1];
                }
                else
                {
                    initials = wbCParts[0][0] + ". " + wbCParts[1];
                }
                upper = wbCParts[0] + " " + wbCParts[1].ToUpper();
            }
            else if (wbCParts.Length == 1)
            {
                initials = wbCParts[0];
                upper = wbCParts[0].ToUpper();
            }
            else
            {
                initials = name;
                upper = name.ToUpper();
                return false;
            }
            return true;
        }
    }
}
