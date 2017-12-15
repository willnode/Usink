using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;

namespace Usink
{
    public class Hint : Base
    {
        static Hint singleton;

        [InitializeOnLoadMethod]
        static void WarmUp()
        {
            Start<Hint>((x) => singleton = x);
        }

        protected override void OnEnable()
        {


        }
        
        protected override void OnDisable()
        {


        }

    }
}