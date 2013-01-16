namespace SV_PLI
{
    public class TreeWalker
    {
        private readonly BTree _tree;
        private BTree.BTreeLeaf _lastLeaf;

        public TreeWalker(BTree tree)
        {
            _tree = tree;
            Reset();
        }

        public void Reset()
        {
            _lastLeaf = _tree.FirstLeaf;
            HasMoreValues = true;
            LastValue = null;
        }

        public void GetNext(char c)
        {
            _lastLeaf = _lastLeaf[c];
            if (_lastLeaf == null)
                HasMoreValues = false;
            else
            {
                LastValue = _lastLeaf.Value;
                HasMoreValues = _lastLeaf.Dictionary.Count != 0;
            }
        }

        public bool HasMoreValues { get; private set; }
        public object LastValue { get; private set; }
    }
}