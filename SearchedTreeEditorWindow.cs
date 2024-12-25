#if UNITY_EDITOR

using System.IO;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Enigmatic.Core;
using Enigmatic.Core.Editor;

using ENIX;
using System;

namespace Enigmatic.SearchedTree
{
    public class SearchedTreeEditorWindow : EnigmaticWindow
    {
        private List<SearchedTreeGrup> m_SearchedTreeGrups = new List<SearchedTreeGrup>();
        private Dictionary<SearchedTree, bool> m_Trees = new Dictionary<SearchedTree, bool>();

        private SearchedTreeGrup m_SelectedGrup;
        private SearchedTree m_SelectedSearchedTree;
        private SearchedTree m_LoaclViewSearchedTree;

        private Vector2 m_GrupScrollPosition;
        private Vector2 m_SearchTreeScrollPosition;

        private Queue<SearchedTree> m_DestroedSearchedTree = new Queue<SearchedTree>();

        private float m_GrupColumnWitdh => position.width * 0.22f;
        private float m_TreeViewColumnWitdh => position.width * 0.515f;
        private float m_SettingsColumnWitdh => position.width * 0.25f;

        private float m_ColumnHeight => position.height - 27;

        private int m_WhereIndex = 0;


        [MenuItem("Tools/Enigmatic/Searched Tree Editor")]
        public static void Open()
        {
            SearchedTreeEditorWindow searchableEditorWindow = GetWindow<SearchedTreeEditorWindow>();
            searchableEditorWindow.titleContent = new GUIContent("Searched Tree Editor");
            searchableEditorWindow.minSize = new Vector2(800, 600);
        }

        protected override void Draw()
        {
            base.Draw();

            DrawingUtilities.BeginToolBar(position.width);
            {
                if (EnigmaticGUILayout.Button("Save", EditorStyles.toolbarButton))
                    Save();

                if (EnigmaticGUILayout.Button("Load", EditorStyles.toolbarButton))
                    Load();

                EnigmaticGUILayout.Space(8);

                if (EnigmaticGUILayout.Button("Generate", EditorStyles.toolbarButton))
                {
                    SearchedTreeGeneratorWindow window = SearchedTreeGeneratorWindow.Open();
                    window.OnGenerated += GenerateTree;
                }
            }
            DrawingUtilities.EndToolBar();

            EnigmaticGUILayout.BeginHorizontal(EnigmaticGUILayout.Width(position.width), EnigmaticGUILayout.Height(position.height - 20));
            {
                DrawingUtilities.BeginColum(m_GrupColumnWitdh, m_ColumnHeight, - 1);
                {
                    DrawingUtilities.ColumnTitle(EnigmaticStyles.columnBackground, "Grup", 
                        m_GrupColumnWitdh, true, m_GrupColumnWitdh - 91, AddGrup, RemoveGrup);

                    DrawingUtilities.BeginVerticalScrollViewByGrup(m_GrupScrollPosition, new Vector2(0, 21), -1);
                    {
                        DrawGrups();
                    }
                    m_GrupScrollPosition = DrawingUtilities.EndVerticalScrollViewByGrup();
                }
                DrawingUtilities.EndColum();

                DrawingUtilities.BeginColum(m_TreeViewColumnWitdh, m_ColumnHeight);
                {
                    DrawingUtilities.ColumnTitle(EnigmaticStyles.columnBackground, "Tree View", m_TreeViewColumnWitdh,
                        true, m_TreeViewColumnWitdh - 122, AddRootTree, RemoveSelectedTree);

                    DrawingUtilities.BeginVerticalScrollViewByGrup(m_SearchTreeScrollPosition, new Vector2(1, 22), 0);
                    {
                        DrawTrees();
                    }
                    m_SearchTreeScrollPosition = DrawingUtilities.EndVerticalScrollViewByGrup();
                }
                DrawingUtilities.EndColum();

                DrawingUtilities.BeginColum(m_SettingsColumnWitdh, m_ColumnHeight);
                {
                    DrawingUtilities.ColumnTitle(EnigmaticStyles.columnBackground, "Settings", m_SettingsColumnWitdh);

                    DrawSettings();
                }
                DrawingUtilities.EndColum();
            }
            EnigmaticGUILayout.EndHorizontal();

            RemoveTrees();
        }

        private void AddGrup()
        {
            m_SearchedTreeGrups.Add(new SearchedTreeGrup("New Grup"));
            Repaint();
        }

        private void RemoveGrup()
        {
            if (m_SelectedGrup == null)
                return;

            m_SearchedTreeGrups.Remove(m_SelectedGrup);
            m_SelectedGrup = null;
            Repaint();
        }

        private void DrawGrups()
        {
            foreach (SearchedTreeGrup grup in m_SearchedTreeGrups)
                DrawGrup(grup);
        }

        private void DrawGrup(SearchedTreeGrup grup)
        {
            float width = EnigmaticGUILayout.GetActiveGrup().Rect.width;
            Vector2 size = new Vector2(width, 20);

            GUIStyle style = m_SelectedGrup == grup ? EnigmaticStyles.columnBackgroundSelected : EnigmaticStyles.columnBackground;

            if (EnigmaticGUILayout.Button(grup.Name, size, style))
            {
                m_SelectedGrup = grup;
                Repaint();
            }
        }

        private void DrawTrees()
        {
            if(m_SelectedGrup == null)
                return;

            m_WhereIndex = 0;
            m_SelectedGrup.ForEach(DrawTree);
        }

        private void DrawTree(SearchedTree tree)
        {
            bool expanded = m_Trees[tree];
            m_WhereIndex++;

            EnigmaticGUILayout.BeginHorizontal(EnigmaticGUILayout.Width(m_TreeViewColumnWitdh - 1),
                EnigmaticGUILayout.Height(21), EnigmaticGUILayout.Clickable(true));
            {
                EnigmaticGUILayout.Space(tree.Level * 16);

                if (m_WhereIndex % 2 == 0)
                    EnigmaticGUILayout.FillGrup(EnigmaticStyles.DarkedBackgroundColor);
                else
                    EnigmaticGUILayout.FillGrup(EnigmaticStyles.BackgroundColor);

                if (tree == m_SelectedSearchedTree)
                    EnigmaticGUILayout.FillGrup(EnigmaticStyles.DarkThemeBlueElementSelected);

                if (tree.Count > 0)
                {
                    EnigmaticGUILayout.BeginFoldout(ref expanded, "", Repaint, null, 16);
                    EnigmaticGUILayout.EndFoldout(expanded);
                }
                else
                {
                    EnigmaticGUILayout.Space(16);
                }

                EnigmaticGUILayout.Lable(tree.Value);
            }
            if (EnigmaticGUILayout.EndHorizontal())
                m_SelectedSearchedTree = tree;

            if (expanded)
                tree.ForEach(DrawTree);

            m_Trees[tree] = expanded;
        }

        private void DrawSettings()
        {
            if(m_SelectedGrup == null)
                return;

            EnigmaticGUILayout.BeginVertical(EnigmaticGUILayout.Width(m_SettingsColumnWitdh), 
                EnigmaticGUILayout.ExpandHeight(true), EnigmaticGUILayout.Padding(6));
            {
                m_SelectedGrup.Name = EnigmaticGUILayout.TextField("Grup Name:", m_SelectedGrup.Name);

                EnigmaticGUILayout.Space(6);
                EnigmaticGUILayout.Image(new Vector2(m_SettingsColumnWitdh - 12, -1), EnigmaticStyles.columnBackground);
                EnigmaticGUILayout.Space(6);

                if (m_SelectedSearchedTree != null)
                {
                    m_SelectedSearchedTree.Value = EnigmaticGUILayout.TextField("Tree Value:", m_SelectedSearchedTree.Value);

                    EnigmaticGUILayout.Space(3);

                    EnigmaticGUILayout.BeginHorizontal(EnigmaticGUILayout.Width(m_SettingsColumnWitdh),
                        EnigmaticGUILayout.ExpandHeight(true), EnigmaticGUILayout.Padding(0));
                    {
                        float buttonWidth = (m_SettingsColumnWitdh - 12) / 2;

                        if(EnigmaticGUILayout.Button("Add", new Vector2(buttonWidth, 18)))
                            OnAddedTree(m_SelectedSearchedTree);

                        EnigmaticGUILayout.Button("Remove", new Vector2(buttonWidth, 18));
                    }
                    EnigmaticGUILayout.EndHorizontal();
                }
            }
            EnigmaticGUILayout.EndVertical();
        }

        private void Load()
        {
            string path = EditorUtility.OpenFilePanel("Select a folder", Application.dataPath, "enix");

            if (string.IsNullOrEmpty(path) == false)
                Load(path);
        }

        private void Load(string path)
        {
            string filePath = $"{path.Replace('\\', '/')}";
            string[] pathSplited = filePath.Split('/');
            string grupName = pathSplited[pathSplited.Length - 2];

            SearchedTreeGrup grup = null;

            m_SearchedTreeGrups.ForEach((x) => 
            {
                if(x.Name == grupName)
                    grup = x;
            });

            if (grup == null)
            {
                grup = new SearchedTreeGrup(grupName);
                m_SearchedTreeGrups.Add(grup);
                RegisterGrup(grup);
            }

            StreamReader streamReader = new StreamReader(filePath);
            string file = streamReader.ReadToEnd();

            object[] trees = ENIXDeserializer.Deserialize(file);

            foreach (object tree in trees)
            {
                SearchedTree searchedTree = (SearchedTree)tree;

                if (searchedTree.GetParent() == null)
                    grup.Add(searchedTree);

                RegisterTree(searchedTree);
            }

            streamReader.Close();
        }

        private void Save()
        {
            foreach(SearchedTreeGrup grup in m_SearchedTreeGrups)
            {
                string path = $"{EnigmaticData.GetFullPath(SearchedTreeData.resources)}/{grup.Name}";

                grup.ForEach((x) =>
                {
                    object[] objects = { x };
                    ENIXSerializer.Serialize(x.Value, objects, path);
                });
            }
        }

        private void OnAddedTree(SearchedTree parentTree)
        {
            SearchedTree newTree = new SearchedTree("New Tree");
            parentTree.AddChild(newTree);

            RegisterTree(newTree);
            Repaint();
        }

        private void AddRootTree()
        {
            if (m_SelectedGrup == null)
                return;

            SearchedTree newTree = new SearchedTree("New Tree");
            m_SelectedGrup.Add(newTree);

            RegisterTree(newTree);
            Repaint();
        }

        private void RemoveSelectedTree()
        {
            RemoveTree(m_SelectedSearchedTree);
        }

        private void RemoveTree(SearchedTree tree)
        {
            if (tree == null)
                return;

            m_DestroedSearchedTree.Enqueue(tree);

            tree.ForEach((x) =>
            {
                m_DestroedSearchedTree.Enqueue(x);
            });

            Repaint();
        }

        private void OnRemovedTree(SearchedTree tree)
        {
            m_DestroedSearchedTree.Enqueue(tree);

            SearchedTree[] children = tree.GetСhildren();

            foreach (SearchedTree child in children)
                OnRemovedTree(child);
        }

        private void RemoveTrees()
        {
            while (m_DestroedSearchedTree.Count > 0)
            {
                SearchedTree tree = m_DestroedSearchedTree.Dequeue();
                SearchedTree parent = tree.GetParent();

                if (parent != null)
                    parent.RemoveChild(tree);

                m_Trees.Remove(tree);
                m_SelectedGrup.TryRemove(tree);
            }
        }

        private void RegisterTree(SearchedTree tree) => m_Trees.Add(tree, false);

        private void RegisterGrup(SearchedTreeGrup grup)
        {
            foreach (SearchedTree tree in grup.SearchedTrees)
            {
                foreach (SearchedTree childTree in tree.GetAllTree())
                    RegisterTree(childTree);
            }
        }

        private void GenerateTree(PathTypeTree pathType, string branchName, string[] childs)
        {
            SearchedTree tree = new SearchedTree(branchName);
            RegisterTree(tree);

            foreach(string child in childs)
            {
                SearchedTree childTree = new SearchedTree(child);
                tree.AddChild(childTree);
                RegisterTree(childTree);
            }

            if (pathType == PathTypeTree.SelectionGrup)
                m_SelectedGrup.Add(tree);
            else
                m_SelectedSearchedTree.AddChild(tree);

            SearchedTree[] trees = tree.GetAllTree();
            
            foreach(SearchedTree t in trees)
                t.UpdateLevel();
        }
    }
}

#endif