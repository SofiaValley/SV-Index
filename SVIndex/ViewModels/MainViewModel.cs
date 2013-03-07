using MongoDB.Driver;
using SVIndex.Crawlers;
using SVIndex.Persistence;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SVIndex.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private const string DateFormat = "yyyy-M";
        private readonly ObservableCollection<JobPost> posts;
        private readonly MongoDatabase db;
        private IEnumerable<SVIndexInfo> svIndices;
        private string statusText;
        private bool isCrawlerRunning;
        private static readonly List<Mention> mentions = new List<Mention>
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

            this.RunCrawlerCommand = new DelegateCommand((_) => this.RunCrawlerAsync(), (_) => !isCrawlerRunning);
            this.ExportCommand = new DelegateCommand((_) => this.Export());
            this.PreserveCommand = new DelegateCommand((_) => this.Preserve());
            this.LoadPostsCommand = new DelegateCommand((_) => this.LoadPostsAsync());
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

        public IEnumerable<SVIndexInfo> SVIndices
        {
            get
            {
                return this.svIndices;
            }
            private set
            {
                if (this.svIndices != value)
                {
                    this.svIndices = value;
                    OnPropertyChanged("SVIndices");
                }
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

        public DelegateCommand LoadPostsCommand
        {
            get;
            private set;
        }

        private void LoadPostsAsync()
        {
            Task.Factory.StartNew(() => LoadPosts(), CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void LoadPosts()
        {
            var jobPosts = this.db.GetPosts();
            foreach (JobPost post in jobPosts)
            {
                post.Categories = GetCategories(post);
                this.AddPost(post);
            }
        }

        private void AddPost(JobPost post)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.posts.Add(post);
                    this.StatusText = String.Format("Total job posts: {0}, Failed {1}", posts.Count, posts.Count(p => p.IsFailed));
                }));
        }

        private void RunCrawlerAsync()
        {
            this.isCrawlerRunning = true;
            posts.Clear();
            Task.Factory.StartNew(() => RunCrawler(), CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Default).ContinueWith((_) => this.isCrawlerRunning = false);
        }

        private void RunCrawler()
        {
            this.db.DropDatabase();
            var crawler = new JobsBgCrawler();
            //crawler.PostCreated += (_, e) =>
            //    {
            //        var post = e.Post;
            //        post.Categories = GetCategories(post);
            //        JobPost post1 = post;
            //        this.db.AddPost(post1);
            //        this.AddPost(post1);
            //    };
            //crawler.GetAllPosts();

            foreach (JobPost post in crawler.GetAllPosts())
            {
                post.Categories = GetCategories(post);
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

            this.SVIndices = new List<SVIndexInfo>(output.Select(i => new SVIndexInfo
            {
                Id = string.Format("{0}-{1}", GetCurrentId(), i.Key),
                Position = position++,
                Language = i.Key,
                Mentions = i.Value,
                Index = (double)i.Value / total
            }));

            var prevIndices = db.GetCollection<SVIndexByMonth>("SVIndexByMonth").FindOne(new QueryDocument("_id", GetPrevId()));

            if (prevIndices != null)
            {
                foreach (var index in this.SVIndices)
                {
                    var prevIndex = prevIndices.SVIndices.FirstOrDefault(i => i.Language == index.Language);
                    if (prevIndex != null)
                    {
                        index.Delta = index.Index - prevIndex.Index;
                    }
                }
            }

            using (var writer = new StreamWriter("out.csv"))
            {
                writer.WriteLine("\"Позиция\"\tЕзик за Програмиране\"\t\"Срещания\"\t\"SV Index\"\t\"Изменение\"");

                foreach (var i in this.SVIndices)
                {
                    writer.WriteLine("{0}\t\"{1}\"\t{2}\t{3:P}\t{4:P}", i.Position, i.Language, i.Mentions, i.Index, i.Delta);
                }
            }
        }

        private void Preserve()
        {
            var indexByMonth = new SVIndexByMonth
            {
                Id = GetCurrentId(),
                SVIndices = this.SVIndices
            };
            db.GetCollection<SVIndexByMonth>("SVIndexByMonth").Save(indexByMonth);
        }

        private static string[] GetCategories(JobPost post)
        {
            var text = post.Title + " " + post.Details;
            return mentions.Where(m => m.Regex.IsMatch(text)).Select(m => m.Technology).ToArray();
        }

        private static string GetCurrentId()
        {
            return DateTime.Now.ToString(DateFormat);
        }

        private static string GetPrevId()
        {
            return DateTime.Now.Subtract(TimeSpan.FromDays(30)).ToString(DateFormat);
        }
    }
}
