using System.Collections.Generic;
namespace SVIndex
{
    public class SVIndexByMonth
    {
        public int Month { get; set; }
        public IEnumerable<SVIndexInfo> SVIndices { get; set; }
    }

    public class SVIndexInfo
    {
        public int Position { get; set; }

        public string Language { get; set; }

        public int Mentions { get; set; }

        public double Index { get; set; }
    }
}