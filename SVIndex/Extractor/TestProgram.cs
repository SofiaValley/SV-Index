using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SVIndex.Extractor;

namespace SVIndex.Extractor
{
    class TestProgram
    {
        private static void Test()
        {
            var warmUp = "".GetTextUsingDefaultHtmlExtractor();
            var client = new WebClient();
            string html = client.DownloadString("http://blog.databigbang.com/tag/boilerpipe/");
            Stopwatch watch = Stopwatch.StartNew();
            string text = BoilerPipe.KeepEverythingExtractor.GetText(html);
            watch.Stop();
            var firstTime = watch.Elapsed;
            watch = Stopwatch.StartNew();
            string text2 = html.GetTextUsingDefaultHtmlExtractor();
            watch.Stop();
            var secondTime = watch.Elapsed;
            watch = Stopwatch.StartNew();
            Task<string> task = html.GetTextUsingDefaultHtmlExtractorAsync();
            task.Wait();
            string string3 = task.Result;
            watch.Stop();
            var thirdTime = watch.Elapsed;
            Console.WriteLine(firstTime);
            Console.WriteLine(secondTime);
            Console.WriteLine(thirdTime);
        }
    }
}
