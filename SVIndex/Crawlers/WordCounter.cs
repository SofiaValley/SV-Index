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
        private static List<string> terms;

        public static List<string> Terms
        {
            get
            {
                if (terms == null)
                {
                    terms = GetTerms();
                }
                return terms;
            }
        }

        private static List<string> GetTerms()
        {
            List<String> terms = null;
            Uri uri = new Uri("/terms.txt", UriKind.Relative);
            StreamResourceInfo info = Application.GetResourceStream(uri);
            using (var reader = new StreamReader(info.Stream))
            {
                var termsFile = reader.ReadToEnd();
                terms = termsFile.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            return terms;
        }

        public static IEnumerable<string> GetTags(string article)
        {
            //Tokenizer t = new SimpleTokenizer();
            article = article.ToLower();
            foreach (var term in Terms)
            {
                //if (article.Contains(term))
                //{
                //    yield return term;
                //}
                //else
                {
                    //var words = t.tokenize(article);
                    var wordPattern = new Regex(@"[\w\.]+", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
                    var words = wordPattern.Matches(article).OfType<Match>().Select(m => m.Value);
                    foreach (var word in words)
                    {
                        if (Similarity.GetSimilarity(term, word) > .8)
                        {
                            yield return term;
                            break;
                        }
                        // TODO: Phrases?
                    }
                }
            }
        }

        public static Dictionary<string, int> CountWords(IEnumerable<string> articles)
        {
            List<String> terms = null;
            Uri uri = new Uri("/terms.txt", UriKind.Relative);
            StreamResourceInfo info = Application.GetResourceStream(uri);
            using (var reader = new StreamReader(info.Stream))
            {
                var termsFile = reader.ReadToEnd();
                terms = termsFile.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            var words = new Dictionary<string, int>();
            foreach (var term in terms)
            {
                words[term] = 0;
            }
            foreach (var article in articles)
            {
                //var wordPattern = new Regex(@"[^\s]+", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
                foreach (var term in terms)
                {
                    foreach (var word in article.Split().Select(i => i.ToLower()))
                    {
                        if (term == word)
                        {
                            int currentCount;
                            words.TryGetValue(term, out currentCount);
                            words[term] = ++currentCount;
                            break;
                        }
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

    public static class Similarity
    {
        public static int GetLevensteinDistance(this string firstString, string secondString)
        {
            if (firstString == null)
                throw new ArgumentNullException("firstString");
            if (secondString == null)
                throw new ArgumentNullException("secondString");
            if (firstString == secondString)
                return 0;

            int[,] matrix = new int[firstString.Length + 1, secondString.Length + 1];

            for (int i = 0; i <= firstString.Length; i++)
                matrix[i, 0] = i; // deletion
            for (int j = 0; j <= secondString.Length; j++)
                matrix[0, j] = j; // insertion

            for (int i = 0; i < firstString.Length; i++)
                for (int j = 0; j < secondString.Length; j++)
                    if (firstString[i] == secondString[j])
                        matrix[i + 1, j + 1] = matrix[i, j];
                    else
                    {
                        matrix[i + 1, j + 1] = Math.Min(matrix[i, j + 1] + 1, matrix[i + 1, j] + 1); //deletion or insertion
                        matrix[i + 1, j + 1] = Math.Min(matrix[i + 1, j + 1], matrix[i, j] + 1); //substitution
                    }
            return matrix[firstString.Length, secondString.Length];
        }

        public static double GetSimilarity(this string firstString, string secondString)
        {
            if (firstString == null)
                throw new ArgumentNullException("firstString");
            if (secondString == null)
                throw new ArgumentNullException("secondString");

            if (firstString == secondString)
                return 1;

            int longestLenght = Math.Max(firstString.Length, secondString.Length);
            int distance = GetLevensteinDistance(firstString, secondString);
            double percent = distance / (double)longestLenght;
            return 1 - percent;
        }


        public static string CleanUp(this string str)
        {
            return str.Trim().Trim('"').Trim('\'').Trim('"').Replace(@"&quot;", "").Replace("\n", " ");
        }
    }
}
