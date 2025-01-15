#if UNITY_EDITOR

using System.IO;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Enigmatic.Core;
using Enigmatic.Core.Editor;

using ENIX;

namespace Enigmatic.SearchedTrees
{
    public class SearchedTreeEditorWindow : EnigmaticWindow
    {
        private List<SearchedTreeGroup> m_Groups = new List<SearchedTreeGroup>();
        private Dictionary<SearchedTree, bool> m_Expanded = new Dictionary<SearchedTree, bool>();

        private SearchedTreeGroup m_SelectedGroup;
        private SearchedTree m_SelectedSearchedTree;
        private SearchedTree m_LocalViewSearchedTree;

        private Vector2 m_GroupScrollPosition;
        private Vector2 m_SearchTreeScrollPosition;

        private ReorderableList<SearchedTreeGroup> m_ReorderableGroups = new ReorderableList<SearchedTreeGroup>();
        private HierarchicalReorderableList<SearchedTree> m_HierarchicalReorderableTrees = new HierarchicalReorderableList<SearchedTree>();

        private Queue<SearchedTree> m_DestroyedSearchedTree = new Queue<SearchedTree>();

        private float m_GroupColumnWidth => position.width * 0.22f;
        private float m_TreeViewColumnWidth => position.width * 0.515f;
        private float m_SettingsColumnWidth => position.width * 0.25f;

        private float m_ColumnHeight => position.height - 27;

        [MenuItem("Tools/Enigmatic/Searched Tree Editor")]
        public static void Open()
        {
            SearchedTreeEditorWindow searchableEditorWindow = GetWindow<SearchedTreeEditorWindow>();
            searchableEditorWindow.titleContent = new GUIContent("Searched Tree Editor");
            searchableEditorWindow.minSize = new Vector2(800, 600);
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            m_ReorderableGroups.AttachList(m_Groups);
            m_ReorderableGroups.OnDrawElementByIndex += DrawGroup;

            m_ReorderableGroups.OnDrawBackground += DrawTreeGroupBackground;
            m_ReorderableGroups.OnDrawHoverBackground += DrawTreeGroupHoverBackground;
            m_ReorderableGroups.OnDrawSelectedBackground += DrawGroupSelectedBackground;

            m_ReorderableGroups.OnSelectElementT += SelectGroup;

            m_HierarchicalReorderableTrees.OnDrawChild += CheckExpand;
            m_HierarchicalReorderableTrees.OnDrawNode += DrawTree;

            m_HierarchicalReorderableTrees.OnSelectElement += SelectTree;

            m_HierarchicalReorderableTrees.OnDrawBackground += DrawTreeBackGround;
            m_HierarchicalReorderableTrees.OnDrawHoverNodeBackground += DrawTreeHoverBackground;
            m_HierarchicalReorderableTrees.OnDrawSelectedNodeBackground += DrawTreeSelectedBackground;
        }

        protected override void OnDraw()
        {
            base.OnDraw();

            if (DragAndDropHandler.Handel(Rect, out string[] paths))
                Load(paths);

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
                DrawingUtilities.BeginColum(m_GroupColumnWidth, m_ColumnHeight, -1);
                {
                    DrawingUtilities.ColumnTitle(EnigmaticStyles.columnBackground, "Group",
                        m_GroupColumnWidth, true, m_GroupColumnWidth - 98, AddGroup, RemoveGroup);

                    DrawingUtilities.BeginVerticalScrollViewByGroup(m_GroupScrollPosition, new Vector2(0, 21), -2);
                    {
                        DrawGroups();
                    }
                    m_GroupScrollPosition = DrawingUtilities.EndVerticalScrollViewByGroup();
                }
                DrawingUtilities.EndColum();

                DrawingUtilities.BeginColum(m_TreeViewColumnWidth, m_ColumnHeight);
                {
                    DrawingUtilities.ColumnTitle(EnigmaticStyles.columnBackground, "Tree View", m_TreeViewColumnWidth,
                        true, m_TreeViewColumnWidth - 122, AddRootTree, RemoveSelectedTree);

                    DrawingUtilities.BeginVerticalScrollViewByGroup(m_SearchTreeScrollPosition, new Vector2(1, 22), 0,
                        new RectOffset(0, 2, 0, 0));
                    {
                        DrawTrees();
                    }
                    m_SearchTreeScrollPosition = DrawingUtilities.EndVerticalScrollViewByGroup();
                }
                DrawingUtilities.EndColum();

                DrawingUtilities.BeginColum(m_SettingsColumnWidth, m_ColumnHeight);
                {
                    DrawingUtilities.ColumnTitle(EnigmaticStyles.columnBackground, "Settings", m_SettingsColumnWidth);

                    DrawSettings();
                }
                DrawingUtilities.EndColum();
            }
            EnigmaticGUILayout.EndHorizontal();

            RemoveTrees();
        }

        private void DrawGroups()
        {
            float width = EnigmaticGUILayout.GetActiveGroup().Rect.width;
            m_ReorderableGroups.Draw(width, EnigmaticGUILayout.Padding(1), EnigmaticGUILayout.ElementSpacing(-1));
        }

        private void DrawGroup(int index)
        {
            SearchedTree group = m_Groups[index];

            EnigmaticGUILayout.BeginHorizontal(EnigmaticGUILayout.Width(m_GroupColumnWidth - 16), EnigmaticGUILayout.Height(22));
            {
                EnigmaticGUILayout.Label(group.Value);
            }
            EnigmaticGUILayout.EndHorizontal();
        }

        private void DrawTrees()
        {
            if (m_SelectedGroup == null)
                return;

            m_HierarchicalReorderableTrees.Draw(m_TreeViewColumnWidth, -1);
        }

        private void DrawTree(SearchedTree tree)
        {
            bool expanded = m_Expanded[tree];

            EnigmaticGUILayout.BeginHorizontal(EnigmaticGUILayout.Width(m_TreeViewColumnWidth),
                    EnigmaticGUILayout.Height(18), EnigmaticGUILayout.Padding(0));
            {
                EnigmaticGUILayout.Space((tree.DepthLevel - 1) * 16);

                if (tree.ChildCount > 0)
                {
                    EnigmaticGUILayout.BeginFoldout(ref expanded, "", Repaint, null, 16);
                    EnigmaticGUILayout.EndFoldout(expanded);
                }
                else
                {
                    EnigmaticGUILayout.Space(16);
                }

                EnigmaticGUILayout.Label(tree.Value);
            }
            EnigmaticGUILayout.EndHorizontal();

            if (m_Expanded[tree] != expanded)
                m_Expanded[tree] = expanded;
        }

        private void DrawSettings()
        {
            if (m_SelectedGroup == null)
                return;

            EnigmaticGUILayout.BeginVertical(EnigmaticGUILayout.Width(m_SettingsColumnWidth),
                EnigmaticGUILayout.ExpandHeight(true), EnigmaticGUILayout.Padding(6));
            {
                m_SelectedGroup.Value = EnigmaticGUILayout.TextField("Group Name:", m_SelectedGroup.Value);

                EnigmaticGUILayout.Space(6);
                EnigmaticGUILayout.Image(new Vector2(m_SettingsColumnWidth - 12, -1), EnigmaticStyles.columnBackground);
                EnigmaticGUILayout.Space(6);

                if (m_SelectedSearchedTree != null)
                {
                    m_SelectedSearchedTree.Value = EnigmaticGUILayout.TextField("Tree Value:", m_SelectedSearchedTree.Value);

                    EnigmaticGUILayout.Space(3);

                    EnigmaticGUILayout.BeginHorizontal(EnigmaticGUILayout.Width(m_SettingsColumnWidth),
                        EnigmaticGUILayout.ExpandHeight(true), EnigmaticGUILayout.Padding(0));
                    {
                        float buttonWidth = (m_SettingsColumnWidth - 12) / 2;

                        if (EnigmaticGUILayout.Button("Add", new Vector2(buttonWidth, 18)))
                            OnAddedTree(m_SelectedSearchedTree);

                        EnigmaticGUILayout.Button("Remove", new Vector2(buttonWidth, 18));
                    }
                    EnigmaticGUILayout.EndHorizontal();
                }
            }
            EnigmaticGUILayout.EndVertical();
        }

        private void DrawTreeGroupBackground()
        {
            EnigmaticGUILayout.FillLastGroupImage(EnigmaticStyles.columnBackground);
        }

        private void DrawTreeGroupHoverBackground()
        {
            EnigmaticGUILayout.FillLastGroupImage(EnigmaticStyles.columnBackgroundHover);
        }

        private void DrawGroupSelectedBackground()
        {
            EnigmaticGUILayout.FillLastGroupImage(EnigmaticStyles.columnBackgroundSelected);
        }

        private void DrawTreeBackGround(SearchedTree tree)
        {
            EnigmaticGUILayout.FillLastGroupImage(EnigmaticStyles.columnBackground);
        }

        private void DrawTreeHoverBackground(SearchedTree tree)
        {
            EnigmaticGUILayout.FillLastGroupImage(EnigmaticStyles.columnBackgroundHover);
        }

        private void DrawTreeSelectedBackground(SearchedTree tree)
        {
            EnigmaticGUILayout.FillLastGroupImage(EnigmaticStyles.columnBackgroundSelected);
        }

        private void AddGroup()
        {
            SearchedTreeGroup group = new SearchedTreeGroup("New group");
            m_Groups.Add(group);

            Repaint();
        }

        private void RemoveGroup()
        {
            if (m_SelectedGroup == null)
                return;

            m_Groups.Remove(m_SelectedGroup);
            m_SelectedGroup = null;
            Repaint();
        }

        private void SelectGroup(SearchedTreeGroup group)
        {
            m_SelectedGroup = group;
            m_HierarchicalReorderableTrees.AttachList(m_SelectedGroup.Children);
        }

        private void SelectTree(SearchedTree tree)
        {
            m_SelectedSearchedTree = tree;
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
            if (m_SelectedGroup == null)
                return;

            SearchedTree newTree = new SearchedTree("New Tree");
            m_SelectedGroup.AddChild(newTree);
            
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

            m_DestroyedSearchedTree.Enqueue(tree);

            tree.ForEach((x) =>
            {
                m_DestroyedSearchedTree.Enqueue(x);
            });

            Repaint();
        }

        private void OnRemovedTree(SearchedTree tree)
        {
            m_DestroyedSearchedTree.Enqueue(tree);

            SearchedTree[] children = tree.GetChildren();

            foreach (SearchedTree child in children)
                OnRemovedTree(child);
        }

        private void RemoveTrees()
        {
            while (m_DestroyedSearchedTree.Count > 0)
            {
                SearchedTree tree = m_DestroyedSearchedTree.Dequeue();
                SearchedTree parent = tree.Parent;

                if (parent != null)
                    parent.RemoveChild(tree);

                m_Expanded.Remove(tree);
                m_SelectedGroup.RemoveChild(tree);
            }
        }

        private void RegisterTree(SearchedTree tree)
        {
            if (m_Expanded.ContainsKey(tree))
                return;

            m_Expanded.Add(tree, false);
        }

        private void RegisterGroup(SearchedTreeGroup group)
        {
            group.ForEach((x) =>
            {
                foreach (SearchedTree childTree in x.GetAllTree())
                    RegisterTree(childTree);
            });
        }

        private void GenerateTree(PathTypeTree pathType, string branchName, string[] childs)
        {
            SearchedTree tree = new SearchedTree(branchName);
            RegisterTree(tree);

            foreach (string child in childs)
            {
                SearchedTree childTree = new SearchedTree(child);
                tree.AddChild(childTree);
                RegisterTree(childTree);
            }

            if (pathType == PathTypeTree.SelectionGroup)
                m_SelectedGroup.AddChild(tree);
            else
                m_SelectedSearchedTree.AddChild(tree);

            SearchedTree[] trees = tree.GetAllTree();

            foreach (SearchedTree t in trees)
                t.UpdateLevel();
        }

        private void Load()
        {
            string path = EditorUtility.OpenFilePanel("Select a folder", Application.dataPath, "stp");

            if (string.IsNullOrEmpty(path) == false)
                Load(path);
        }

        private void Load(string[] paths)
        {
            foreach(string path in paths)
            {
                if (Path.GetExtension(path) != ".stp")
                    continue;

                string uniformPath = EnigmaticData.GetUniformPath(path);
                string fullPath  = EnigmaticData.GetFullPath(uniformPath);

                Load(fullPath);
            }
        }

        private void Load(string path)
        {
            string filePath = $"{path.Replace('\\', '/')}";
            string[] pathSpited = filePath.Split('/');
            string groupName = pathSpited[pathSpited.Length - 2];

            SearchedTreeGroup group = null;
            SearchedTreeGroup originGroup = STPFile.Load(filePath);

            m_Groups.ForEach((x) =>
            {
                if (x.Value == originGroup.Value)
                    group = x;
            });

            if (group == null)
            {
                group = new SearchedTreeGroup(groupName);
                m_Groups.Add(group);
            }

            while (originGroup.ChildCount > 0)
            {
                SearchedTree tree = originGroup.Children[0];
                originGroup.RemoveChild(tree);

                group.AddChild(tree);
            }

            RegisterGroup(group);
        }

        private void Save()
        {
            foreach (SearchedTreeGroup group in m_Groups)
                STPFile.Save(group);
        }

        private bool CheckExpand(SearchedTree tree)
        {
            if (m_Expanded.ContainsKey(tree) == false)
                return false;

            return m_Expanded[tree];
        }
    }
}

#endif