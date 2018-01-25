
using UnityEditor;
using System;
using UnityEngine;
using System.Linq;

namespace Usink
{
    class EnumerationPopup : EditorWindow
    {
        static class Styles
        {
            public static GUIStyle LeftButton = EditorStyles.miniButtonLeft;
            public static GUIStyle MainButton = EditorStyles.miniButtonRight;
            public static GUIStyle Header = EditorStyles.boldLabel;
        }

        static EnumerationPopup singleton;

        public static void ShowEnumeration<T>(Vector2 pos, string header, Action<int> action)
        {
            ShowEnumeration(pos, new Vector2(300, 400), 2, header, typeof(T), action);
        }
        public static void ShowEnumeration(Vector2 pos, string header, string[] enums, Action<int> action)
        {
            ShowEnumeration(pos, new Vector2(300, 400), 2, header, enums, action);
        }
        public static void ShowEnumeration(Vector2 pos, Vector2 size, int columnCount, string header, Type enumType, Action<int> action)
        {
            ShowEnumeration(pos, size, columnCount, header, Array.ConvertAll(Enum.GetNames(enumType), x => new GUIContent(ObjectNames.NicifyVariableName(x))), action);
        }

        public static void ShowEnumeration(Vector2 pos, Vector2 size, int columnCount, string header, string[] enums, Action<int> action)
        {
            ShowEnumeration(pos, size, columnCount, header, Array.ConvertAll(enums, x => new GUIContent(x)), action);
        }

        public static void ShowEnumeration(Vector2 pos, Vector2 size, int columnCount, string header, GUIContent[] enums, Action<int> action)
        {
            if (!singleton)
                singleton = CreateInstance<EnumerationPopup>();
            singleton.contents = enums;
            singleton.trigger = action;
            singleton.header = new GUIContent(header);
            size.y = (Mathf.Ceil(singleton.contents.Length / (float)columnCount) + 2) * 20;
            if (Event.current != null)
                pos = GUIUtility.GUIToScreenPoint(pos);
            singleton.ShowAsDropDown(pos.ToRect(), size);
        }


        public GUIContent[] contents;
        public Action<int> trigger;
        public int columnCount = 2;
        public GUIContent header;

        public Vector2 scroll;
        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField(header, Styles.Header);
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < columnCount; x++)
            {
                EditorGUILayout.BeginVertical();
                int i = x;
                for (int y = 0; i < contents.Length; y++)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button((i <= 9 ? (i + 1).ToString() : ""), Styles.LeftButton, GUILayout.Width(20)))
                        Apply(i);
                    if (GUILayout.Button(contents[i], Styles.MainButton, GUILayout.ExpandWidth(true)))
                        Apply(i);
                    GUILayout.EndHorizontal();
                    i = x + (y + 1) * columnCount;
                }
                EditorGUILayout.EndVertical();
                //GUILayout.Space(10);

            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();

            var ev = Event.current;
            if (ev.type != EventType.KeyUp)
                return;
            switch (ev.keyCode)
            {
                case KeyCode.Escape: Close(); break;
                case KeyCode.Return: Apply(0); break;
                case KeyCode.Alpha1: Apply(0); break;
                case KeyCode.Alpha2: Apply(1); break;
                case KeyCode.Alpha3: Apply(2); break;
                case KeyCode.Alpha4: Apply(3); break;
                case KeyCode.Alpha5: Apply(4); break;
                case KeyCode.Alpha6: Apply(5); break;
                case KeyCode.Alpha7: Apply(6); break;
                case KeyCode.Alpha8: Apply(7); break;
                case KeyCode.Alpha9: Apply(8); break;
                case KeyCode.Alpha0: Apply(9); break;
            }
        }

        void Apply(int index)
        {
            if (index >= contents.Length)
                return;
            trigger(index);
            Close();
        }
    }
    }
