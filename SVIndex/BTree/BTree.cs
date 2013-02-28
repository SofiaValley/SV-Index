using System.Collections.Generic;

namespace SVIndex
{
    public class BTree
    {
        private readonly object _insertSyncLock = new object();
        public readonly BTreeLeaf FirstLeaf;

        public BTree()
        {
            FirstLeaf = new BTreeLeaf(_insertSyncLock);
        }

        public void Add(string key, object value)
        {
            BTreeLeaf current = FirstLeaf;
            foreach (char c in key)
            {
                if (current.Dictionary.ContainsKey(c))
                    current = current.Dictionary[c];
                else
                {
                    BTreeLeaf old = current;
                    current = new BTreeLeaf(_insertSyncLock);
                    old.Dictionary.Add(c, current);
                }
            }
            current.Value = value;
        }

        public class BTreeLeaf
        {
            protected readonly object InsertSyncLock;
            public readonly Dictionary<char, BTreeLeaf> Dictionary = new Dictionary<char, BTreeLeaf>();

            public BTreeLeaf(object insertSyncLock)
            {
                InsertSyncLock = insertSyncLock;
            }

            public BTreeLeaf this[char c]
            {
                get
                {
                    return Dictionary.ContainsKey(c)
                        ? Dictionary[c]
                        : null;
                }
            }

            public object Value { get; set; }
        }
    }
}