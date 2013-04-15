using System;
using System.IO;

namespace SVIndex.Extractor
{
    public interface IExtractor
    {
        string GetText(Uri uri);
        string GetText(string html);
        string GetText(TextReader reader);
        string GetText(Stream stream);
    }
}