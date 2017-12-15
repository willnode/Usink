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
            if (ev.type == EventType.KeyUp && ev.modifiers == EventModifiers.None)
            {
                switch (ev.keyCode)
                {
                    case KeyCode.F2: Extras.OpenRenameDialog(); break;
                    case KeyCode.L: Extras.OpenSelectLinkedDialog(); break;
                    case KeyCode.G: Extras.OpenObjectGizmoDialog(); break;
                    case KeyCode.Comma: Extras.OpenLayerMaskDialog(); break;
                    case KeyCode.Period: Extras.OpenLayoutDialog(); break;
                    case KeyCode.A: SelectNone(); break;
                    case KeyCode.S: Extras.OpenRemoveComponentDialog(); break;
                    case KeyCode.O: Extras.OpenSceneDialog(); break;
                    case KeyCode.D: Extras.OpenAddGameObjectDialog(); break;
                    case KeyCode.Space: AssetSearcherPopup.Show(ev.mousePosition); break;
                }
            }
        }

        int[] lastselect = null;

        void SelectNone()
        {
            if (Selection.activeGameObject)
            {
                lastselect = Selection.instanceIDs;
                Selection.instanceIDs = new int[0];
            }
            else Selection.instanceIDs = lastselect;
        }
    }
}