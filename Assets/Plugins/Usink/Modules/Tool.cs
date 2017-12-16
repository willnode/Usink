#pragma warning disable 162
using UnityEngine;
using UnityEditor;
using Prefs = Usink.Config.Tool;
using System.Collections.Generic;

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
                else if (ev.type == EventType.MouseDown && Prefs.RightClickSample)
                    lastDown = ev.button == 1 ? ev.mousePosition : new Vector2();
            }
            if (ev.type == EventType.KeyUp && ev.modifiers == EventModifiers.None)
            {
                switch (ev.keyCode)
                {           
                    case Prefs.OpenScene: Extras.OpenSceneDialog(); break;
                    case Prefs.SelectParent: SelectParent(); break;                  
                    case Prefs.SelectPrev: SelectPrev(); break;
                    case Prefs.SelectNext: SelectNext(); break;
                    case Prefs.SelectNone: SelectNone(); break;
                    case Prefs.RemoveComp: Extras.OpenRemoveComponentDialog(); break;
                    case Prefs.AddObject: Extras.OpenAddGameObjectDialog(); break;
                    case Prefs.SetGizmo: Extras.OpenObjectGizmoDialog(); break;
                    case Prefs.SetActive: ToggleActive(); break;
                    case Prefs.SetLock: ToggleLock(); break;
                    case Prefs.SelectOperation: Extras.OpenSelectOperationDialog(ev.shift); break;
                    case Prefs.SelectLinked: Extras.OpenSelectLinkedDialog(); break;
                    case Prefs.OpenLayerMask: Extras.OpenLayerMaskDialog(); break;
                    case Prefs.OpenLayout: Extras.OpenLayoutDialog(); break;
                    case Prefs.OpenRename: Extras.OpenRenameDialog(); break;
                    case Prefs.OpenQuery: AssetSearcherPopup.Show(ev.mousePosition); break;
                    case Prefs.ClearConsole: Extras.ClearDeveloperConsole(); break;
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

        void SelectParent ()
        {
            var ts = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.ExcludePrefab);
            var dest = new List<Transform>();
            foreach (var t in ts)
            {
                if (t.parent)
                    dest.Add(t.parent);
            }
            Selection.instanceIDs = dest.ConvertAll(x => x.gameObject.GetInstanceID()).ToArray();
        }
        void SelectPrev()
        {

        }
        void SelectNext()
        {
            
        }
        void ToggleActive()
        {
            foreach (var g in Selection.gameObjects)
            {
                Undo.RecordObject(g, "Toggle Active");
                g.SetActive(!g.activeSelf);
            }
        }
        void ToggleLock()
        {
            foreach (var g in Selection.gameObjects)
            {
                g.hideFlags = g.hideFlags.SetFlag(HideFlags.NotEditable,
                    !g.hideFlags.GetFlag(HideFlags.NotEditable));
            }
        }
    }
}