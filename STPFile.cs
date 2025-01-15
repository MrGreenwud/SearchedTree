#if UNITY_EDITOR

using System.IO;
using System.Collections.Generic;

using Enigmatic.Core;
using ENIX;

namespace Enigmatic.SearchedTrees
{
    public static class STPFile
    {
        public static void Save(SearchedTreeGroup group)
        {
            string path = $"{EnigmaticData.GetFullPath(SearchedTreeData.resources)}/{group.Value}";
            List<SearchedTree> roots = new List<SearchedTree>(group.ChildCount);

            while(group.ChildCount > 0)
            {
                SearchedTree root = group.GetChild(0);

                SearchedTreeGroup emptyGroup = new SearchedTreeGroup(root.Value);
                emptyGroup.AddChild(root);

                object[] objects = { root };
                Save(root.Value, ENIXSerializer.Serialize(objects), path);

                roots.Add(root);
            }

            foreach(SearchedTree root in roots)
                group.AddChild(root);
        }

        public static SearchedTreeGroup Load(string path)
        {
            if (File.Exists(path) == false)
                return null;

            StreamReader streamReader = new StreamReader(path);
            string file = streamReader.ReadToEnd();

            object[] trees = ENIXDeserializer.Deserialize(file);            

            SearchedTreeGroup group = null;

            foreach (object tree in trees)
            {
                if (tree.GetType() == typeof(SearchedTreeGroup))
                    group = (SearchedTreeGroup)tree;
            }

            group.UpdateAllLevel();

            streamReader.Close();

            return group;
        }

        private static void Save(string name, List<string> serializedObjects, string path)
        {
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);

            string stp = "#STP v1.0";

            foreach (string serializedObject in serializedObjects)
                stp += serializedObject;

            string pathWithFile = $"{path}/{name}.stp";
            File.WriteAllText(pathWithFile, stp);
        }
    }
}

#endif