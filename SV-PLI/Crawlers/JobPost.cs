using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace SV_PLI.Crawlers
{
    public class JobPost
    {
        private readonly string _url;

        public string Id { get; set; }
        public bool IsExternal { get; set; }
        public bool IsFailed { get; set; }
        public string RefNo { get; set; }
        public DateTime Date { get; set; }
        public string[] Categories { get; set; }
        public string JobType { get; set; }
        public string Level { get; set; }
        public string EmploymentType { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string Location { get; set; }
        public string Organisation { get; set; }
        public string Zaplata { get; set; }

        internal JobPost()
        {
        }

        public JobPost(string id, string date, string title, string company)
        {
            Id = id;
            Date = ExtractDate(date);
            Title = title;
            Organisation = company;
            if (id.ToLower().StartsWith("http://"))
            {
                IsExternal = true;
                _url = id;
            }
            else
                _url = String.Format("http://it.jobs.bg/{0}", id);
            
            //Debug.WriteLine("Detected article at {0}", _url);
        }

        public void Load()
        {
            var document = new HtmlDocument();
            var client = new WebClient();
            using (var stream = client.OpenRead(_url))
            {
                document.Load(stream, Encoding.UTF8);
            }

            var detailDescriptions =
                document.DocumentNode.SelectNodes(@"//td[@class='jobDataView']/../td[1]");

            var detailElements =
                document.DocumentNode.SelectNodes(@"//td[@class='jobDataView']/../td[2]");

            if (detailElements == null)
            {
                IsFailed = true;
                return;
            }

            var texts = detailElements.Select(e => e.InnerText.Trim()).ToArray();
            var descriptions = detailDescriptions.Select(e => e.InnerText.Trim()).ToArray();

            Debug.Assert(texts.Length == descriptions.Length);

            for (int i = 0; i < texts.Length; i++)
            {
                if (descriptions[i] == "Ref.No:")
                    RefNo = texts[i];
                else if (descriptions[i] == "Описание иИзисквания:")
                    Details = texts[i];
                else if (descriptions[i] == "Месторабота:")
                    Location = texts[i];
                else if (descriptions[i] == "Заплата:")
                    Zaplata = texts[i];
                else if (descriptions[i] == "Организация:")
                    ;
                    //Organisation = texts[i];
                else
                    Debug.Assert(false);
            }
        }

        private static DateTime ExtractDate(string date)
        {
            if (String.IsNullOrWhiteSpace(date))
                throw new ArgumentException("Invalid date", "date");

            if (date.ToLower().Trim() == "днес")
                return DateTime.Today;

            if (date.ToLower().Trim() == "вчера")
                return DateTime.Today.AddDays(-1);

            return DateTime.ParseExact(date, "dd.MM.yy", CultureInfo.GetCultureInfo("bg-bg"), DateTimeStyles.AssumeLocal);
        }
    }
}