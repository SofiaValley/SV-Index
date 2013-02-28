using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVIndex
{
    public static class BTreeExtensions
    {
        public static IEnumerable<object> Match(this BTree tree, string input, ITokenizer tokenizer = null)
        {
            if (tokenizer == null)
                tokenizer = new SimpleTokenizer();
            //var watch = new Stopwatch();
            var results = new List<object>();
            var syncLock = new object();
            //watch.Start();
            var tasks =
                tokenizer.Tokenize(input).Select(ti => Task.Factory.StartNew(() => Seek(ti, input, tree, results, syncLock))).
                          ToArray();
            Task.WaitAll(tasks);
            //watch.Stop();
            return results;
        }

        private static void Seek(int i, string input, BTree tree, List<object> results, object syncLock)
        {
            int pos = i;
            var builder = new StringBuilder();
            var walker = new TreeWalker(tree);

            object lastGoodValue = null;
            while (pos < input.Length)
            {
                walker.GetNext(input[pos]);
                builder.Append(input[pos]);
                if (walker.LastValue != null)
                    lastGoodValue = walker.LastValue;
                if (!walker.HasMoreValues)
                    break;
                pos++;
            }

            if (lastGoodValue != null)
            {
                lock (syncLock)
                {
                    Console.WriteLine(builder.ToString());
                    results.Add(lastGoodValue);
                }
            }

        }
    }
}