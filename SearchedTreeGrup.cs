#if UNITY_EDITOR

using System;
using System.Collections.Generic;

namespace Enigmatic.SearchedTree
{
    public class SearchedTreeGrup
    {
        public string Name;

        private List<SearchedTree> m_SearchedTrees { get; set; }

        public SearchedTree[] SearchedTrees => m_SearchedTrees.ToArray();

        public SearchedTreeGrup(string name)
        {
            Name = name;
            m_SearchedTrees = new List<SearchedTree>();
        }

        public void ForEach(Action<SearchedTree> action)
        {
            foreach(SearchedTree tree in m_SearchedTrees)
                action?.Invoke(tree);
        }

        public void Add(SearchedTree newSearchedTree) => m_SearchedTrees.Add(newSearchedTree);

        public void Remove(SearchedTree newSearchedTree) => m_SearchedTrees.Remove(newSearchedTree);

        public bool TryRemove(SearchedTree newSearchedTree)
        {
            if (m_SearchedTrees.Contains(newSearchedTree) == false)
                return false;

            m_SearchedTrees.Remove(newSearchedTree);
            return true;
        }
    }
}

#endif