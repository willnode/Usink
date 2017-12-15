
using System;
using System.IO;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor;

namespace Usink
{
    public class InputStringPopup : EditorWindow
    {
        static InputStringPopup _singleton;

        bool isFirstGUI = false;
        bool emptyAcceptable = false;
        string input;
        string header;
        Action<string> OnAccepted;

        public static void Show(Vector2 pos, string header, string input, bool emptyAcceptable, Action<string> OnAccepted)
        {
            if (!_singleton)
                _singleton = ScriptableObject.CreateInstance<InputStringPopup>();
            _singleton.header = header;
            _singleton.input = input;
            _singleton.isFirstGUI = true;
            _singleton.emptyAcceptable = emptyAcceptable;
            _singleton.OnAccepted = OnAccepted;
            _singleton.ShowAsDropDown(GUIUtility.GUIToScreenPoint(pos).ToRect(),
            new Vector2(200, 90));
        }

        public static GUIStyle Header = new GUIStyle("In BigTitle");

        private void OnGUI()
        {
            GUILayout.Space(5f);
            Event current = Event.current;
            EditorGUILayout.LabelField(header, Header, GUILayout.Height(25));
            bool flag = current.type == EventType.KeyDown && (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter);
            GUI.SetNextControlName("m_PreferencesName");
            input = EditorGUILayout.TextField(input);
            if (isFirstGUI)
            {
                isFirstGUI = false;
                EditorGUI.FocusTextInControl("m_PreferencesName");
            }
            var okAllowed = emptyAcceptable || input.Length > 0;
            GUI.enabled = okAllowed;
            if (GUILayout.Button("OK") || (flag & okAllowed))
            {
                Close();
                OnAccepted(input);
            }
        }
    }
}
