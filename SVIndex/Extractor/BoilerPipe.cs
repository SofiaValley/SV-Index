using System;
using System.IO;
using System.Threading.Tasks;
using de.l3s.boilerpipe.extractors;
using java.net;

namespace SVIndex.Extractor
{
    public static class BoilerPipe
    {
        private class ExtractorBaseWrapper : IExtractor
        {
            protected ExtractorBase Extractor;

            protected ExtractorBaseWrapper(ExtractorBase extractor)
            {
                Extractor = extractor;
            }

            public string GetText(Uri uri)
            {
                return Extractor.getText(new URL(uri.AbsoluteUri));
            }

            public string GetText(string html)
            {
                return Extractor.getText(html);
            }

            public string GetText(TextReader reader)
            {
                return Extractor.getText(reader.ReadToEnd());
            }

            public string GetText(Stream stream)
            {
                using (var reader = new StreamReader(stream))
                {
                    return Extractor.getText(reader.ReadToEnd());
                }
            }
        }

        private class ArticleExtractorWrapper : ExtractorBaseWrapper
        {
            public ArticleExtractorWrapper()
                : base(de.l3s.boilerpipe.extractors.ArticleExtractor.INSTANCE)
            { }

            private ArticleExtractorWrapper(ExtractorBase extractorInstance)
                : base(extractorInstance)
            { }

            public static ArticleExtractorWrapper GetNewWrapper()
            {
                return new ArticleExtractorWrapper(new ArticleExtractor());
            }
        }

        private class ArticleSentencesExtractorWrapper : ExtractorBaseWrapper
        {
            public ArticleSentencesExtractorWrapper()
                : base(de.l3s.boilerpipe.extractors.ArticleSentencesExtractor.INSTANCE)
            { }
        }

        private class CanolaExtractorWrapper : ExtractorBaseWrapper
        {
            public CanolaExtractorWrapper()
                : base(de.l3s.boilerpipe.extractors.CanolaExtractor.INSTANCE)
            { }
        }

        private class DefaultExtractorWrapper : ExtractorBaseWrapper
        {
            public DefaultExtractorWrapper()
                : base(de.l3s.boilerpipe.extractors.DefaultExtractor.INSTANCE)
            { }

            private DefaultExtractorWrapper(ExtractorBase extractorInstance)
                : base(extractorInstance)
            { }

            public static DefaultExtractorWrapper GetNewWrapper()
            {
                return new DefaultExtractorWrapper(new DefaultExtractor());
            }
        }

        private class KeepEverythingExtractorWrapper : ExtractorBaseWrapper
        {
            public KeepEverythingExtractorWrapper()
                : base(de.l3s.boilerpipe.extractors.KeepEverythingExtractor.INSTANCE)
            { }
        }

        private class KeepEverythingWithMinKWordsExtractorWrapper : ExtractorBaseWrapper
        {
            private int _kmin = 1;

            public KeepEverythingWithMinKWordsExtractorWrapper()
                : base(new KeepEverythingWithMinKWordsExtractor(1))
            { }

            public int KMin
            {
                get { return _kmin; }
                set
                {
                    if (value == _kmin) return;
                    _kmin = value;
                    Extractor = new KeepEverythingWithMinKWordsExtractor(_kmin);
                }
            }
        }

        private class LargestContentExtractorWrapper : ExtractorBaseWrapper
        {
            public LargestContentExtractorWrapper()
                : base(de.l3s.boilerpipe.extractors.LargestContentExtractor.INSTANCE)
            { }
        }

        private class NumWordsRulesExtractorWrapper : ExtractorBaseWrapper
        {
            public NumWordsRulesExtractorWrapper()
                : base(de.l3s.boilerpipe.extractors.NumWordsRulesExtractor.INSTANCE)
            { }
        }

        public static IExtractor ArticleExtractor { get; private set; }
        public static IExtractor ArticleSentencesExtractor { get; private set; }
        public static IExtractor CanolaExtractor { get; private set; }
        public static IExtractor DefaultExtractor { get; private set; }
        public static IExtractor KeepEverythingExtractor { get; private set; }
        public static IExtractor KeepEverythingWithMinKWordsExtractor { get; private set; }
        public static IExtractor LargestContentExtractor { get; private set; }
        public static IExtractor NumWordsRulesExtractor { get; private set; }

        static BoilerPipe()
        {
            ArticleExtractor = new ArticleExtractorWrapper();
            ArticleSentencesExtractor = new ArticleSentencesExtractorWrapper();
            CanolaExtractor = new CanolaExtractorWrapper();
            DefaultExtractor = new DefaultExtractorWrapper();
            KeepEverythingExtractor = new KeepEverythingExtractorWrapper();
            KeepEverythingWithMinKWordsExtractor = new KeepEverythingWithMinKWordsExtractorWrapper();
            LargestContentExtractor = new LargestContentExtractorWrapper();
            NumWordsRulesExtractor = new NumWordsRulesExtractorWrapper();
        }

        public static string GetTextUsingDefaultHtmlExtractor(this Uri uri)
        {
            return DefaultExtractor.GetText(uri);
        }

        public static string GetTextUsingDefaultHtmlExtractor(this string html)
        {
            return DefaultExtractor.GetText(html);
        }

        public static async Task<string> GetTextUsingDefaultHtmlExtractorAsync(this Uri uri)
        {
            Task<string> t = Task<string>.Factory.StartNew(() => DefaultExtractorWrapper.GetNewWrapper().GetText(uri));
            string result = await t;
            return result;
        }

        public static async Task<string> GetTextUsingDefaultHtmlExtractorAsync(this string html)
        {
            Task<string> t = Task<string>.Factory.StartNew(() => DefaultExtractorWrapper.GetNewWrapper().GetText(html));
            string result = await t;
            return result;
        }

        public static string GetTextUsingDefaultHtmlExtractor(this TextReader reader)
        {
            return DefaultExtractor.GetText(reader);
        }

        public static string GetTextUsingDefaultHtmlExtractor(this Stream stream)
        {
            return DefaultExtractor.GetText(stream);
        }
    }
}