using System.Text.RegularExpressions;

namespace SVIndex.ViewModels
{
    public class Mention
    {
        public Mention()
        {
        }

        public Mention(string regex, string technology)
        {
            Technology = technology;
            Regex = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public Regex Regex { get; set; }

        public string Technology { get; set; }
    }
}