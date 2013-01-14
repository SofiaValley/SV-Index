using System;
using System.Collections.Generic;

namespace SV_PLI
{
    public class SimpleTokenizer : ITokenizer
    {
        public IEnumerable<int> Tokenize(string input)
        {
            int i = 0;
            if (String.IsNullOrEmpty(input))
                yield break;

            bool lastNonLetterOrDigit = true;

            foreach (char c in input)
            {
                if (!Char.IsLetterOrDigit(c))
                    lastNonLetterOrDigit = true;
                else if (lastNonLetterOrDigit)
                {
                    lastNonLetterOrDigit = false;
                    yield return i;
                }
                i++;
            }
        }
    }
}