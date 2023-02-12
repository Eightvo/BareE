using System;
using System.Collections;
using System.Collections.Generic;

namespace BareE.DataStructures
{
    public class BinaryKeyValueTree<K,V>:IEnumerable<KeyValuePair<K,V>>
    {
        public BinaryKeyValueTreeNode<K,V> Root;
        IComparer<K> NodeComparer;
        public BinaryKeyValueTree(IComparer<K> keyComparer)
        {
            NodeComparer = keyComparer;
        }

        public void Insert(K key, V toInsert)
        {
            if (Root == null)
            {
                Root = new BinaryKeyValueTreeNode<K,V>(key, toInsert);
                return;
            }
            InsertIntoNode(Root,key, toInsert);
        }
        private void InsertIntoNode(BinaryKeyValueTreeNode<K,V> node, K key, V toInsert)
        {
            var comparison = NodeComparer.Compare(key, node.Key);
            if (comparison<0)
            {
                if (node.Left == null)
                    node.Left = new BinaryKeyValueTreeNode<K,V>(key, toInsert);
                else
                {
                    InsertIntoNode(node.Left,key, toInsert);
                }
            }
            else if (comparison>0)
            {
                if (node.Right == null)
                    node.Right = new BinaryKeyValueTreeNode<K,V>(key, toInsert);
                else
                {
                    InsertIntoNode(node.Right, key, toInsert);
                }
            }
            else
            {
                node.Value = toInsert;
            }
        }

        public void Remove(K keyToRemove)
        {
            if (Root == null) return;

            if (NodeComparer.Compare(Root.Key, keyToRemove) == 0)
            {
                BinaryKeyValueTreeNode<K,V> newRoot;
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
                RemoveNode(Root, keyToRemove);

        }

        private void RemoveLeftChild(BinaryKeyValueTreeNode<K,V> node)
        {
            if (node.Left == null) return;
            if (node.Left.Right == null) { node.Left = node.Left.Left; return; }
            var newLeft = RemoveLeastNode(node.Left.Right);
            if (newLeft == null)
                newLeft = node.Left.Right;
            else newLeft.Right=node.Left.Right;
            newLeft.Left=node.Left.Left;
            node.Left=newLeft;
        }
        private void RemoveRightChild(BinaryKeyValueTreeNode<K,V> node)
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

        private void RemoveNode(BinaryKeyValueTreeNode<K,V> node, K keyToRemove)
        {
            if (node == null) return;

            if (node.Left != null && NodeComparer.Compare(node.Left.Key, keyToRemove) == 0)
            {
                RemoveLeftChild(node);
                return;
            }
            if (node.Right != null && NodeComparer.Compare(node.Right.Key, keyToRemove) == 0)
            {
                RemoveRightChild(node);
                return;
            }
            var compRes = NodeComparer.Compare(keyToRemove, node.Key);
            if (compRes < 0)
                RemoveNode(node.Left, keyToRemove);
            else
                RemoveNode(node.Right, keyToRemove);
        }

        public V Lookup(K key)
        {
            return Lookup(Root, key);
        }
        private V Lookup(BinaryKeyValueTreeNode<K,V> node, K key)
        {
            if (node == null) return default(V);
            var compRes = NodeComparer.Compare(key, node.Key);
            if (compRes == 0) return node.Value;
            if (compRes < 0)
                return Lookup(node.Left, key);
            return Lookup(node.Right, key);
        }
        private BinaryKeyValueTreeNode<K,V> RemoveLeastNode(BinaryKeyValueTreeNode<K,V> node)
        {
            if (node.Left == null) return null;
            BinaryKeyValueTreeNode<K,V> ret = RemoveGreatestNode(node.Left);
            if (ret != null) return ret;

            ret = node.Left;
            node.Left = null;
            return ret;
        }
        private BinaryKeyValueTreeNode<K,V> RemoveGreatestNode(BinaryKeyValueTreeNode<K,V> node)
        {
            //THe idea is we go all the way to the end, but returning null signals we found the end.
            //When we receive null we know that the right child is the last node in the chain.
            //when we receive non-null we know that thee GreatestChild was already found and removed.
            if (node.Right == null) return null;
            BinaryKeyValueTreeNode<K,V> ret = RemoveGreatestNode(node.Right);
            if (ret != null) return ret;

            ret = node.Right;
            node.Right = null;
            return ret;
        }


        public IEnumerable<V> Values
        {
            get
            {
                foreach (BinaryKeyValueTreeNode<K,V> v in traverseNode(Root))
                    yield return v.Value;
            }
        }
        public IEnumerable<K> Keys
        {
            get
            {
                foreach (BinaryKeyValueTreeNode<K, V> v in traverseNode(Root))
                    yield return v.Key;
            }
        }
        public IEnumerable<KeyValuePair<K,V>> KeyValuePairs
        {
            get
            {
                foreach (BinaryKeyValueTreeNode<K, V> v in traverseNode(Root))
                    yield return new KeyValuePair<K, V>(v.Key, v.Value);
            }
        }
        private IEnumerable<BinaryKeyValueTreeNode<K,V>> traverseNode(BinaryKeyValueTreeNode<K,V> node)
        {
            if (node.Left != null)
                foreach (var v in traverseNode(node.Left))
                    yield return v;
            yield return node;
            if (node.Right != null)
                foreach (var v in traverseNode(node.Right))
                    yield return v;
        }
        public bool ContainsKey(K key)
        {
            if (Root == null) return false;
            return ContainsKey(Root, key);
        }
        private bool ContainsKey(BinaryKeyValueTreeNode<K,V> node, K key)
        {
            if (node == null) return false;
            var compareResult = NodeComparer.Compare(key, node.Key);
            if (compareResult == 0) return true;
            if (compareResult<0)
                return ContainsKey((BinaryKeyValueTreeNode<K,V>)node.Left, key);
            else return ContainsKey((BinaryKeyValueTreeNode<K,V>)node.Right, key);
        }

        public IEnumerator<KeyValuePair<K,V>> GetEnumerator()
        {
            return KeyValuePairs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return KeyValuePairs.GetEnumerator();
        }

        public void DebugPrint()
        {
            if (Root == null) { Console.WriteLine("[NULL]"); return; }
            int depth = 0;
            DebugPrint(Root, depth);
        }
        private void DebugPrint(BinaryKeyValueTreeNode<K,V> node, int depth)
        {
            if (node == null) return;
            DebugPrint(node.Left, depth + 1);

            String padLeft = "".PadLeft(depth);
            Console.WriteLine($"{padLeft}[{node.Key}]=>{node.Value}");
            DebugPrint(node.Right, depth + 1);

        }

        internal void Clear()
        {
            Clear(Root);
            Root = null;
        }
        private void Clear(BinaryKeyValueTreeNode<K, V> node)
        {
            if (node == null) return;
            Clear(node.Left);
            node.Left = null;
            Clear(node.Right);
            node.Right = null;
        }

        public V this[K key]
        {
            get
            {
                return Lookup(key);
            }
            set
            {
                Insert(key, value);
            }
        }

    }

    public class BinaryKeyValueTreeNode<K,V>
    {
        public K Key;
        public V Value;
        public BinaryKeyValueTreeNode<K,V> Left;
        public BinaryKeyValueTreeNode<K,V> Right;
        public BinaryKeyValueTreeNode(K key, V value) { Key = key; Value = value; }

        public bool IsLeafNode()
        {
            return Left == null && Right == null;
        }

    }
}
