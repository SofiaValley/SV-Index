/*


using System.Collections.Generic;
using System.Linq;
using opennlp.tools.tokenize;

namespace SV_PLI
{
    class OpenNlpTokenizer : ITokenizer
    {
        private opennlp.tools.tokenize.Tokenizer _internalTokenizer;

        public OpenNlpTokenizer()
        {
            _internalTokenizer = opennlp.tools.tokenize.SimpleTokenizer.INSTANCE;
        }

        public OpenNlpTokenizer(string modelFile)
        {
            LoadModel(modelFile);
        }

        public void LoadModel(string modelFile)
        {
            var modelInpStream = new java.io.FileInputStream(modelFile);
            var tokenizerModel = new TokenizerModel(modelInpStream);
            _internalTokenizer = new TokenizerME(tokenizerModel);
        }

        public IEnumerable<int> Tokenize(string text)
        {
            return _internalTokenizer.tokenizePos(text).Select(s => s.getStart());
        }
    }
}

*/