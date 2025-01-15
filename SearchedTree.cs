#if UNITY_EDITOR

using System.Collections.Generic;
using Enigmatic.Core;
using ENIX;

namespace Enigmatic.SearchedTrees
{
    [SerializebleObject]
    public class SearchedTree : TreeNode<SearchedTree>
    {
        [SerializebleProperty] public string Value;

        public SearchedTree() { }

        public SearchedTree(string value)
        {
            Value = value;
        }

        public int GetCount()
        {
            int count = 0;
            count += ChildCount;

            if (Parent != null)
                count += Parent.GetCount();

            return count;
        }

        public SearchedTree[] GetAllTree()
        {
            List<SearchedTree> trees = new List<SearchedTree>(GetCount()) { this };

            ForEach((tree) =>
            {
                if (tree.GetChildren().Length > 0)
                    trees.AddRange(tree.GetAllTree());
                else
                    trees.Add(tree);
            });

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

            if (Parent != null)
                tree += $"/{Parent.InvertedTree()}";

            return tree;
        }

        public override string ToString() => GetTree();
    }
}

#endif