#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

using Enigmatic.Core;
using Enigmatic.Core.Editor;

using ENIX;

namespace Enigmatic.SearchedTree
{
    public static class SearchedTreeUtility
    {
        public static string DeCompileTree(string tree, uint depthLevel)
        {
            string[] tempTree = tree.Split("/");

            if (tempTree.Length < depthLevel)
            {
                throw new Exception($"This search tree has a maximum depth of {tempTree.Length}. " +
                    $"It is not possible to take an element from a depth of {depthLevel}");
            }

            return tempTree[depthLevel];
        }

        public static string CompileTree(List<string> values)
        {
            string tree = "";

            for (int i = 1; i < values.Count; i++)
            {
                tree += values[i];

                if (i + 1 != values.Count)
                    tree += "/";
            }

            return tree;
        }

        public static void DrawSelectionTree(string name, string value, float width, SearchedTreeListProvider provider)
        {
            EnigmaticGUILayout.BeginHorizontal(EnigmaticGUILayout.ExpandWidth(true), EnigmaticGUILayout.ExpandHeight(true),
                EnigmaticGUILayout.ElementSpacing(0), EnigmaticGUILayout.Padding(0));
            {
                EnigmaticGUILayout.Lable(name);

                EnigmaticGUILayout.Space(width / 2 - EnigmaticGUILayout.GetLastGUIRect().width);

                if (EnigmaticGUILayout.Button(value, new Vector2(width / 2, 18), EditorStyles.popup))
                {
                    Vector2 position = EnigmaticGUILayout.GetLastGUIRect().position + Vector2.one * 36 + Vector2.right * 85;
                    SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(position)), provider);
                }
            }
            EnigmaticGUILayout.EndHorizontal();
        }

        public static SearchedTree LoadTree(string path, string grup, string root)
        {
            string fullPath = EnigmaticData.GetFullPath($"{path}/{grup}/{root}.enix");

            StreamReader streamReader = new StreamReader(fullPath);
            string file = streamReader.ReadToEnd();

            object[] trees = ENIXDeserializer.Deserialize(file);

            streamReader.Close();
            return (SearchedTree)trees.Last();
        }
    }
}

#endif