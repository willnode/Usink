using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

namespace Usink
{
    public class AssetSearcherPopup : EditorWindow
    {
        const int kMaxLists = 20;

        static AssetSearcherPopup _singleton;


        static public void Show(Vector2 pos)
        {
            if (!_singleton)
            {
                _singleton = Resources.FindObjectsOfTypeAll<AssetSearcherPopup>().FirstOrNull() ?? CreateInstance<AssetSearcherPopup>();
            }

            _singleton.isFirstGUI = true;
            _singleton.queryType = KeyTypes.SelectGameObject;
            _singleton.query = "";
            _singleton.ReloadResults("");
            _singleton.ShowAsDropDown(GUIUtility.GUIToScreenPoint(pos).ToRect(),
                new Vector2(300, 400));
        }

        string query = "";
        int focusedIdx = 0;
        bool isFirstGUI;
        KeyTypes queryType = KeyTypes.SelectGameObject;
        List<string> res_str = new List<string>(32);
        List<GUIContent> res_gui = new List<GUIContent>(32);
        List<object> res_obj = new List<object>(32);

        void OnGUI()
        {
            EditorGUILayout.LabelField(titles[(int)queryType + 1], Styles.Header, GUILayout.Height(25));
            HandleKeyboard();
            HandleQuery();
            if (isFirstGUI && Event.current.type == EventType.Layout)
            {
                EditorGUI.FocusTextInControl("CommandSearch");
                isFirstGUI = false;
                _singleton.Focus();
            }
            if (res_obj.Count > 0)
                HandleResults();
            else
                EditorGUILayout.HelpBox(Styles.KeysHint.text, MessageType.None);
        }
        void HandleQuery()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
            EditorGUILayout.LabelField(keyGUIs[(int)queryType + 1], GUILayout.Width(20));
            var newQ = SearchField(query, GUILayout.ExpandWidth(true));
            if (newQ != query)
            {
                query = newQ;
                queryType = (KeyTypes)keys.IndexOf(query.Length > 0 ? query[0] : ' ');
                ReloadResults(queryType == KeyTypes.SelectGameObject ? query : query.Substring(1));
            }
            EditorGUILayout.EndHorizontal();
        }

        void ReloadResults(string query)
        {
            res_str.Clear();
            res_gui.Clear();
            res_obj.Clear();
            if (string.IsNullOrEmpty(query))
                return;
            switch (queryType)
            {
                case KeyTypes.SelectGameObject:
                case KeyTypes.SelectAdditive:
                case KeyTypes.SelectSubstractive:
                    DoObjectSearch(query, HierarchyType.GameObjects);
                    break;
                case KeyTypes.SelectInChildren:
                    DoObjectSearchInChild(Selection.transforms, query);
                    break;
                case KeyTypes.SelectAsset:
                case KeyTypes.OpenAsset:
                    DoObjectSearch(query, HierarchyType.Assets);
                    break;
                case KeyTypes.ExecuteWindow:
                    DoWindowSearch(query);
                    break;
            }
        }

        void DoObjectSearch(string filter, HierarchyType type)
        {
            var props = new HierarchyProperty(type);
            props.SetSearchFilter(filter, 0);
            props.Reset();
            while (props.Next(null))
            {
                if (res_str.Count > kMaxLists)
                    return;
                res_str.Add(type == HierarchyType.GameObjects ? props.name : AssetDatabase.GUIDToAssetPath(props.guid));
                res_gui.Add(new GUIContent(props.name, AssetPreview.GetMiniThumbnail(props.pptrValue)));
                res_obj.Add(props.instanceID);
            }
        }


        void DoObjectSearchInChild(Transform[] objects, string filter)
        {
            var props = new HierarchyProperty(HierarchyType.GameObjects);
            props.SetSearchFilter(filter, 0);
            props.Reset();
            while (props.Next(null))
            {
                if (res_str.Count > kMaxLists)
                    return;
                var obj = ((GameObject)EditorUtility.InstanceIDToObject(props.instanceID));
                if (!obj || (objects.Length > 0 && !objects.Any(x => x && obj.transform.IsChildOf(x))))
                    continue;
                res_str.Add(props.name);
                res_gui.Add(new GUIContent(props.name, AssetPreview.GetMiniThumbnail(props.pptrValue)));
                res_obj.Add(props.instanceID);
            }
        }

        void DoWindowSearch(string filter)
        {
            InitWindows();
            for (int i = 0; i < editors.Count; i++)
            {
                if (res_str.Count > kMaxLists)
                    return;
                if (editors[i].Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                res_str.Add(editors[i].Name);
                res_gui.Add(new GUIContent(editors[i].Name));
                res_obj.Add(editors[i]);
            }
        }

        static public List<Type> editors;
        static bool hasInitEditors = false;

        public static void InitWindows()
        {
            if (hasInitEditors)
                return;
            var typ = typeof(EditorWindow);
            editors = typeof(EditorWindowWorker).Assembly.GetTypes().Where(t => typ.IsAssignableFrom(t)).ToList();
            editors.AddRange(typeof(Editor).Assembly.GetTypes().Where(t => typ.IsAssignableFrom(t)));
            hasInitEditors = true;
        }


        void HandleResults()
        {
            var count = Mathf.Min(kMaxLists, res_obj.Count);
            for (int i = 0; i < count; i++)
            {
                if (focusedIdx == i)
                    GUI.backgroundColor = Color.cyan;
                EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
                //GUI.DrawTexture(EditorGUILayout.GetControlRect(GUILayout.Width(20)), res_gui[i].image);
                var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
                if (GUI.Button(rect, res_gui[i], Styles.ListButton))
                {
                    Apply(i);
                }
                if (focusedIdx == i)
                    GUI.backgroundColor = Color.white;
            }
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
            if (queryIndex >= res_gui.Count)
                return;
            Close();
            UnityEngine.Object[] o = null;
            if (queryType <= KeyTypes.OpenAsset)
            {
                o = queryIndex < 0 ? GetObjects() : new UnityEngine.Object[] { EditorUtility.InstanceIDToObject((int)res_obj[queryIndex]) };
            }
            switch (queryType)
            {
                case KeyTypes.SelectGameObject:
                case KeyTypes.SelectInChildren:
                case KeyTypes.SelectAsset:
                    if (queryIndex >= 0 && queryType >= KeyTypes.SelectAsset)
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
                    // This time would be a type instead of instance ID
                    var y = (Type)res_obj[Mathf.Max(0, queryIndex)];
                    ((EditorWindow)EditorWindow.CreateInstance(y)).Show();
                    break;
            }
        }

        UnityEngine.Object[] GetObjects()
        {
            var obj = new UnityEngine.Object[res_obj.Count];
            for (int i = 0; i < res_obj.Count; i++)
            {
                obj[i] = EditorUtility.InstanceIDToObject((int)res_obj[i]);
            }
            return obj;
        }

        /*
        string query;
        string header;
        GUIContent[] itemGUIs;
        string[] items;
        List<int> queriedItems = new List<int>(kMaxLists);
        Action<int> OnAccepted;
        bool isFirstGUI = false;
        Vector2 scroll;
        int focusedIdx = 0;

        void OnGUI ()
        {
            EditorGUILayout.LabelField(header, Styles.Header, GUILayout.Height(25));
             HandleKeyboard();
           query = SearchField(query);
            if (isFirstGUI && Event.current.type == EventType.Layout) {
                EditorGUI.FocusTextInControl("CommandSearch");
                isFirstGUI = false;
                scroll = Vector2.zero;
                _singleton.Focus();
            }
            HandleQuery();
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            HandleItems();
            EditorGUILayout.EndScrollView();
        }

        void HandleQuery () {
            var ev = Event.current;
            if (ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Return)
                return;
            queriedItems.Clear();
            bool isQueryEmpty = string.IsNullOrEmpty(query);
            for (int i = 0; i < items.Length; i++)
            {
                if (isQueryEmpty || StringContains(items[i], query)) {
                    queriedItems.Add(i);
                    if (queriedItems.Count >= kMaxLists)
                        break;
                }
            }
            focusedIdx = focusedIdx < 0 ? queriedItems.Count - 1 : 
                (focusedIdx >= queriedItems.Count ? 0 : focusedIdx);
        }
        void HandleItems () {
            var backColor = GUI.backgroundColor;
            for (int i = 0; i < queriedItems.Count; i++)
            {
                if (focusedIdx == i)
                    GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button(itemGUIs[queriedItems[i]], Styles.ListButton))
                {
                    Apply(i);
                }
                 if (focusedIdx == i)
                    GUI.backgroundColor = backColor;           
            }
        }



        void Apply (int queryIndex) {
            if (queryIndex >= queriedItems.Count)
                return;
            Close();
            OnAccepted(queriedItems[queryIndex]);
        }*/

        static public string SearchField(string filterText, params GUILayoutOption[] options)
        {
            GUI.SetNextControlName("CommandSearch");
            GUILayout.Space(5);
            Rect position = EditorGUILayout.GetControlRect(options);//GUILayoutUtility.GetRect(10f, 20f);
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
            public static GUIContent KeysHint = new GUIContent(
                "\nType what you want to do\n" + "\n...\tSelect GameObjects" +
                "\n+\tSelect Additively" + "\n-\tSubtact Selection" + "\n/\tSelect in Selected Children" +
                "\n#\tOpen Asset" + "\n>\tSelect Asset" + "\n!\tOpen Editor Window"
                + "\n\nShift+Enter to select all in the list");
            static Styles()
            {
                ListButton.alignment = TextAnchor.MiddleLeft;
                ListButton.richText = true;
                ListButton.fontSize = 11;
                ListButton.fixedHeight = 20;
                Header.font = EditorStyles.boldLabel.font;
            }
        }

        const string keys = "+-/#>!?";

        public enum KeyTypes
        {
            SelectGameObject = -1,
            SelectAdditive = 0,
            SelectSubstractive = 1,
            SelectInChildren = 2,
            OpenAsset = 3,
            SelectAsset = 4,
            ExecuteWindow = 5,
            ExecuteHelp = 6
        }

        public static readonly GUIContent[] titles = {
        new GUIContent("Select Game Object"),
        new GUIContent("Select Additively"),
        new GUIContent("Select Subtactively"),
        new GUIContent("Select in Children"),
        new GUIContent("Open Asset"),
        new GUIContent("Select Asset"),
        new GUIContent("Lauch Window"),
        new GUIContent("Open Help Doc"),
   };

        public static readonly GUIContent[] keyGUIs = {
        new GUIContent("..."),
        new GUIContent("+"),
        new GUIContent("-"),
        new GUIContent("/"),
        new GUIContent("#"),
        new GUIContent(">"),
        new GUIContent("!"),
        new GUIContent("?"),
   };
    }

    public class EditorWindowWorker
    {



    }
}
