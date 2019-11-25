using System;

namespace MSR.CVE.BackMaker
{
    public class RBTreeNode<T>
    {
        private enum COLOR
        {
            BLACK,
            RED
        }

        private RedBlackTree<T> tree;
        private RBTreeNode<T> Left;
        private RBTreeNode<T> Right;
        private RBTreeNode<T> Parent;
        private COLOR Color = COLOR.RED;

        public T Data
        {
            get;
            set;
        }

        public RBTreeNode(RedBlackTree<T> tree) : this(tree, default(T))
        {
        }

        private RBTreeNode(RedBlackTree<T> tree, T value)
        {
            this.tree = tree;
            this.Data = value;
            Left = null;
            Right = null;
            Color = COLOR.RED;
            Parent = null;
        }

        internal static RBTreeNode<T> MakeRootNode(RedBlackTree<T> tree)
        {
            return new RBTreeNode<T>(tree) {Left = tree.NIL, Right = tree.NIL};
        }

        internal static RBTreeNode<T> MakeNilNode(RedBlackTree<T> tree)
        {
            RBTreeNode<T> rBTreeNode = new RBTreeNode<T>(tree);
            rBTreeNode.Color = COLOR.BLACK;
            rBTreeNode.Left = rBTreeNode.Right = rBTreeNode.Parent = rBTreeNode;
            return rBTreeNode;
        }

        public RBTreeNode<T> GetLeft()
        {
            return Left;
        }

        public RBTreeNode<T> GetRight()
        {
            return Right;
        }

        public RBTreeNode<T> GetRoot()
        {
            return tree.root;
        }

        private bool IsRed(RBTreeNode<T> T)
        {
            return T.Color == COLOR.RED;
        }

        private bool IsBlack(RBTreeNode<T> T)
        {
            return T.Color == COLOR.BLACK;
        }

        public bool IsRed()
        {
            return Color == COLOR.RED;
        }

        public bool IsBlack()
        {
            return Color == COLOR.BLACK;
        }

        private RBTreeNode<T> GetGrandFather()
        {
            if (Parent == null)
            {
                return null;
            }

            if (Parent.Parent != null)
            {
                return Parent.Parent;
            }

            return null;
        }

        private RBTreeNode<T> GetRightUncle()
        {
            RBTreeNode<T> grandFather = GetGrandFather();
            if (grandFather != null)
            {
                return grandFather.Right;
            }

            return null;
        }

        private RBTreeNode<T> GetLeftUncle()
        {
            RBTreeNode<T> grandFather = GetGrandFather();
            if (grandFather != null)
            {
                return grandFather.Left;
            }

            return null;
        }

        private bool IsFatherLeftofGrandFather()
        {
            return !IsRoot() && Parent.Parent != null && Parent.Parent.Left == this;
        }

        private bool IsFatherRightofGrandFather()
        {
            return !IsRoot() && Parent.Parent != null && Parent.Parent.Right == this;
        }

        private void RotateLeft(RBTreeNode<T> x)
        {
            RBTreeNode<T> right = x.Right;
            if (right != tree.NIL)
            {
                x.Right = right.Left;
                if (right.Left != tree.NIL)
                {
                    right.Left.Parent = x;
                }

                right.Parent = x.Parent;
                if (x.Parent == null || x.Parent == tree.NIL)
                {
                    tree.root = right;
                }
                else
                {
                    if (x.Parent.Left == x)
                    {
                        x.Parent.Left = right;
                    }
                    else
                    {
                        x.Parent.Right = right;
                    }
                }

                right.Left = x;
                x.Parent = right;
            }
        }

        private void RotateRight(RBTreeNode<T> x)
        {
            RBTreeNode<T> left = x.Left;
            if (left != tree.NIL)
            {
                x.Left = left.Right;
                if (left.Right != tree.NIL)
                {
                    left.Right.Parent = x;
                }

                left.Parent = x.Parent;
                if (x.Parent == null || x.Parent == tree.NIL)
                {
                    tree.root = left;
                }
                else
                {
                    if (x.Parent.Right == x)
                    {
                        x.Parent.Right = left;
                    }
                    else
                    {
                        x.Parent.Left = left;
                    }
                }

                left.Right = x;
                x.Parent = left;
            }
        }

        private bool IsRoot(RBTreeNode<T> T)
        {
            return T.Parent == null;
        }

        private bool IsRoot()
        {
            return Parent == null;
        }

        private void ColorGrandFather(COLOR c)
        {
            if (Parent.Parent != null)
            {
                Parent.Parent.Color = c;
            }
        }

        private void ColorFather(COLOR c)
        {
            if (Parent != null)
            {
                Parent.Color = c;
            }
        }

        private void CheckBalance()
        {
            if (!IsRoot() && Parent.IsRed())
            {
                if (IsFatherLeftofGrandFather())
                {
                    RBTreeNode<T> rightUncle = GetRightUncle();
                    if (rightUncle != tree.NIL && rightUncle != null && rightUncle.IsRed())
                    {
                        ColorGrandFather(COLOR.RED);
                        rightUncle.Color = COLOR.BLACK;
                        Parent.Color = COLOR.BLACK;
                        RBTreeNode<T> grandFather = GetGrandFather();
                        if (grandFather != null)
                        {
                            grandFather.CheckBalance();
                        }
                    }
                    else
                    {
                        if (this == Parent.Right)
                        {
                            RotateLeft(Parent);
                        }

                        ColorFather(COLOR.BLACK);
                        ColorGrandFather(COLOR.RED);
                        if (Parent.Parent != null)
                        {
                            RotateRight(Parent.Parent);
                        }
                    }
                }
                else
                {
                    RBTreeNode<T> leftUncle = GetLeftUncle();
                    if (leftUncle != tree.NIL && leftUncle != null && leftUncle.IsRed())
                    {
                        ColorGrandFather(COLOR.RED);
                        leftUncle.Color = COLOR.BLACK;
                        Parent.Color = COLOR.BLACK;
                        RBTreeNode<T> grandFather2 = GetGrandFather();
                        if (grandFather2 != null)
                        {
                            grandFather2.CheckBalance();
                        }
                    }
                    else
                    {
                        if (this == Parent.Left)
                        {
                            RotateRight(Parent);
                        }

                        ColorFather(COLOR.BLACK);
                        ColorGrandFather(COLOR.RED);
                        if (Parent.Parent != null)
                        {
                            RotateLeft(Parent.Parent);
                        }
                    }
                }
            }

            tree.root.Color = COLOR.BLACK;
        }

        private RBTreeNode<T> TreeMinimum(RBTreeNode<T> T)
        {
            if (T == null)
            {
                return null;
            }

            while (T.Left != tree.NIL)
            {
                T = T.Left;
            }

            return T;
        }

        private RBTreeNode<T> TreeMaximum(RBTreeNode<T> T)
        {
            if (T == null)
            {
                return null;
            }

            while (T.Right != tree.NIL)
            {
                T = T.Right;
            }

            return T;
        }

        private RBTreeNode<T> TreeSuccessor(RBTreeNode<T> T)
        {
            if (T == tree.NIL)
            {
                return null;
            }

            if (T.Right != tree.NIL)
            {
                return TreeMinimum(T.Right);
            }

            RBTreeNode<T> parent = T.Parent;
            while (parent != null && T == parent.Right)
            {
                T = parent;
                parent = parent.Parent;
            }

            return parent;
        }

        private RBTreeNode<T> Search(RBTreeNode<T> tn, T Value)
        {
            while (tn != tree.NIL && tree.comparer.Compare(tn.Data, Value) != 0)
            {
                if (tree.comparer.Compare(Value, tn.Data) < 0)
                {
                    tn = tn.Left;
                }
                else
                {
                    tn = tn.Right;
                }
            }

            return tn;
        }

        internal bool InsertValue(T newValue)
        {
            RBTreeNode<T> rBTreeNode = this;
            if (tree.NIL != rBTreeNode.Search(rBTreeNode, newValue))
            {
                return false;
            }

            while (rBTreeNode != tree.NIL)
            {
                if (tree.comparer.Compare(newValue, rBTreeNode.Data) <= 0)
                {
                    if (rBTreeNode.Left == tree.NIL)
                    {
                        break;
                    }

                    rBTreeNode = rBTreeNode.Left;
                }
                else
                {
                    if (rBTreeNode.Right == tree.NIL)
                    {
                        break;
                    }

                    rBTreeNode = rBTreeNode.Right;
                }
            }

            if (tree.comparer.Compare(newValue, rBTreeNode.Data) <= 0)
            {
                RBTreeNode<T> rBTreeNode2 = new RBTreeNode<T>(tree, newValue);
                rBTreeNode2.Color = COLOR.RED;
                rBTreeNode.Left = rBTreeNode2;
                rBTreeNode2.Parent = rBTreeNode;
                rBTreeNode2.Left = rBTreeNode2.Right = tree.NIL;
                rBTreeNode2.CheckBalance();
            }
            else
            {
                RBTreeNode<T> rBTreeNode3 = new RBTreeNode<T>(tree, newValue);
                rBTreeNode3.Color = COLOR.RED;
                rBTreeNode.Right = rBTreeNode3;
                rBTreeNode3.Parent = rBTreeNode;
                rBTreeNode3.Left = rBTreeNode3.Right = tree.NIL;
                rBTreeNode3.CheckBalance();
            }

            return true;
        }

        public void Delete(T i)
        {
            RBTreeNode<T> z = Search(tree.root, i);
            RBDelete(z);
        }

        private RBTreeNode<T> RBDelete(RBTreeNode<T> z)
        {
            RBTreeNode<T> rBTreeNode;
            if (z.Left == tree.NIL || z.Right == tree.NIL)
            {
                rBTreeNode = z;
            }
            else
            {
                rBTreeNode = TreeSuccessor(z);
            }

            RBTreeNode<T> rBTreeNode2;
            if (rBTreeNode.Left != tree.NIL)
            {
                rBTreeNode2 = rBTreeNode.Left;
            }
            else
            {
                rBTreeNode2 = rBTreeNode.Right;
            }

            if (tree.root == (rBTreeNode2.Parent = rBTreeNode.Parent))
            {
                tree.root.Left = rBTreeNode2;
            }
            else
            {
                if (rBTreeNode == rBTreeNode.Parent.Left)
                {
                    rBTreeNode.Parent.Left = rBTreeNode2;
                }
                else
                {
                    rBTreeNode.Parent.Right = rBTreeNode2;
                }
            }

            if (rBTreeNode != z)
            {
                z.Data = rBTreeNode.Data;
            }

            if (rBTreeNode.IsBlack())
            {
                RBDeleteFixUp(rBTreeNode2);
            }

            return rBTreeNode;
        }

        private void RBDeleteFixUp(RBTreeNode<T> x)
        {
            while (x != tree.root && x.IsBlack())
            {
                if (x == x.Parent.Left)
                {
                    RBTreeNode<T> right = x.Parent.Right;
                    if (right.IsRed())
                    {
                        right.Color = COLOR.BLACK;
                        x.Parent.Color = COLOR.RED;
                        RotateLeft(x.Parent);
                        right = x.Parent.Right;
                    }

                    if (right.Left.IsBlack() && right.Right.IsBlack())
                    {
                        right.Color = COLOR.RED;
                        x = x.Parent;
                    }
                    else
                    {
                        if (right.Right.IsBlack())
                        {
                            right.Left.Color = COLOR.BLACK;
                            right.Color = COLOR.RED;
                            RotateRight(right);
                            right = x.Parent.Right;
                        }

                        right.Color = x.Parent.Color;
                        x.Parent.Color = COLOR.BLACK;
                        right.Right.Color = COLOR.BLACK;
                        RotateLeft(x.Parent);
                        x = tree.root;
                    }
                }
                else
                {
                    RBTreeNode<T> left = x.Parent.Left;
                    if (left.IsRed())
                    {
                        left.Color = COLOR.BLACK;
                        x.Parent.Color = COLOR.RED;
                        RotateRight(x.Parent);
                        left = x.Parent.Left;
                    }

                    if (left.Right.IsBlack() && left.Left.IsBlack())
                    {
                        left.Color = COLOR.RED;
                        x = x.Parent;
                    }
                    else
                    {
                        if (left.Left.IsBlack())
                        {
                            left.Right.Color = COLOR.BLACK;
                            left.Color = COLOR.RED;
                            RotateRight(left);
                            left = x.Parent.Left;
                        }

                        left.Color = x.Parent.Color;
                        x.Parent.Color = COLOR.BLACK;
                        left.Left.Color = COLOR.BLACK;
                        RotateRight(x.Parent);
                        x = tree.root;
                    }
                }
            }

            x.Color = COLOR.BLACK;
        }

        public void Print(int depth, bool Tab)
        {
            if (this != tree.NIL)
            {
                Right.Print(depth + 1, Tab);
                int num = 0;
                while (num++ < depth && Tab)
                {
                    Console.Write("   ");
                }

                Console.WriteLine(Data + "-" + Convert.ToString(Color));
                Left.Print(depth + 1, Tab);
            }
        }

        private static RBTreeNode<T> GetMaxRight(ref RBTreeNode<T> Cursor)
        {
            if (Cursor == null)
            {
                return null;
            }

            if (Cursor.Right != null)
            {
                Cursor = Cursor.Right;
                GetMaxRight(ref Cursor);
            }

            return Cursor;
        }

        public static void MakeNull(ref RBTreeNode<T> root)
        {
            if (root != null)
            {
                if (root.tree.comparer.Compare(root.Data, default(T)) == 0)
                {
                    root = null;
                    return;
                }

                MakeNull(ref root.Left);
                MakeNull(ref root.Right);
            }
        }
    }
}
