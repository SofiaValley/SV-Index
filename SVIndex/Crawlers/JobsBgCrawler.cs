using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using System.Web.Script.Serialization;
using System.IO;

namespace SVIndex.Crawlers
{
    public class JobsBgCrawler
    {
        private const int PageSize = 20;
        private const string Url =
            "http://it.jobs.bg/front_job_search.php?frompage={0}&str_regions=&str_locations=&tab=jobs&old_country=&country=-1&region=0&l_category%5B%5D=0&keyword=#paging";

        private const string BoilerpipeUrl = "http://boilerpipe-web.appspot.com/extract?url={0}&extractor=ArticleExtractor&output=json";

        public int Total { get; private set; }

        public IEnumerable<JobPost> GetAllPosts()
        {
            var pages = this.GetPageCount();
            for (int page = 0; page < pages; page++)
            {
                var results = GetPage(page);
                foreach (JobPost post in results)
                {
                    yield return post;
                }
            }
        }

        private static IEnumerable<JobPost> GetPage(int page)
        {
            Debug.WriteLine("Opening page {0}", page);

            var document = new HtmlDocument();
            var client = new WebClient();
            using (var stream = client.OpenRead(String.Format(Url, (page * PageSize))))
            {
                document.Load(stream, Encoding.UTF8);
            }

            var posts = new List<JobPost>();
            var rowNode = document.DocumentNode.SelectSingleNode(@"//td[@class='offerslistRow']/..");
            while (rowNode != null)
            {
                string date = rowNode.SelectSingleNode("./td[1]").InnerText;
                var pos = rowNode.SelectSingleNode("./td[3]/a");
                string href = pos.Attributes["href"].Value;
                string title = pos.InnerText;
                string company = rowNode.SelectSingleNode("./td[5]").InnerText.Trim();
                var post = CreatePost(href, title, date, company);
                posts.Add(post);

                rowNode = GetNextNode(rowNode, "tr");
            }
            return posts;
        }

        private int GetPageCount()
        {
            var document = new HtmlDocument();

            var client = new WebClient();
            using (var stream = client.OpenRead(String.Format(Url, 0)))
            {
                document.Load(stream, Encoding.UTF8);
            }

            string totalText = document.DocumentNode.SelectSingleNode(@"//td[@class='pagingtotal']").InnerText;

            this.Total = Int32.Parse(totalText.Substring(totalText.LastIndexOf(' ')));

            return this.Total / PageSize;
        }

        private static JobPost CreatePost(string id, string date, string title, string company)
        {
            var post = new JobPost(id, date, title, company);

            var articleUrl = string.Format(BoilerpipeUrl, post.Url);
            var client = new WebClient();
            using (var articleStream = client.OpenRead(articleUrl))
            {
                using (var reader = new StreamReader(articleStream))
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    var article = serializer.DeserializeObject(reader.ReadToEnd()) as Dictionary<string, object>;
                    var response = (Dictionary<string, object>)article["response"];
                    title = response["title"].ToString();
                    var content = response["content"].ToString();
                    post.Details = content;
                }
            }

            return post;
        }

        //private static JobPost CreateNReadabilityPost(string id, string date, string title, string company)
        //{
        //    var post = new JobPost(id, date, title, company);
        //    var transcoder = new NReadability.NReadabilityWebTranscoder();
        //    bool success;

        //    var settings = new NReadability.DomSerializationParams() { DontIncludeDocTypeMetaElement = true, DontIncludeContentTypeMetaElement = true, DontIncludeGeneratorMetaElement = true, DontIncludeMobileSpecificMetaElements = true };

        //    var details =
        //      transcoder.Transcode(post.Url, settings, out success);

        //    if (success)
        //    {
        //        post.Details = details;
        //    }

        //    return post;
        //}

        //private static JobPost CreateParserPost(string id, string date, string title, string company)
        //{
        //    var post = new JobPost(id, date, title, company);

        //    var articleUrl = string.Format("https://readability.com/api/content/v1/parser?url={0}&token=8bd4b5dccf76253c6b9de2bf0ef88412e62f1dc2", post.Url);
        //    var client = new WebClient();
        //    using (var articleStream = client.OpenRead(articleUrl))
        //    {
        //        using (var reader = new StreamReader(articleStream))
        //        {
        //            JavaScriptSerializer serializer = new JavaScriptSerializer();
        //            var article = serializer.DeserializeObject(reader.ReadToEnd()) as Dictionary<string, object>;
        //            var response = (Dictionary<string, object>)article["response"];
        //            title = response["title"].ToString();
        //            var content = response["content"].ToString();
        //            post.Details = content;
        //        }
        //    }

        //    return post;
        //}

        private static HtmlNode GetNextNode(HtmlNode node, string nodeName)
        {
            nodeName = nodeName.ToLower();

            if (node == null)
                return null;

            HtmlNode nextSibling = node.NextSibling;
            if (nextSibling != null && nextSibling.Name.ToLower() == nodeName)
                return nextSibling;
            return GetNextNode(nextSibling, nodeName);
        }
    }
}