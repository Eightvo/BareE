using Assimp;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.DataStructures
{
    public class BinaryTree<T>
    {
        public BinaryTreeNode<T> Root;
        IComparer<T> NodeComparer;
        public BinaryTree(IComparer<T> nodeComparer)
        {
            NodeComparer = nodeComparer;
        }

        public void Insert(T toInsert)
        {
            if (Root == null)
            {
                Root = new BinaryTreeNode<T>(toInsert);
                return;
            }
            InsertIntoNode(Root, toInsert);
        }
        private void InsertIntoNode(BinaryTreeNode<T> node, T toInsert)
        {
            if (NodeComparer.Compare(toInsert, node.Value) < 0)
            {
                if (node.Left == null)
                    node.Left = new BinaryTreeNode<T>(toInsert);
                else
                {
                    InsertIntoNode(node.Left, toInsert);
                }
            }
            else
            {
                if (node.Right == null)
                    node.Right = new BinaryTreeNode<T>(toInsert);
                else
                {
                    InsertIntoNode(node.Right, toInsert);
                }
            }
        }

        public void Remove(T toRemove)
        {
            if (Root == null) return;

            if (NodeComparer.Compare(Root.Value, toRemove) == 0)
            {
                BinaryTreeNode<T> newRoot;
                if (Root.Left == null)
                {
                    if (Root.Right == null) { Root = null; return; };

                    newRoot = RemoveLeastNode(Root.Right);
                    newRoot.Right = Root.Right;
                    Root = newRoot;
                }
                else
                {
                    newRoot = RemoveGreatestNode(Root.Left);
                    newRoot.Right = Root.Right;
                    newRoot.Left = Root.Left;
                    Root = newRoot;
                }
            }
            else
                RemoveNode(Root, toRemove);

        }

        private void RemoveLeftChild(BinaryTreeNode<T> node)
        {
            if (node.Left == null) return;
            if (node.Left.Right == null) { node.Left = node.Left.Left; return; }
            var newLeft = RemoveLeastNode(node.Left.Right);
            if (newLeft == null)
                newLeft = node.Left.Right;
            else newLeft.Right = node.Left.Right;
            newLeft.Left = node.Left.Left;
            node.Left = newLeft;
        }
        private void RemoveRightChild(BinaryTreeNode<T> node)
        {
            if (node.Right == null) return;
            if (node.Right.Left == null) { node.Right = node.Right.Right; return; }
            var newRight = RemoveGreatestNode(node.Right.Left);
            if (newRight == null)
                newRight = node.Right.Left;
            else newRight.Left = node.Right.Left;
            newRight.Right = node.Right.Right;
            node.Right = newRight;
        }

        private void RemoveNode(BinaryTreeNode<T> node, T toRemove)
        {
            if (node == null) return;

            if (node.Left != null && NodeComparer.Compare(node.Left.Value, toRemove) == 0)
            {
                RemoveLeftChild(node);
                return;
            }
            if (node.Right != null && NodeComparer.Compare(node.Right.Value, toRemove) == 0)
            {
                RemoveRightChild(node);
                return;
            }
            var compRes = NodeComparer.Compare(toRemove, node.Value);
            if (compRes < 0)
                RemoveNode(node.Left, toRemove);
            else
                RemoveNode(node.Right, toRemove);

        }

        private BinaryTreeNode<T> RemoveLeastNode(BinaryTreeNode<T> node)
        {
            if (node.Left== null) return null;
            BinaryTreeNode<T> ret = RemoveGreatestNode(node.Left);
            if (ret != null) return ret;

            ret = node.Left;
            node.Left = null;
            return ret;
        }
        private BinaryTreeNode<T> RemoveGreatestNode(BinaryTreeNode<T> node)
        {
            //THe idea is we go all the way to the end, but returning null signals we found the end.
            //When we receive null we know that the right child is the last node in the chain.
            //when we receive non-null we know that thee GreatestChild was already found and removed.
            if (node.Right == null) return null;
            BinaryTreeNode<T> ret = RemoveGreatestNode(node.Right);
            if (ret != null) return ret;

            ret = node.Right;
            node.Right = null;
            return ret;
        }


        public IEnumerable<T> Values
        {
            get
            {
                foreach(T v in traverseNode(Root))
                    yield return v;
            }
        }

        private IEnumerable<T> traverseNode(BinaryTreeNode<T> node)
        {
            if (node.Left != null)
                foreach (T v in traverseNode(node.Left))
                    yield return v;
            yield return node.Value;
            if (node.Right != null)
                foreach (T v in traverseNode(node.Right))
                    yield return v;
        }

    }
    public class BinaryTreeNode<T>
    {
        public T Value;
        public BinaryTreeNode<T> Left;
        public BinaryTreeNode<T> Right;
        public BinaryTreeNode(T value) { Value = value; }

        public bool IsLeafNode()
        {
            return Left == null && Right == null;
        }

    }
}
