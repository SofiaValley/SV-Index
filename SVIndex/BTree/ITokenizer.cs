using System.Collections.Generic;

namespace SVIndex
{
    public interface ITokenizer
    {
        IEnumerable<int> Tokenize(string input);
    }
}