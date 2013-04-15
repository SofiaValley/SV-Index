using System.Windows;
using System.Windows.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using opennlp.tools.tokenize;

namespace SVIndex.Crawlers
{
    public class WordCounter
    {
        public static Dictionary<string, int> CountWords(IEnumerable<string> articles)
        {
            List<String> terms = null;
            Uri uri = new Uri("/terms.txt", UriKind.Relative);
            StreamResourceInfo info = Application.GetResourceStream(uri);
            using (var reader = new StreamReader(info.Stream))
            {
                var termsFile = reader.ReadToEnd();
                terms = termsFile.Split().ToList();
            }

            var words = new Dictionary<string, int>();
            foreach (var article in articles)
            {
                var wordPattern = new Regex(@"\w+", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

                foreach (Match match in wordPattern.Matches(article))
                {
                    var word = match.Value.ToLower();
                    if (terms.Contains(word))
                    {
                        int currentCount = 0;
                        words.TryGetValue(word, out currentCount);

                        words[word] = ++currentCount;
                    }
                }
            }

            return words.Where(p => p.Value > 2).ToDictionary(p => p.Key, p => p.Value);
        }

        public static Dictionary<string, int> CountTokens(IEnumerable<string> articles)
        {
            Tokenizer t = new SimpleTokenizer();
            //TokenizerME m = new TokenizerME(new TokenizerModel("e 

            // List<String> terms = null;
            // Uri uri = new Uri("/terms.txt", UriKind.Relative);
            //StreamResourceInfo info = Application.GetResourceStream(uri);
            //using (var reader = new StreamReader(info.Stream))
            //{
            //    var termsFile = reader.ReadToEnd();
            //    terms = termsFile.Split().ToList();
            //}

            var words = new Dictionary<string, int>();
            foreach (var article in articles)
            {
                //var wordPattern = new Regex(@"\w+", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

                var tokens = t.tokenize(article);

                foreach (var token in tokens)
                {
                    int currentCount = 0;
                    words.TryGetValue(token, out currentCount);

                    words[token] = ++currentCount;
                }

                ////foreach (Match match in wordPattern.Matches(article))
                //{
                //    //var word = match.Value.ToLower();
                //    //if (terms.Contains(word))
                //    {
                //        int currentCount = 0;
                //        words.TryGetValue(word, out currentCount);

                //        words[word] = ++currentCount;
                //    }
                //}
            }

            return words.Where(p => p.Value > 2).ToDictionary(p => p.Key, p => p.Value);
        }
    }
}
