using System.Collections.Generic;

namespace MSR.CVE.BackMaker
{
    public class RedBlackTree<T>
    {
        internal RBTreeNode<T> NIL;
        internal RBTreeNode<T> root;
        internal IComparer<T> comparer;

        public RedBlackTree()
        {
            NIL = RBTreeNode<T>.MakeNilNode(this);
            root = null;
        }

        public void Insert(T value)
        {
            root.InsertValue(value);
        }
    }
}
