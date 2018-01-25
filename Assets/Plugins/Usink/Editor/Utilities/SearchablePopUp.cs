using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Usink
{
    public class SearchablePopup : EditorWindow
    {
        const int kMaxLists = 20;

        static SearchablePopup _singleton;

        static public void Show(Vector2 pos, string header, string[] items, Action<int> onAccepted)
        {
            if (!_singleton)
                _singleton = Resources.FindObjectsOfTypeAll<SearchablePopup>().FirstOrDefault() ?? CreateInstance<SearchablePopup>();

            _singleton.query = null;
            _singleton.header = header;
            _singleton.items = items;
            _singleton.itemGUIs = Array.ConvertAll(items, x => new GUIContent(x));
            _singleton.OnAccepted = onAccepted;
            _singleton.HandleQuery();
            _singleton.isFirstGUI = true;
            var rect = GUIUtility.GUIToScreenPoint(pos).ToRect();
            _singleton.ShowAsDropDown(rect,
                new Vector2(200, Mathf.Clamp(items.Length * 20 + 60, 50, 330)));
        }

        string query;
        string header;
        GUIContent[] itemGUIs;
        string[] items;
        List<int> queriedItems = new List<int>(kMaxLists);
        Action<int> OnAccepted;
        bool isFirstGUI = false;
        Vector2 scroll;
        int focusedIdx = 0;

        void OnGUI()
        {
            EditorGUILayout.LabelField(header, Styles.Header, GUILayout.Height(25));

            HandleKeyboard();
            HandleFieldBox();
            HandleQuery();
            HandleItems();
        }

        void HandleFieldBox ()
        {
            query = SearchField(query);
            if (isFirstGUI && Event.current.type == EventType.Layout)
            {
                EditorGUI.FocusTextInControl("CommandSearch");
                isFirstGUI = false;
                scroll = Vector2.zero;
                Focus();
            }
        }

        void HandleQuery()
        {

            var ev = Event.current;
            if (ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Return)
                return;
            queriedItems.Clear();
            bool isQueryEmpty = string.IsNullOrEmpty(query);

            for (int i = 0; i < items.Length; i++)
            {
                if (isQueryEmpty || StringContains(items[i], query))
                {
                    queriedItems.Add(i);
                    if (queriedItems.Count >= kMaxLists)
                        break;
                }
            }

            focusedIdx = Utility.Repeat(focusedIdx, queriedItems.Count);

        }
        void HandleItems()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));

            for (int i = 0; i < queriedItems.Count; i++)
            {
                Utility.GUITint(Color.white, focusedIdx == i ? Color.cyan : Color.white, () =>
                {
                    if (GUILayout.Button(itemGUIs[queriedItems[i]], Styles.ListButton))
                        Apply(i);
                });
            }

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
                case KeyCode.Return: Apply(focusedIdx); break;
                case KeyCode.Alpha0: Apply(0); break;
                case KeyCode.Alpha1: Apply(1); break;
                case KeyCode.Alpha2: Apply(2); break;
                case KeyCode.Alpha3: Apply(3); break;
                case KeyCode.Alpha4: Apply(4); break;
                case KeyCode.Alpha5: Apply(5); break;
                case KeyCode.Alpha6: Apply(6); break;
                case KeyCode.Alpha7: Apply(7); break;
                case KeyCode.Alpha8: Apply(8); break;
                case KeyCode.Alpha9: Apply(9); break;
                default: return;
            }

            ev.Use();
        }

        void Apply(int queryIndex)
        {
            if (queryIndex >= queriedItems.Count)
                return;
            Close();
            OnAccepted(queriedItems[queryIndex]);
        }

        static public string SearchField(string filterText)
        {
            GUI.SetNextControlName("CommandSearch");
            GUILayout.Space(5);
            Rect position = GUILayoutUtility.GetRect(10f, 20f);
            position.x += 8f;
            position.width -= 31f;

            filterText = EditorGUI.TextField(position, filterText, Styles.SearchBox);

            position.x += position.width;
            position.width = 15f;
            if (GUI.Button(position, GUIContent.none, filterText == null ? Styles.SearchCancelEmp : Styles.SearchCancelBut))
            {
                filterText = null;
            } 

            EditorGUILayout.Separator();

            return string.IsNullOrEmpty(filterText) ? null : filterText;
        }

        public static bool StringContains(string a, string b)
        {
            return a.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static class Styles
        {
            public static GUIStyle ListButton = new GUIStyle(EditorStyles.toolbarButton);
            public static GUIStyle Header = new GUIStyle("In BigTitle");
            public static GUIStyle SearchBox = new GUIStyle("SearchTextField");
            public static GUIStyle SearchCancelBut = new GUIStyle("SearchCancelButton");
            public static GUIStyle SearchCancelEmp = new GUIStyle("SearchCancelButtonEmpty");

            static Styles()
            {
                ListButton.alignment = TextAnchor.MiddleLeft;
                ListButton.richText = true;
                ListButton.fontSize = 11;
                ListButton.fixedHeight = 20;
                Header.font = EditorStyles.boldLabel.font;
            }
        }
    }
}
