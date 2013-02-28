using MongoDB.Driver;
using SVIndex.Crawlers;
using SVIndex.Persistence;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SVIndex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<JobPost> posts;
        private readonly MongoDatabase db;
        private IEnumerable<SVIndexInfo> svIndices;
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

        public static string[] GetMentions(IEnumerable<Mention> mentions, string text)
        {
            return mentions.Where(m => m.Regex.IsMatch(text)).Select(m => m.Technology).ToArray();
        }

        public MainWindow()
        {
            InitializeComponent();

            this.db = JobPostExtensions.OpenDatabase();
            this.posts = new ObservableCollection<JobPost>();

            this.LoadPostsAsync();
            this.posts.CollectionChanged += PostsCollectionChanged;
            this.listBox.ItemsSource = posts;
        }

        private void LoadPostsAsync()
        {
            Task.Factory.StartNew(() =>
                {
                    var jobPosts = this.db.GetPosts();
                    foreach (JobPost post in jobPosts)
                    {
                        post.Categories = GetMentions(mentions, post.Title + " " + post.Details);
                        this.Dispatcher.BeginInvoke(new Action(() => this.posts.Add(post)));
                    }
                });
            //var filteredJobPosts = jobPosts.Where(p => !_mentions.Any(m => m.Regex.IsMatch(p.Title + " " + p.Details)));
        }

        private void PostsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (posts.Count <= 0)
                return;

            int selectedIndex = posts.Count - 1;
            listBox.SelectedIndex = selectedIndex;

            StatusText.Content = String.Format("Total job posts: {0}, Failed {1}", posts.Count(), posts.Count(p => p.IsFailed));
        }

        private void RunCrawlerClick(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() => RunCrawler((Button)sender),
                TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning);
        }

        private void RunCrawler(Button sender)
        {
            Dispatcher.BeginInvoke(new Action(() => sender.IsEnabled = false));

            Dispatcher.BeginInvoke(new Action(() => posts.Clear()));
            this.db.DropDatabase();
            var crawler = new JobsBgCrawler();
            foreach (JobPost post in crawler.GetAllPosts())
            {
                post.Load();
                post.Categories = GetMentions(mentions, post.Title + " " + post.Details);
                JobPost post1 = post;
                Dispatcher.BeginInvoke(new Action(() => posts.Add(post1)));
                this.db.AddPost(post1);
                Application.Current.DoEvents();
            }
            Dispatcher.BeginInvoke(new Action(() => sender.IsEnabled = true));
        }

        private void ExportClick(object sender, RoutedEventArgs e)
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

        private void PreserveClick(object sender, RoutedEventArgs e)
        {
            var indexByMonth = new SVIndexByMonth
            {
                Id = GetCurrentId(),
                SVIndices = svIndices
            };
            db.GetCollection<SVIndexByMonth>("SVIndexByMonth").Save(indexByMonth);
        }

        private static string GetCurrentId()
        {
            return DateTime.Now.ToString("yyyy-M");
        }

        private static string GetPrevId()
        {
            return DateTime.Now.Subtract(TimeSpan.FromDays(30)).ToString("yyyy-M");
        }
    }
}
