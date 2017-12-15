#pragma warning disable 162
using UnityEngine;
using UnityEditor;
using Prefs = Usink.Config.View;

namespace Usink
{
    public class Tool : Base
    {
        static public Tool singleton;

        [InitializeOnLoadMethod]
        static void WarmUp()
        {
            Start<Tool>((x) => singleton = x);
        }

        protected override void OnEnable()
        {
            if (Prefs.Active)
                RegisterOnSceneGUI(true);
        }

        protected override void OnDisable()
        {
            if (Prefs.Active)
                RegisterOnSceneGUI(false);
        }

        Vector2 lastDown;

        protected override void OnSceneGUI(SceneView view)
        {
            var ev = Event.current;

            if (ev.isMouse)
            {
                if (ev.type == EventType.MouseUp && lastDown == ev.mousePosition)
                {
                    Tools.viewTool = ViewTool.None;
                    Extras.OpenSamplePopup();
                    ev.Use();
                }
                else if (ev.type == EventType.MouseDown)
                    lastDown = ev.button == 1 ? ev.mousePosition : new Vector2();
            }
            if (ev.type == EventType.KeyUp)
            {
                switch (ev.keyCode)
                {
                    case KeyCode.F2: Extras.OpenRenameDialog(); break;
                    case KeyCode.L: Extras.OpenSelectLinkedDialog(); break;
                    case KeyCode.G: Extras.OpenObjectGizmoDialog(); break;
                    case KeyCode.A: if (Selection.activeGameObject) { lastselect = Selection.instanceIDs; Selection.instanceIDs = new int[0]; } else Selection.instanceIDs = lastselect; break;
                    case KeyCode.Space: AssetSearcherPopup.Show(ev.mousePosition); break;
                }
            }
        }

        int[] lastselect = null;

        void Rotate(int axis, SceneView view, bool up)
        {
            var now = new SceneState(view);
            var eul = now.rotation.eulerAngles;
            eul[axis] = Mathf.Round(eul[axis] / 90f) * 90f + (up ? 90 : -90);
            now.rotation = Quaternion.Euler(eul);
            now.Apply(view);
        }
    }
}