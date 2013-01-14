using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SV_PLI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> languages = new List<string>
        {
            "Java",
            "C#",
            "C++",
            "Php",
            "Python",
            "Ruby",
            "Objective-C",
            "JavaScript"
        };

        private ObservableCollection<Index> monthIndex = new ObservableCollection<Index>();

        public MainWindow()
        {
            InitializeComponent();

            this.listBox.ItemsSource = monthIndex;            
        }

        private void JobsbgClick(object sender, RoutedEventArgs e)
        {
            foreach (var language in languages)
            {
                Task.Run(() =>
                    {
                        WebClient client = new WebClient();
                        using (var stream = client.OpenRead(GetUrl(language)))
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                var site = reader.ReadToEnd();

                                var index = new Index
                                {
                                    Language = language,
                                    Mentions = GetCount(site)
                                };

                                this.Dispatcher.BeginInvoke(new Action(() => this.monthIndex.Add(index)));
                            }
                        }
                    });
            }
        }

        private void ExportClick(object sender, RoutedEventArgs e)
        {
            Export(this.monthIndex);
        }

        internal static void Export(IEnumerable<Index> mentions)
        {
            var allMentions = mentions.Sum(i => i.Mentions);

            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                using (var file = dialog.OpenFile())
                {
                    using (StreamWriter write = new StreamWriter(file))
                    {
                        write.WriteLine("Позиция, Език за програмиране, Срещания в обяви, % от всички ");
                        int position = 1;
                        foreach (var index in mentions.OrderByDescending(l => l.Mentions))
                        {
                            write.WriteLine("{0}, {1}, {2}, {3:P}", position++, index.Language, index.Mentions, (double)index.Mentions / allMentions);
                        }
                    }
                }
            }
        }

        private Uri GetUrl(string language)
        {
            language = Uri.EscapeDataString(language);
            var uri = string.Format("http://www.jobs.bg/front_job_search.php?keyword={0}", language);
            return new Uri(uri, UriKind.Absolute);
        }

        private static int GetCount(string site)
        {
            Regex r = new Regex("<td class=\"pagingtotal\">\\d - \\d* от (\\d*)</td>", RegexOptions.IgnoreCase);
            foreach (Match match in r.Matches(site))
            {
                int value = 0;
                if (int.TryParse(match.Groups[1].Value, out value))
                {
                    return value;
                }
            }
            return 0;
        }
    }

    public class Index
    {
        public string Language { get; set; }

        public int Mentions { get; set; }
    }
}
