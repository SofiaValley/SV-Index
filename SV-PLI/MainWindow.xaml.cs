using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SV_PLI.Crawlers;
using SV_PLI.Persistence;

namespace SV_PLI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly ObservableCollection<JobPost> _posts;

        private readonly List<Mention> _mentions = new List<Mention>
                                              {
                                                  new Mention(@"\bjava\b", "Java"),
                                                  new Mention(@"\bc\#", "C#"),
                                                  new Mention(@"\bvb\b", "VisualBasic"),
                                                  new Mention(@"\bvisual\s*basic\b", "VisualBasic"),
                                                  new Mention(@"\bc\s*\+\+", "C++"),
                                                  new Mention(@"\s+c\b", "C"),
                                                  new Mention(@"\bphp\b", "PHP"),
                                                  new Mention(@"\bpython\b", "Python"),
                                                  new Mention(@"\bruby\b", "Ruby"),
                                                  new Mention(@"\bobjective[\s-]*c\b", "ObjectiveC"),
                                                  new Mention(@"\bjava\s*script\b", "JavaScript"),
                                                  new Mention(@"\bdelphi\b", "Delphi"),
                                              };
        /*private List<string> _languages = new List<string>
        {
            "java",
            "c#",
            "c++",
            "php",
            "python",
            "ruby",
            "objective-c",
            "javascript"
        };*/

        public static string[] GetMentions(IEnumerable<Mention> mentions, string text)
        {
            return mentions.Where(m => m.Regex.IsMatch(text)).Select(m => m.Technology).ToArray();
        }

        public MainWindow()
        {
            InitializeComponent();

            var jobPosts = JobPostExtensions.Read("posts.db3").Where(p => (p.Date >= new DateTime(2013, 1, 1)) && (p.Date < new DateTime(2013, 2, 1)));
            foreach (JobPost post in jobPosts)
            {
                post.Categories = GetMentions(_mentions, post.Title + " " + post.Details);
            }
            //var filteredJobPosts = jobPosts.Where(p => !_mentions.Any(m => m.Regex.IsMatch(p.Title + " " + p.Details)));
            _posts = new ObservableCollection<JobPost>(jobPosts);
            _posts.CollectionChanged += PostsCollectionChanged;
            listBox.ItemsSource = _posts; //monthIndex;
        }

        void PostsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_posts.Count <= 0)
                return;

            int selectedIndex = _posts.Count - 1;
            listBox.SelectedIndex = selectedIndex;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {

        }

        private void JobsbgClick(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() => RunCrawler((Button)sender),
                TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning);
        }

        private void RunCrawler(Button sender)
        {
            Dispatcher.BeginInvoke(new Action(() => sender.IsEnabled = false));

            Dispatcher.BeginInvoke(new Action(() => _posts.Clear()));
            var connection = JobPostExtensions.OpenNewDatabase("posts.db3");
            var crawler = new JobsBgCrawler();
            //DateTime prev = DateTime.Today.AddDays((DateTime.Today.Day - 1) * -1);
            foreach (JobPost post in crawler.GetAllPosts())
            {
                /*if (post.Date < prev)
                    break;*/
                post.Load();
                JobPost post1 = post;
                Dispatcher.BeginInvoke(new Action(() => _posts.Add(post1)));
                post1.Write(connection);
                Application.Current.DoEvents();
            }
            connection.Close();
            Dispatcher.BeginInvoke(new Action(() => sender.IsEnabled = true));
        }

        private void ExportClick(object sender, RoutedEventArgs e)
        {
            var boo =
                _posts.SelectMany(p => p.Categories)
                      .GroupBy(s => s)
                      .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                      .OrderByDescending(k=>k.Value)
                      .ToArray();
            using (var writer = new StreamWriter("out.csv"))
            {
                writer.WriteLine("\"tech\"\t\"count\"");
                foreach (var keyValuePair in boo)
                {
                    writer.WriteLine("\"{0}\"\t{1}", keyValuePair.Key, keyValuePair.Value);
                }
            }
        }
    }

    public class Mention
    {
        public Mention()
        {
        }

        public Mention(string regex, string technology)
        {
            Technology = technology;
            Regex =
                new Regex(regex,
                          RegexOptions.IgnoreCase |
                          RegexOptions.Compiled);
        }

        public Regex Regex { get; set; }
        public string Technology { get; set; }
    }
}
