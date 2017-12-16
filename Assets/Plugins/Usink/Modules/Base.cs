using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;

namespace Usink
{
    public abstract class Base : ScriptableObject
    {

        protected virtual void OnSceneGUI(SceneView view) { }

        protected virtual void OnSelectionChange() { }

        protected virtual void OnHierarchyGUI(int id, Rect r) { }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() { }

        protected void RegisterOnSceneGUI(bool reg)
        {
            if (reg)
                SceneView.onSceneGUIDelegate += OnSceneGUI;
            else
                SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        protected void RegisterOnSelectionChange(bool reg)
        {
            if (reg)
                Selection.selectionChanged += OnSelectionChange;
            else
                Selection.selectionChanged -= OnSelectionChange;
        }


        protected void RegisterOnHierarchyGUI(bool reg)
        {
            if (reg)
                EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            else
                EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
        }

        protected static void Start<T>(Action<T> delay) where T : Base
        {
            EditorApplication.delayCall += delegate ()
            {
                var g = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
                if (!g)
                {
                    g = CreateInstance<T>();
                    g.hideFlags = HideFlags.DontSave;
                }
                delay(g);
            };
        }
    }
}