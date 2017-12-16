using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Text.RegularExpressions;

namespace Usink
{
    static public class Extras
    {
        static Event ev { get { return Event.current; } }

        /// <summary>
        /// Open dialog to quickly open scene in project
        /// </summary>
        static public void OpenSceneDialog()
        {
            var paths = Array.ConvertAll(AssetDatabase.FindAssets("t:Scene"), x => AssetDatabase.GUIDToAssetPath(x));

            var namePaths = Array.ConvertAll(paths, x => Path.GetFileNameWithoutExtension(x));

            var mode = ev.shift ? OpenSceneMode.Additive : OpenSceneMode.Single;

            SearchablePopup.Show(ev.mousePosition, "Open Scene" + (ev.shift ? " (Additive)" : "")
                , namePaths, (x) => EditorSceneManager.OpenScene(paths[x], mode));

        }

        /// <summary>
        /// Open dialog to quickly rename object (support bulk rename)
        /// </summary>
        static public void OpenRenameDialog()
        {
            var names = Array.ConvertAll(Selection.gameObjects, x => (x.name));
            var baseNames = Array.ConvertAll(names, x => kBaseName.Replace(x, ""));

            // Check if name consistent
            // 0 = Names consistent, 1 = Base Name consistent, 2 = All mixed
            int flag = names.All(x => x == names[0]) ? 0 :
                (baseNames.All(x => x == baseNames[0]) ? 1 : 2);
            InputStringPopup.Show(Event.current.mousePosition, "Rename",
                flag == 0 ? names[0] : (flag == 1 ? baseNames[0] : ""), false, (x) =>
                {
                    foreach (var g in Selection.gameObjects)
                    {
                        g.name = flag == 1 ? x + (kBaseName.Match(g.name)) : x;
                    }
                });
        }

        static readonly Regex kBaseName = new Regex("([ ]?[(]\\d+[)])");

        /// <summary>
        /// Open dialog to quickly remove object component (support bulk removal)
        /// </summary>
        static public void OpenRemoveComponentDialog()
        {
            var TComponents = new HashSet<Type>();
            var objects = Selection.gameObjects.Clone() as GameObject[];

            foreach (var g in objects)
            {
                TComponents.UnionWith(Array.ConvertAll(g.GetComponents<Component>(), x => x.GetType()));
            }

            var TComps = TComponents.ToArray();

            SearchablePopup.Show(Event.current.mousePosition, "Remove Component",
                Array.ConvertAll(TComps, x => ObjectNames.NicifyVariableName(x.Name)), (x) =>
                {
                    foreach (var g in objects)
                    {
                        var c = g.GetComponent(TComps[x]);
                        if (c) Undo.DestroyObjectImmediate(c);
                    }
                });

        }

        /// <summary>
        /// Select similar objects related so active (selected) gameobject
        /// </summary>
        static public void OpenSelectLinkedDialog()
        {
            var src = Selection.activeGameObject;
            EnumerationPopup.ShowEnumeration<LinkedTypes>(Event.current.mousePosition, "Select Linked", (x) =>
            {
                var link = (LinkedTypes)x;
                var gameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                var dest = new List<GameObject>(16);
                var srcname = kBaseName.Replace(src.name, "");
                var srctrs = src.transform;
                var srcprfb = PrefabUtility.GetPrefabParent(src);
                var srcfilt = src.GetComponent<MeshFilter>();
                var srcrend = src.GetComponent<MeshRenderer>();
                var meshObj = srcfilt ? srcfilt.sharedMesh : null;
                var matObj = srcrend ? srcrend.sharedMaterial : null;
                var snapY = EditorPrefs.GetFloat("MoveSnapY", 1f);

                if (link == LinkedTypes.Prefab && !srcprfb)
                    return;
                if (link == LinkedTypes.Mesh && !meshObj)
                    return;
                if (link == LinkedTypes.Material && !matObj)
                    return;

                foreach (var g in gameObjects)
                    if (g.activeInHierarchy)
                        switch (link)
                        {
                            case LinkedTypes.Name: if (g.name.Contains(srcname)) dest.Add(g); break;
                            case LinkedTypes.Prefab: if (PrefabUtility.GetPrefabParent(g) == srcprfb) dest.Add(g); break;
                            case LinkedTypes.Layer: if (g.layer == src.layer) dest.Add(g); break;
                            case LinkedTypes.Tag: if (g.tag == src.tag) dest.Add(g); break;
                            case LinkedTypes.Mesh: if ((srcfilt = g.GetComponent<MeshFilter>()) && srcfilt.sharedMesh == meshObj) dest.Add(g); break;
                            case LinkedTypes.Material: if ((srcrend = g.GetComponent<MeshRenderer>()) && srcrend.sharedMaterial == matObj) dest.Add(g); break;
                            case LinkedTypes.Sibling: if (g.transform.parent == srctrs.parent) dest.Add(g); break;
                            case LinkedTypes.Level: if (Utility.CloseEnough(g.transform.position.y, srctrs.position.y, snapY)) dest.Add(g); break;
                        }

                Selection.objects = dest.ToArray();
            });
        }

        enum LinkedTypes { Name, Prefab, Layer, Tag, Mesh, Material, Sibling, Level, Parent, Children }

        /// <summary>
        /// Open popup to open layer masking (top right in editor)
        /// </summary>
        static public void OpenLayerMaskDialog()
        {
            ReflectionUtility.IStaticCall("LayerVisibilityWindow", "ShowAtPosition", ev.mousePosition.ToRect());
        }

        /// <summary>
        /// Open popup to open layout (top right in editor)
        /// </summary>
        static public void OpenLayoutDialog()
        {
            ReflectionUtility.IStaticCall("EditorUtility", "Internal_DisplayPopupMenu",
                GUIUtility.GUIToScreenPoint(ev.mousePosition).ToRect(), "Window/Layouts", null, 0);
        }

        /// <summary>
        /// Open popup to create game object
        /// </summary>
        static public void OpenAddGameObjectDialog()
        {
            // hieararchy window is mandatory :(
            var hierarchy = ReflectionUtility.IStaticCall("SceneHierarchyWindow", "get_lastInteractedHierarchyWindow");
            if (hierarchy == null)
                return;
            GUIUtility.hotControl = 0;
            GenericMenu genericMenu = new GenericMenu();
            if (Selection.activeGameObject == null)
                hierarchy.ICall("AddCreateGameObjectItemsToMenu", genericMenu, null, true, false, 0);
            else
                hierarchy.ICall("CreateGameObjectContextClick", genericMenu, 0);
            genericMenu.DropDown(ev.mousePosition.ToRect());
        }

        /// <summary>
        /// Open popup to pick objects behind
        /// </summary>
        static public void OpenSamplePopup()
        {
            var gms = new List<GameObject>(); GameObject g;

            while (g = HandleUtility.PickGameObject(ev.mousePosition, true, gms.ToArray()))
                if (!gms.Contains(g))
                    gms.Add(g);
                else
                    break;

            if (gms.Count == 0) return;

            var names = Array.ConvertAll(gms.ToArray(), x => x.name);

            EditorUtility.DisplayCustomMenu(ev.mousePosition.ToRect(), Array.ConvertAll(names, (x) => new GUIContent(x)),
                gms.IndexOf(Selection.activeGameObject), (obj, list, idx) => Selection.activeGameObject = gms[idx], null);
        }

        /// <summary>
        /// Open popup to set object gizmo
        /// </summary>
        static public void OpenObjectGizmoDialog()
        {
            var trans = Selection.GetTransforms(SelectionMode.ExcludePrefab | SelectionMode.Editable);
            if (trans.Length != 1)
                return;

            ReflectionUtility.IStaticCall("IconSelector", "ShowAtPosition",
                trans[0].gameObject, ev.mousePosition.ToRect(), true);
        }
    }
}
