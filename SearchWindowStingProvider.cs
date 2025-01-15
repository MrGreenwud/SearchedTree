#if UNITY_EDITOR

using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Enigmatic.SearchedTrees
{
    public class SearchWindowStingProvider : ScriptableObject, ISearchWindowProvider
    {
        private string[] m_Items;
        public event Action<string> OnSelect;

        public void Init(string[] elements, Action<string> action)
        {
            m_Items = elements;
            OnSelect += action;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>();

            List<string> sortedListItems = m_Items.ToList();
            sortedListItems.Sort((a, b) =>
            {
                string[] splitedA = a.Split("/");
                string[] splitedB = b.Split("/");

                for (int i = 0; i < splitedA.Length; i++)
                {
                    if (i >= splitedB.Length)
                        return 1;

                    int value = splitedA[i].CompareTo(splitedB[i]);

                    if (value != 0)
                    {
                        if (splitedA.Length != splitedB.Length &&
                        (i == splitedA.Length - 1 || i == splitedB.Length - 1))
                        {
                            return splitedA.Length < splitedB.Length ? 1 : -1;
                        }

                        return value;
                    }
                }

                return 0;
            });

            List<string> grups = new List<string>();

            foreach (string item in sortedListItems)
            {
                string[] entryTitle = item.Split("/");

                for (int i = 0; i < entryTitle.Length - 1; i++)
                {
                    if (grups.Contains(entryTitle[i]) == false)
                    {
                        searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent(entryTitle[i]), i));
                        grups.Add(entryTitle[i]);
                    }
                }

                SearchTreeEntry searchTreeEntry = new SearchTreeEntry(new GUIContent(entryTitle.Last()));
                searchTreeEntry.level = entryTitle.Length - 1;
                searchTreeEntry.userData = item;
                searchTreeEntries.Add(searchTreeEntry);
            }

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            OnSelect?.Invoke(SearchTreeEntry.userData.ToString());
            return true;
        }
    }
}

#endif