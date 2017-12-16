#pragma warning disable 162
using UnityEngine;
using UnityEditor;
using Prefs = Usink.Config.Hint;

// ok
namespace Usink
{
    public class Hint : Base
    {
        static public Hint singleton;

        public string ObjectLabel;
        public string ActionLabel;

        [InitializeOnLoadMethod]
        static void WarmUp()
        {
            Start<Hint>((x) => singleton = x);
        }

        protected override void OnEnable()
        {
            if (!Prefs.Active) return;
            RegisterOnSceneGUI(true);
            RegisterOnSelectionChange(true);
        }

        protected override void OnDisable()
        {
            if (!Prefs.Active) return;
            RegisterOnSceneGUI(false);
            RegisterOnSelectionChange(false);
        }

        protected override void OnSceneGUI(SceneView view)
        {
            if (Event.current.type == EventType.Repaint /*&& (ActionLabel != null || ObjectLabel != null)*/)
            {
                Handles.BeginGUI();
                Utility.GUITint(Color.white, Color.black, () =>
                {
                    var wh = new Vector2(Screen.width, Screen.height);
                    var r = new Rect(0, wh.y - 75, wh.x, 50);
                    r = new Rect(30, wh.y - 70, wh.x - 60, 20);
                    // Text              
                    GUI.Box(r, GUIContent.none, Styles.backStatus);
                    // Background
                    if (ActionLabel != null)
                        EditorGUI.LabelField(r, ActionLabel, Styles.textStatus);
                    if (ObjectLabel != null)
                        EditorGUI.LabelField(r, ObjectLabel, Styles.textStatus);
                });
                Handles.EndGUI();
            }
        }

        public static void Log (string action)
        {
            singleton.ActionLabel = action;
        }

        protected override void OnSelectionChange()
        {
            var selection = Selection.gameObjects;            
            if (selection.Length == 0)
                singleton.ObjectLabel = null;
            else if (selection.Length == 1)
                singleton.ObjectLabel = selection[0].name;
            else
                singleton.ObjectLabel = string.Format("{0}, ({1})", selection[0].name, selection.Length);
            ActionLabel = null;
        }


        static class Styles
        {

            static public GUIStyle textStatus = new GUIStyle(EditorStyles.wordWrappedLabel);
            static public GUIStyle backStatus = new GUIStyle(EditorStyles.helpBox);

            static Styles()
            {
                textStatus.richText = true;
                textStatus.normal.textColor = Color.white;
            }
        }

    }
}