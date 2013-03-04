using System.Collections.Generic;

namespace SVIndex.ViewModels
{
    public class SVIndexByMonth
    {
        public string Id { get; set; }

        public IEnumerable<SVIndexInfo> SVIndices { get; set; }
    }

    public class SVIndexInfo
    {
        public string Id { get; set; }
        public int Position { get; set; }
        public string Language { get; set; }
        public int Mentions { get; set; }
        public double Index { get; set; }
        public double Delta { get; set; }
    }
}