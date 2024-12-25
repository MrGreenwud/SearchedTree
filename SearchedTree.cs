#if UNITY_EDITOR

using System;
using System.Collections.Generic;

using ENIX;

namespace Enigmatic.SearchedTree
{
    [SerializebleObject]
    public class SearchedTree
    {
        [SerializebleProperty] public string Value;

        [SerializebleProperty] private SearchedTree m_Parent;
        [SerializebleProperty] private List<SearchedTree> m_SearchedTrees = new List<SearchedTree>();

        public uint Level { get; private set; }

        public SearchedTree GetParent() => m_Parent;
        public SearchedTree GetChild(uint index) => m_SearchedTrees[(int)index];
        public SearchedTree[] GetСhildren() => m_SearchedTrees.ToArray();
        
        public int Count => m_SearchedTrees.Count;

        public SearchedTree() { }

        public SearchedTree(string value)
        {
            Value = value;
        }

        public void ForEach(Action<SearchedTree> action)
        {
            foreach(SearchedTree tree in m_SearchedTrees)
                action?.Invoke(tree);
        }

        public void AddParent(SearchedTree parent)
        {
            {
                if (m_Parent != null)
                    throw new InvalidOperationException("This search tree has a parent!");

                if (parent == null)
                    throw new ArgumentNullException("The passed parent is null!");
            }

            m_Parent = parent;
            Level = parent.Level + 1;
        }

        public SearchedTree AddChild(SearchedTree newChild)
        {
            {
                if (newChild == null)
                    throw new ArgumentNullException("This child searched tree is null!");

                if (m_SearchedTrees.Contains(newChild))
                    throw new ArgumentException("This searched tree has a this child!");

            }

            m_SearchedTrees.Add(newChild);
            newChild.AddParent(this);

            return newChild;
        }

        public void RemoveChild(SearchedTree child)
        {
            if (m_SearchedTrees.Contains(child) == false)
                throw new InvalidOperationException("This searched tree is not child the serched tree!");

            m_SearchedTrees.Remove(child);
        }

        public void UpdateLevel()
        {
            if (m_Parent == null)
            {
                Level = 0;
                return;
            }

            Level = m_Parent.Level + 1;
        }

        public void UpdateAllLevels()
        {
            UpdateLevel();

            foreach (SearchedTree tree in m_SearchedTrees)
                tree.UpdateAllLevels();
        }

        public int GetCount()
        {
            int count = 0;
            count += m_SearchedTrees.Count;

            if (m_Parent != null)
                count += m_Parent.GetCount();

            return count;
        }

        public SearchedTree[] GetAllTree()
        {
            List<SearchedTree> trees = new List<SearchedTree>(GetCount()) { this };

            foreach (SearchedTree tree in m_SearchedTrees)
            {
                if (tree.GetСhildren().Length > 0)
                    trees.AddRange(tree.GetAllTree());
                else
                    trees.Add(tree);
            }

            return trees.ToArray();
        }

        public string GetTree()
        {
            string tree = InvertedTree();
            string[] trees = tree.Split("/");
            string newTree = string.Empty;

            for(int i = trees.Length - 2; i >= 0; i--)
            {
                newTree += trees[i];

                if(i != 0)
                    newTree += "/";
            }

            return newTree;
        }

        private string InvertedTree()
        {
            string tree = Value;

            if (m_Parent != null)
                tree += $"/{m_Parent.InvertedTree()}";

            return tree;
        }

        public override string ToString() => GetTree();
    }
}

#endif