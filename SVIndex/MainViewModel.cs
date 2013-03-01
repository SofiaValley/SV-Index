using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using SVIndex.Crawlers;
using SVIndex.Persistence;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;

namespace SVIndex
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<JobPost> posts;
        private readonly MongoDatabase db;
        private IEnumerable<SVIndexInfo> svIndices;
        private string statusText;
        private bool isCrawlerRunning;
        private readonly List<Mention> mentions = new List<Mention>
                                              {
                                                  new Mention(@"\bjava\b(?!\s*script)", "Java"),
                                                  new Mention(@"\bc\#", "C#"),
                                                  new Mention(@"\bvb\b", "VisualBasic"),
                                                  new Mention(@"\bvisual\s*basic\b", "VisualBasic"),
                                                  new Mention(@"\bc\s*\+\+", "C++"),
                                                  new Mention(@"(?!\bobjective)\bc(?![#\+])\b", "C"),
                                                  new Mention(@"\bphp\b", "PHP"),
                                                  new Mention(@"\bpython\b", "Python"),
                                                  new Mention(@"\bruby\b", "Ruby"),
                                                  new Mention(@"\bobjective[\s-]*c\b", "ObjectiveC"),
                                                  new Mention(@"\bjava\s*script\b", "JavaScript"),
                                                  new Mention(@"\bdelphi\b", "Delphi"),
                                              };

        public MainViewModel()
        {
            this.db = JobPostExtensions.OpenDatabase();
            this.posts = new ObservableCollection<JobPost>();

            this.LoadPostsAsync();

            this.RunCrawlerCommand = new DelegateCommand((_) => this.RunCrawler(), (_) => !isCrawlerRunning);
            this.ExportCommand = new DelegateCommand((_) => this.Export());
            this.PreserveCommand = new DelegateCommand((_) => this.Preserve());
        }

        public string StatusText
        {
            get
            {
                return this.statusText;
            }
            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;
                    OnPropertyChanged("StatusText");
                }
            }
        }

        public ObservableCollection<JobPost> Posts
        {
            get
            {
                return this.posts;
            }
        }

        public DelegateCommand RunCrawlerCommand
        {
            get;
            private set;
        }

        public DelegateCommand ExportCommand
        {
            get;
            private set;
        }

        public DelegateCommand PreserveCommand
        {
            get;
            private set;
        }

        private void LoadPostsAsync()
        {
            Task.Factory.StartNew(() =>
            {
                var jobPosts = this.db.GetPosts();
                foreach (JobPost post in jobPosts)
                {
                    post.Categories = GetMentions(mentions, post.Title + " " + post.Details);
                    AddPost(post);
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void AddPost(JobPost post)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.posts.Add(post);
                    this.StatusText = String.Format("Total job posts: {0}, Failed {1}", posts.Count, posts.Count(p => p.IsFailed));
                }));
        }

        private void RunCrawler()
        {
            this.isCrawlerRunning = true;
            posts.Clear();
            Task.Factory.StartNew(() => RunCrawlerAsync(), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).ContinueWith((_) => this.isCrawlerRunning = false);
        }

        private void RunCrawlerAsync()
        {
            this.db.DropDatabase();
            var crawler = new JobsBgCrawler();
            foreach (JobPost post in crawler.GetAllPosts())
            {
                post.Load();
                post.Categories = GetMentions(mentions, post.Title + " " + post.Details);
                JobPost post1 = post;
                this.db.AddPost(post1);
                this.AddPost(post1);
            }
        }

        private void Export()
        {
            var output =
                posts.SelectMany(p => p.Categories)
                      .GroupBy(s => s)
                      .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                      .OrderByDescending(k => k.Value)
                      .ToArray();

            var total = output.Sum(i => i.Value);
            int position = 1;

            svIndices = new List<SVIndexInfo>(output.Select(i => new SVIndexInfo
            {
                Id = string.Format("{0}-{1}", GetCurrentId(), i.Key),
                Position = position++,
                Language = i.Key,
                Mentions = i.Value,
                Index = (double)i.Value / total
            }));

            var prevIndicis = db.GetCollection<SVIndexByMonth>("SVIndexByMonth").FindOne(new QueryDocument("_id", GetPrevId()));

            if (prevIndicis != null)
            {
                foreach (var index in svIndices)
                {
                    var prevIndex = prevIndicis.SVIndices.FirstOrDefault(i => i.Language == index.Language);
                    if (prevIndex != null)
                    {
                        index.Delta = index.Index - prevIndex.Index;
                    }
                }
            }

            using (var writer = new StreamWriter("out.csv"))
            {
                writer.WriteLine("\"Позиция\"\tЕзик за Програмиране\"\t\"Срещания\"\t\"SV Index\"\t\"Изменение\"");

                foreach (var i in svIndices)
                {
                    writer.WriteLine("{0}\t\"{1}\"\t{2}\t{3:P}\t{4:P}", i.Position, i.Language, i.Mentions, i.Index, i.Delta);
                }
            }
            Process.Start("out.csv");
        }

        private void Preserve()
        {
            var indexByMonth = new SVIndexByMonth
            {
                Id = GetCurrentId(),
                SVIndices = svIndices
            };
            db.GetCollection<SVIndexByMonth>("SVIndexByMonth").Save(indexByMonth);
        }

        private static string[] GetMentions(IEnumerable<Mention> mentions, string text)
        {
            return mentions.Where(m => m.Regex.IsMatch(text)).Select(m => m.Technology).ToArray();
        }

        private static string GetCurrentId()
        {
            return DateTime.Now.ToString("yyyy-M");
        }

        private static string GetPrevId()
        {
            return DateTime.Now.Subtract(TimeSpan.FromDays(30)).ToString("yyyy-M");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
