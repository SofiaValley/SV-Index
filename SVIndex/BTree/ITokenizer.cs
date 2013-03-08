using System.Collections.Generic;

namespace SVIndex.BTree
{
    public interface ITokenizer
    {
        IEnumerable<int> Tokenize(string input);
    }
}