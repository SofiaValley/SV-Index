using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MongoDB.Driver;
using SVIndex.Crawlers;
using SVIndex.Persistence;

namespace SVIndex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly ObservableCollection<JobPost> posts;

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

        private readonly MongoDatabase db;

        public static string[] GetMentions(IEnumerable<Mention> mentions, string text)
        {
            return mentions.Where(m => m.Regex.IsMatch(text)).Select(m => m.Technology).ToArray();
        }

        public MainWindow()
        {
            InitializeComponent();

            this.db = JobPostExtensions.OpenDatabase();

            posts = new ObservableCollection<JobPost>();

            this.LoadPostsAsync();
            posts.CollectionChanged += PostsCollectionChanged;
            listBox.ItemsSource = posts; //monthIndex;
        }

        private void LoadPostsAsync()
        {
            Task.Factory.StartNew(() =>
                {
                    var jobPosts = this.db.GetPosts()
                        /*.Where(
                            p =>
                            (p.Date >= new DateTime(2013, 1, 1)) && (p.Date < new DateTime(2013, 2, 1)))*/;
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

        private void JobsbgClick(object sender, RoutedEventArgs e)
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
            //DateTime prev = DateTime.Today.AddDays((DateTime.Today.Day - 1) * -1);
            foreach (JobPost post in crawler.GetAllPosts())
            {
                /*if (post.Date < prev)
                    break;*/
                post.Load();
                post.Categories = GetMentions(mentions, post.Title + " " + post.Details);
                JobPost post1 = post;
                Dispatcher.BeginInvoke(new Action(() => posts.Add(post1)));
                this.db.AddPost(post1);
                Application.Current.DoEvents();
            }
            Dispatcher.BeginInvoke(new Action(() => sender.IsEnabled = true));
        }

        private IEnumerable<SVIndexInfo> svIndices;

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
                Position = position++,
                Language = i.Key,
                Mentions = i.Value,
                Index = (double)i.Value / total
            }));

            using (var writer = new StreamWriter("out.csv"))
            {
                writer.WriteLine("\"Позиция\"\tЕзик за Програмиране\"\t\"Срещания\"\t\"SV Index\"");

                foreach (var i in svIndices)
                {
                    writer.WriteLine("{0}\t\"{1}\"\t{2}\t{3:P}", i.Position, i.Language, i.Mentions, i.Index);
                }
            }
            Process.Start("out.csv");
        }

        private void PreserveClick(object sender, RoutedEventArgs e)
        {
            var indexByMonth = new SVIndexByMonth
            {
                Month = DateTime.Now.Month,
                SVIndices = svIndices
            };
            db.GetCollection<SVIndexByMonth>("SVIndexByMonth").Save(indexByMonth);
        }
    }
}
