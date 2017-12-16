using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

namespace Usink
{
    public class AssetSearcherPopup : EditorWindow
    {
        static int kMaxLists = 16;

        static AssetSearcherPopup _singleton;

        static public void Show(Vector2 pos)
        {
            if (!_singleton)
            {
                _singleton = Resources.FindObjectsOfTypeAll<AssetSearcherPopup>().FirstOrDefault() ?? CreateInstance<AssetSearcherPopup>();
            }

            _singleton.isFirstGUI = true;
            _singleton.queryType = KeyTypes.SelectGameObject;
            _singleton.query = "";
            _singleton.ReloadResults("");
            _singleton.ShowAsDropDown(GUIUtility.GUIToScreenPoint(pos).ToRect(), new Vector2(300, 400));
        }

        string query = "";
        int focusedIdx = 0;
        bool isFirstGUI;

        KeyTypes queryType = KeyTypes.SelectGameObject;

        List<ID> res = new List<ID>(32);

        public struct ID
        {
            public string name;
            public GUIContent gui;
            // could be instanceID or type
            public object obj;
        }

        public struct TypeName
        {
            public Type type;
            public string name;

            public TypeName(Type type, string name) : this()
            {
                this.type = type;
                this.name = name;
            }
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField(Styles.titles[(int)queryType], Styles.Header, GUILayout.Height(25));
            HandleKeyboard();
            HandleQuery();
            if (isFirstGUI && Event.current.type == EventType.Layout)
            {
                EditorGUI.FocusTextInControl("CommandSearch");
                isFirstGUI = false;
                _singleton.Focus();
            }
            if (res.Count > 0)
                HandleResults();
            else
                EditorGUILayout.HelpBox(Styles.KeysHint, MessageType.None);
        }

        void HandleQuery()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
            EditorGUILayout.LabelField(Styles.keyGUIs[(int)queryType], GUILayout.Width(20));
            var newQ = SearchField(query, GUILayout.ExpandWidth(true));
            if (newQ != query)
            {
                query = newQ;
                queryType = (KeyTypes)Math.Max(0, Styles.keys.IndexOf(query.Length > 0 ? query[0] : ' '));
                ReloadResults(queryType == KeyTypes.SelectGameObject ? query : query.Substring(1));
            }
            EditorGUILayout.EndHorizontal();
        }

        void ReloadResults(string query)
        {
            res.Clear();
            if (string.IsNullOrEmpty(query))
                return;
            var q2 = query.Replace(">", "t:");
            switch (queryType)
            {
                case KeyTypes.SelectGameObject: DoObjectSearch(q2); break;
                case KeyTypes.SelectAdditive: DoObjectAdditiveSearch(q2); break;
                case KeyTypes.SelectSubstractive: DoObjectSubstractiveSearch(q2); break;
                case KeyTypes.SelectHasTypes: DoComponentSearch(q2); break;
                case KeyTypes.SelectInChildren: DoObjectSearchInChild(Selection.GetTransforms(SelectionMode.TopLevel), q2); break;
                case KeyTypes.SelectAsset: DoAssetSearch(q2); break;
                case KeyTypes.ExecuteWindow: DoWindowSearch(query); break;
            }
        }

        void DoObjectSearch(string filter)
        {
            var props = new HierarchyProperty(HierarchyType.GameObjects);
            props.SetSearchFilter(filter, 0);
            props.Reset();
            while (props.Next(null))
            {
                if (res.Count >= kMaxLists) return;
                res.Add(new ID()
                {
                    name = props.name,
                    gui = new GUIContent(props.name, AssetPreview.GetMiniThumbnail(props.pptrValue)),
                    obj = props.instanceID
                });
            }
        }

        void DoObjectAdditiveSearch(string filter)
        {
            var props = new HierarchyProperty(HierarchyType.GameObjects);
            props.SetSearchFilter(filter, 0);
            props.Reset();
            var sel = Selection.instanceIDs;
            while (props.Next(null))
            {
                if (res.Count >= kMaxLists) return;
                if (!sel.Contains(props.instanceID))
                    res.Add(new ID()
                    {
                        name = props.name,
                        gui = new GUIContent(props.name, AssetPreview.GetMiniThumbnail(props.pptrValue)),
                        obj = props.instanceID
                    });
            }
        }


        void DoObjectSubstractiveSearch(string filter)
        {
            var sel = Selection.instanceIDs;
            if (sel.Length == 0) return;
            var props = new HierarchyProperty(HierarchyType.GameObjects);
            props.SetSearchFilter(filter, 0);
            props.Reset();
            while (props.Next(null))
            {
                if (res.Count >= kMaxLists) return;

                if (sel.Contains(props.instanceID))
                    res.Add(new ID()
                    {
                        name = props.name,
                        gui = new GUIContent(props.name, AssetPreview.GetMiniThumbnail(props.pptrValue)),
                        obj = props.instanceID
                    });
            }
        }

        void DoComponentSearch(string filter)
        {
            InitTypeIndexing();

            var types = components.Where((x) => StringContains(x.name, filter));

            var props = new HierarchyProperty(HierarchyType.GameObjects);

            foreach (var type in types)
            {
                props.SetSearchFilter("t:" + type.name, 0);
                props.Reset();
                while (props.Next(null))
                {
                    if (res.Count >= kMaxLists) return;

                    if (!res.Any((x) => props.instanceID == (int)x.obj))
                        res.Add(new ID()
                        {
                            name = props.name,
                            gui = new GUIContent(props.name, AssetPreview.GetMiniThumbnail(
                                (props.pptrValue as GameObject).GetComponent(type.type))),
                            obj = props.instanceID
                        });
                }
            }
        }

        void DoObjectSearchInChild(Transform[] objects, string filter)
        {
            var props = new HierarchyProperty(HierarchyType.GameObjects);
            props.SetSearchFilter(filter, 0);
            props.Reset();
            while (props.Next(null))
            {
                if (res.Count >= kMaxLists)
                    return;
                var obj = ((GameObject)EditorUtility.InstanceIDToObject(props.instanceID));
                if (!obj || (objects.Length > 0 && !objects.Any(x => x && obj.transform.IsChildOf(x))))
                    continue;
                res.Add(new ID()
                {
                    name = props.name,
                    gui = new GUIContent(props.name, AssetPreview.GetMiniThumbnail(props.pptrValue)),
                    obj = props.instanceID
                });
            }
        }

        void DoAssetSearch(string filter)
        {
            var props = new HierarchyProperty(HierarchyType.Assets);
            props.SetSearchFilter(filter, 0);
            props.Reset();
            while (props.Next(null))
            {
                if (res.Count >= kMaxLists)
                    return;
                res.Add(new ID()
                {
                    name = AssetDatabase.GUIDToAssetPath(props.guid),
                    gui = new GUIContent(props.name, AssetPreview.GetMiniThumbnail(props.pptrValue)),
                    obj = props.instanceID
                });
            }
        }

        void DoWindowSearch(string filter)
        {
            InitTypeIndexing();
            for (int i = 0; i < editorWindows.Count; i++)
            {
                if (res.Count >= kMaxLists)
                    return;

                if (editorWindows[i].name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                res.Add(new ID()
                {
                    name = editorWindows[i].name,
                    gui = new GUIContent(editorWindows[i].name),
                    obj = editorWindows[i].type
                });
            }
        }

        static public List<TypeName> editorWindows = new List<TypeName>();
        static public List<TypeName> components = new List<TypeName>();
        static bool hasInitTypes = false;

        public static void InitTypeIndexing()
        {
            if (hasInitTypes) return;

            var editorwindow = typeof(EditorWindow);
            var component = typeof(Component);
            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = ass.GetTypes();
                editorWindows.AddRange(types.Where(t => editorwindow.IsAssignableFrom(t)).Select(x => new TypeName(x, x.Name)));
                components.AddRange(types.Where(t => component.IsAssignableFrom(t)).Select(x => new TypeName(x, x.Name)));
            }
            hasInitTypes = true;
        }

        Vector2 scroll;

        void HandleResults()
        {
            var count = Mathf.Min(kMaxLists, res.Count);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int i = 0; i < count; i++)
            {
                if (focusedIdx == i)
                    GUI.backgroundColor = Color.cyan;

                var rect = EditorGUILayout.GetControlRect(GUILayout.Height(20), GUILayout.ExpandWidth(true));

                if (GUI.Button(rect, res[i].gui, Styles.ListButton))
                {
                    Apply(i);
                }
                if (focusedIdx == i)
                    GUI.backgroundColor = Color.white;
            }
            if (res.Count == kMaxLists)
                GUILayout.Label("There may more results available", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndScrollView();
        }

        void HandleKeyboard()
        {
            var ev = Event.current;
            if (ev.type != EventType.KeyDown)
                return;
            switch (ev.keyCode)
            {
                case KeyCode.Escape: Close(); break;
                case KeyCode.DownArrow: focusedIdx++; break;
                case KeyCode.UpArrow: focusedIdx--; break;
                case KeyCode.Backspace: if (string.IsNullOrEmpty(query)) { queryType = KeyTypes.SelectGameObject; break; } else return;
                case KeyCode.Return: Apply(ev.shift ? -1 : focusedIdx); break;
                default: return;
            }
            ev.Use();
        }

        void Apply(int queryIndex)
        {
            if (queryIndex >= res.Count)
                return;
            Close();
            UnityEngine.Object[] o = null;
            if (queryType != KeyTypes.ExecuteWindow)
            {
                // load objects
                if (queryIndex < 0)
                {
                    if (query.Length == kMaxLists)
                    {
                        var oldk = kMaxLists;
                        kMaxLists = int.MaxValue;
                        ReloadResults(query); //full requery
                        kMaxLists = oldk;
                    }
                    o = GetObjects();
                }
                else
                    o = new UnityEngine.Object[] { EditorUtility.InstanceIDToObject((int)res[queryIndex].obj) };
            }
            switch (queryType)
            {
                case KeyTypes.SelectGameObject:
                case KeyTypes.SelectInChildren:
                case KeyTypes.SelectAsset:
                case KeyTypes.SelectHasTypes:
                    if (queryIndex >= 0 && queryType == KeyTypes.SelectAsset)
                        EditorGUIUtility.PingObject(o[0]);
                    Selection.objects = o;
                    break;
                case KeyTypes.SelectAdditive:
                    ArrayUtility.AddRange(ref o, Selection.objects);
                    Selection.objects = o;
                    break;
                case KeyTypes.SelectSubstractive:
                    var x = new HashSet<UnityEngine.Object>(Selection.objects);
                    x.ExceptWith(o);
                    Selection.objects = x.ToArray();
                    break;
                case KeyTypes.ExecuteWindow:
                    // This time obj would be a type instead of instance ID
                    // index -1 (launch all) is not acceptable here
                    var y = (Type)res[Mathf.Max(0, queryIndex)].obj;
                    ((EditorWindow)CreateInstance(y)).Show();
                    break;
            }
        }

        UnityEngine.Object[] GetObjects()
        {
            var obj = new UnityEngine.Object[res.Count];
            for (int i = 0; i < res.Count; i++)
            {
                obj[i] = EditorUtility.InstanceIDToObject((int)res[i].obj);
            }
            return obj;
        }

        static public string SearchField(string filterText, params GUILayoutOption[] options)
        {
            GUI.SetNextControlName("CommandSearch");
            GUILayout.Space(5);
            Rect position = EditorGUILayout.GetControlRect(options);
            position.x += 8f;
            position.width -= 31f;

            filterText = EditorGUI.TextField(position, filterText, Styles.SearchBox);

            position.x += position.width;
            position.width = 15f;
            if (GUI.Button(position, GUIContent.none, (filterText == string.Empty) ? Styles.SearchCancelEmp : Styles.SearchCancelBut))
            {
                filterText = string.Empty;
            }
            EditorGUILayout.Separator();
            return filterText;
        }

        public static bool StringContains(string a, string b)
        {
            return a.IndexOf(b, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static class Styles
        {
            public static GUIStyle ListButton = new GUIStyle(EditorStyles.toolbarButton);
            public static GUIStyle Header = new GUIStyle("In BigTitle");
            public static GUIStyle SearchBox = new GUIStyle("SearchTextField");
            public static GUIStyle SearchCancelBut = new GUIStyle("SearchCancelButton");
            public static GUIStyle SearchCancelEmp = new GUIStyle("SearchCancelButtonEmpty");


            public static readonly GUIContent[] keyGUIs = {
                new GUIContent(" ", "Select Game Objects"),
                new GUIContent("+", "Select Additively"),
                new GUIContent("-", "Select Subtractively"),
                new GUIContent("/", "Select in Childrens"),
                new GUIContent(">", "Select Which Has Type"),
                new GUIContent("#", "Select Assets"),
                new GUIContent("!", "Launch Window"),
            };

            public static readonly GUIContent[] titles = Array.ConvertAll(keyGUIs, (x) => new GUIContent(x.tooltip));

            public static readonly string keys = string.Join("", Array.ConvertAll(keyGUIs, (x) => x.text));

            public static string KeysHintInfo = "\n\nWhen results available:\nEnter to select the first item\nShift+Enter to select all item\n";
            public static string KeysHint = string.Join("\n", Array.ConvertAll(keyGUIs, (x) => x.text + "\t" + x.tooltip)) + KeysHintInfo;

            static Styles()
            {
                ListButton.alignment = TextAnchor.MiddleLeft;
                ListButton.richText = true;
                ListButton.fontSize = 11;
                ListButton.fixedHeight = 20;
                Header.font = EditorStyles.boldLabel.font;
            }
        }

        public enum KeyTypes
        {
            SelectGameObject,
            SelectAdditive,
            SelectSubstractive,
            SelectInChildren,
            SelectHasTypes,
            SelectAsset,
            ExecuteWindow,
        }
    }

}
