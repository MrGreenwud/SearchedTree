#if UNITY_EDITOR

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Enigmatic.SearchedTrees
{
    public class SearchedTreeListProvider : ScriptableObject, ISearchWindowProvider
    {
        private SearchedTree m_Root;
        private string m_SenderUID;

        public Action<string, SearchedTree> OnSelect;

        public static SearchedTreeListProvider Create(SearchedTree root, string senderUID = "0")
        {
            SearchedTreeListProvider provider = CreateInstance<SearchedTreeListProvider>();

            root.Parent?.RemoveChild(root);

            provider.m_Root = root;
            provider.m_SenderUID = senderUID;

            return provider;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return CreateSearchTree(m_Root);
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            OnSelect?.Invoke(m_SenderUID, (SearchedTree)SearchTreeEntry.userData);
            
            OnSelect = null;
            return true;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchedTree tree)
        {
            List<SearchTreeEntry> entries = new List<SearchTreeEntry>();
            tree.UpdateLevel();

            if (tree.ChildCount > 0)
            {
                entries.Add(new SearchTreeGroupEntry(new GUIContent(tree.Value), (int)tree.DepthLevel));

                tree.ForEach((x) =>
                {
                    List<SearchTreeEntry> childEntries = CreateSearchTree(x);
                    entries.AddRange(childEntries);
                });
            }
            else
            {
                SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(tree.Value));
                entry.level = (int)tree.DepthLevel;
                entry.userData = tree;
                entries.Add(entry);
            }

            return entries;
        }
    }
}

#endif