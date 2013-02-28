using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace SVIndex.Crawlers
{
    public class JobsBgCrawler
    {
        private const string Url =
            "http://it.jobs.bg/front_job_search.php?frompage={0}&str_regions=&str_locations=&tab=jobs&old_country=&country=-1&region=0&l_category%5B%5D=0&keyword=#paging";

        public int Total { get; private set; }

        public IEnumerable<JobPost> GetAllPosts()
        {
            int current = 0;
            int total;
            int page = 0;

            var client = new WebClient();

            var results = GetPage(page, client, out total);
            Total = total;

            while (current < total)
            {
                foreach (JobPost post in results)
                {
                    yield return post;
                    current++;
                }
                page++;
                results = GetPage(page, client, out total);
            }

        }

        private static IEnumerable<JobPost> GetPage(int page, WebClient client, out int total)
        {
            Debug.WriteLine("Opening page {0}", page);

            var document = new HtmlDocument();
            
            using (var stream = client.OpenRead(String.Format(Url, (page * 20))))
            {
                document.Load(stream, Encoding.UTF8);
            }
            string totalText = document.DocumentNode.SelectSingleNode(@"//td[@class='pagingtotal']").InnerText;

            total = Int32.Parse(totalText.Substring(totalText.LastIndexOf(' ')));

            var retVal = new List<JobPost>();
            var rowNode = document.DocumentNode.SelectSingleNode(@"//td[@class='offerslistRow']/..");
            while (rowNode != null)
                {
                string date = rowNode.SelectSingleNode("./td[1]").InnerText;
                var pos = rowNode.SelectSingleNode("./td[3]/a");
                string href = pos.Attributes["href"].Value;
                string title = pos.InnerText;
                string company = rowNode.SelectSingleNode("./td[5]").InnerText.Trim();
                var post = new JobPost(href, date, title, company);

                retVal.Add(post);

                rowNode = GetNextNode(rowNode, "tr");
            }
            return retVal;
        }

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