#pragma warning disable 162
using UnityEngine;
using UnityEditor;
using Prefs = Usink.Config.View;

namespace Usink
{
    // ok
    public class View : Base
    {
        static View singleton;

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
                }
            }
        }

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