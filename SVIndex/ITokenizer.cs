using System.Collections.Generic;

namespace SV_PLI
{
    public interface ITokenizer
    {
        IEnumerable<int> Tokenize(string input);
    }
}