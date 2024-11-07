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
    }
}
