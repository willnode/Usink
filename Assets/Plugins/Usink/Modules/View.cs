using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using Prefs = Usink.Config.View;

namespace Usink
{
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
                    
                    case Prefs.Up:
                    break;
                }
            }
        }

        void MoveUp()
        {
            
        }
    }
}