using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace SVIndex.Crawlers
{
    public class JobPost
    {
        public string Url { get; set; }
        public string Id { get; set; }
        public bool IsExternal { get; set; }
        public bool IsFailed { get; set; }
        public string RefNo { get; set; }
        public DateTime Date { get; set; }
        public string[] Categories { get; set; }
        public string[] Tags { get; set; }
        public string JobType { get; set; }
        public string Level { get; set; }
        public string EmploymentType { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string Location { get; set; }
        public string Organisation { get; set; }
        public string Salary { get; set; }

        internal JobPost()
        {
        }

        public JobPost(string id, string title, string date, string company)
        {
            this.Id = id;
            this.Title = title;
            this.Date = ExtractDate(date);
            this.Organisation = company;

            if (id.ToLower().StartsWith("http://"))
            {
                IsExternal = true;
                this.Url = id;
            }
            else
                this.Url = String.Format("http://it.jobs.bg/{0}", id);
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