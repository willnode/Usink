#pragma warning disable 162
using UnityEngine;
using UnityEditor;
using Prefs = Usink.Config.View;
using System.Linq;

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
                    case Prefs.In: Zoom(view, true); break;
                    case Prefs.Out: Zoom(view, false); break;
                    case Prefs.Ortho: view.orthographic = !view.orthographic; break;
                    case Prefs.GoToCamera: ViewToCamera(view); break;
                    case Prefs.GoToCanvas: ViewToCanvas(view); break;
                }
            }
        }

        SceneState savedstate;

        void ViewToCamera(SceneView view)
        {
            var cam = Camera.main ? Camera.main : (Selection.activeGameObject ? Selection.activeGameObject.GetComponent<Camera>() : null);
            if (cam)
            {
                var state = new SceneState(view);
                if (state.rotation == cam.transform.rotation && Mathf.Approximately(state.size, 10f))
                    savedstate.Apply(view);
                else
                {
                    savedstate = state;
                    view.AlignViewToObject(cam.transform);
                }
            }
        }

        void ViewToCanvas(SceneView view)
        {
            Canvas canvas = FindObjectsOfType<Canvas>().FirstOrDefault(x => x.enabled
                && x.isRootCanvas && x.renderMode == RenderMode.ScreenSpaceOverlay);

            if (!canvas) return;
            var rect = canvas.transform.localToWorldMatrix.Multiply(canvas.GetComponent<RectTransform>().rect);           
            var ratio = Mathf.Min(view.camera.aspect, rect.size.x / rect.size.y);
            var size = rect.size.ScalarMax() * 1.05f * 0.707106769f / Mathf.Sqrt(ratio); // trigonometry
            var newState = new SceneState() { pivot = rect.center, rotation = Quaternion.identity, size = size };
            var state = new SceneState(view);

            if (state != newState)
            {
                savedstate = state;
                newState.Apply(view);
            } else
                savedstate.Apply(view);
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

        const float speed = 0.9f;
        const float invspeed = 1f / speed;

        void Zoom(SceneView view, bool In)
        {
            // yes. KeyDown is a recurring event like fire()
            var now = new SceneState(view);
            now.size *= In ? speed : invspeed;
            now.Apply(view);
        }
    }
}