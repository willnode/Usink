#pragma warning disable 162
using UnityEngine;
using UnityEditor;
using Prefs = Usink.Config.View;

namespace Usink
{
    // ok
    public class View : Base
    {
        static public View singleton;

        [InitializeOnLoadMethod]
        static void WarmUp()
        {
            Start<View>((x) => singleton = x);
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

        protected override void OnSceneGUI(SceneView view)
        {
            var ev = Event.current;
            if (ev.type == EventType.KeyDown)
            {
                switch (ev.keyCode)
                {
                    case Prefs.Up: Rotate(0, view, true); break;
                    case Prefs.Down: Rotate(0, view, false); break;
                    case Prefs.Left: Rotate(1, view, true); break;
                    case Prefs.Right: Rotate(1, view, false); break;
                    case Prefs.TiltLeft: Rotate(2, view, true); break;
                    case Prefs.TiltRight: Rotate(2, view, false); break;
                    case Prefs.Ortho: view.orthographic = !view.orthographic; break;
                    case Prefs.Camera: if (Camera.main) view.AlignViewToObject(Camera.main.transform); break;
                }
            }
        }

        void ViewToCamera(SceneView view)
        {
            var cam = Camera.main ? Camera.main : (Selection.activeGameObject ? Selection.activeGameObject.GetComponent<Camera>() : null);
            if (cam)
                view.AlignViewToObject(cam.transform);
        }

        void Rotate(int axis, SceneView view, bool up)
        {
            var now = new SceneState(view);
            var eul = now.rotation.eulerAngles;
            eul[axis] = Mathf.Round(eul[axis] / 90f) * 90f;
            now.rotation = Quaternion.Euler(eul); // align
            var v = new Vector3(); v[axis] = up ? 90 : -90;
            now.rotation *= Quaternion.Euler(v);
            now.Apply(view);
        }
    }
}