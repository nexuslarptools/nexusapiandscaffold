using System;
using System.Linq;
using System.Text;

namespace NEXUSDataLayerScaffold.Logic
{
    public class StringLogic
    { 
        public static string IgnorePunct(string input)
        {
            var sb = new StringBuilder();

            foreach (char c in input)
            {
                if (!char.IsPunctuation(c))
                    sb.Append(c);
            }

            return sb.ToString();
        }
        public static bool IsNumeric(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            return value.All(char.IsNumber);
        }

        public static bool CompareNumericsWithSlashes(string value, string comparevalue, string comparetype)
        {
            try
            {
                if (!IsNumeric(comparevalue.Trim()))
                {
                    return false;
                }

                int val2 = int.Parse(comparevalue.Trim());

                string newvalue = value.Replace('\\', '/');
                newvalue = newvalue.Replace('-', '/');
                var strarray = newvalue.Split('/');

                foreach (var substr in strarray)
                {
                    if (IsNumeric(substr.Trim()))
                    {
                        int val1 = int.Parse(substr.Trim());
                        switch (comparetype)
                        {
                            case "=":
                                if (val1 == val2)
                                {
                                    return true;
                                }
                                break;
                            case ">":
                                if (val1 > val2)
                                {
                                    return true;
                                }
                                break;
                            case ">=":
                                if (val1 >= val2)
                                {
                                    return true;
                                }
                                break;
                            case "<":
                                if (val1 < val2)
                                {
                                    return true;
                                }
                                break;
                            case "<=":
                                if (val1 <= val2)
                                {
                                    return true;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
