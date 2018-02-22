#pragma warning disable 162
using UnityEngine;
using UnityEditor;
using Prefs = Usink.Config.Tool;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

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
            EditorApplication.playmodeStateChanged += TurnSGMOff;
            UnityEditor.SceneManagement.EditorSceneManager.sceneClosing += TurnSGMOff;
        }

        protected override void OnDisable()
        {
            if (Prefs.Active)
                RegisterOnSceneGUI(false);
            TurnSGMOff();
            EditorApplication.playmodeStateChanged -= TurnSGMOff;
            UnityEditor.SceneManagement.EditorSceneManager.sceneClosing -= TurnSGMOff;
        }

        void TurnSGMOff()
        {
            if (sgm_on)
                ToggleSeparateLayer();
        }

        void TurnSGMOff(Scene v, bool b)
        {
            TurnSGMOff();
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
            if (ev.type == EventType.KeyUp && modifierOK(ev.modifiers))
            {
                switch (ev.keyCode)
                {
                    case Prefs.SelectParent: SelectOrMakeParent(); break;
                    case Prefs.SelectPrev: SelectOrGroupPrev(); break;
                    case Prefs.SelectNext: SelectOrGroupNext(); break;
                    case Prefs.SelectNone: SelectNone(); break;
                    case Prefs.RemoveComp: Extras.OpenRemoveComponentDialog(); break;
                    case Prefs.AddObject: if (ev.shift) SelectParent(Selection.GetTransforms(SelectionMode.ExcludePrefab)); Extras.OpenAddGameObjectDialog(); break;
                    case Prefs.SetGizmo: Extras.OpenObjectGizmoDialog(); break;
                    case Prefs.SetActive: ToggleActive(); break;
                    case Prefs.SetLock: ToggleLock(); break;
                    case Prefs.SelectOperation: Extras.OpenSelectOperationDialog(ev.shift); break;
                    case Prefs.SelectLinked: Extras.OpenSelectLinkedDialog(); break;
                    case Prefs.GoSeparateLayer: ToggleSeparateLayer(); break;
                    case Prefs.OpenLayerMask: Extras.OpenLayerMaskDialog(); break;
                    case Prefs.OpenLayout: Extras.OpenLayoutDialog(); break;
                    case Prefs.OpenRename: Extras.OpenRenameDialog(); break;
                    case Prefs.OpenQuery: AssetSearcherPopup.Show(ev.mousePosition); break;
                    case Prefs.ClearConsole: Extras.ClearDeveloperConsole(); break;
                }
            }
        }

        bool modifierOK(EventModifiers m)
        {
            return m == EventModifiers.None || m == EventModifiers.FunctionKey || m == EventModifiers.Shift;
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

        void SelectParent(Transform[] ts)
        {
            var dest = new List<Transform>();
            foreach (var t in ts)
            {
                if (t.parent)
                    dest.Add(t.parent);
            }
            Selection.instanceIDs = dest.ConvertAll(x => x.gameObject.GetInstanceID()).ToArray();
        }

        void SelectPrev(Transform[] ts)
        {
            var dest = new List<Transform>();
            Transform[] roots = null;
            foreach (var t in ts)
            {
                // don't work for root so far.
                var i = t.GetSiblingIndex() - 1;
                if (i < 0) continue;
                if (t.parent == null)
                {
                    roots = roots ?? Utility.GetRootSiblings();
                    dest.Add(roots[i]);
                }
                else
                    dest.Add(t.parent.GetChild(i));
            }
            Selection.instanceIDs = dest.ConvertAll(x => x.gameObject.GetInstanceID()).ToArray();
        }

        void SelectNext(Transform[] ts)
        {
            var dest = new List<Transform>();
            Transform[] roots = null;
            foreach (var t in ts)
            {
                // don't work for root so far.
                var i = t.GetSiblingIndex() + 1;
                if (t.parent == null)
                {
                    roots = roots ?? Utility.GetRootSiblings();
                    if (i < roots.Length)
                        dest.Add(roots[i]);
                }
                else if (i < t.parent.childCount)
                    dest.Add(t.parent.GetChild(i));
            }
            Selection.instanceIDs = dest.ConvertAll(x => x.gameObject.GetInstanceID()).ToArray();
        }

        void SelectOrMakeParent()
        {
            var ts = Selection.GetTransforms(SelectionMode.ExcludePrefab);
            if (ts.Length > 1) Extras.MakeParent(Selection.activeTransform = Selection.activeTransform, ts);
            else SelectParent(ts);
        }

        void SelectOrGroupPrev()
        {
            var ts = Selection.GetTransforms(SelectionMode.ExcludePrefab);
            var min = ts.Min((t) => t.GetSiblingIndex());
            if (ts.Length > 1)
            {
                foreach (var t in ts)
                {
                    Undo.SetTransformParent(t, t.parent, "Group Objects Order"); // weird, but works.
                    t.SetSiblingIndex(min);
                }
            }
            else SelectPrev(ts);
        }

        void SelectOrGroupNext()
        {
            var ts = Selection.GetTransforms(SelectionMode.ExcludePrefab);
            var max = ts.Max((t) => t.GetSiblingIndex());
            if (ts.Length > 1)
            {
                foreach (var t in ts)
                {
                    Undo.SetTransformParent(t, t.parent, "Group Objects Order"); // weird, but works.
                    t.SetSiblingIndex(max);
                }
            }
            else SelectNext(ts);
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

        bool sgm_on = false;
        GameObject[] sgm_objects;
        int[] sgm_layers;
        int sgm_globalLayer;

        void ToggleSeparateLayer()
        {
            sgm_on = !sgm_on;
            if (sgm_on)
            {
                var objs = (Selection.GetTransforms(SelectionMode.ExcludePrefab | SelectionMode.Editable | SelectionMode.TopLevel));
                var count = objs.Length;
                var list = new List<GameObject>();
                for (int i = 0; i < count; i++)
                {
                    list.AddRange(Array.ConvertAll(objs[i].GetComponentsInChildren<Transform>(), x => x.gameObject));
                }
                sgm_objects = list.ToArray();
                sgm_layers = new int[sgm_objects.Length];
                sgm_globalLayer = Tools.visibleLayers;

                for (int i = 0; i < sgm_objects.Length; i++)
                {
                    sgm_layers[i] = sgm_objects[i].layer;
                    sgm_objects[i].layer = 31;
                }
                Tools.visibleLayers = 1 << 31;
            }
            else
            {
                for (int i = 0; i < sgm_objects.Length; i++)
                {
                    if (sgm_objects[i])
                        sgm_objects[i].layer = sgm_layers[i];
                }
                Tools.visibleLayers = sgm_globalLayer;
            }
        }

    }
}