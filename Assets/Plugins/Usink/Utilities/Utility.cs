using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;

namespace Usink
{
    // Using reflection, get access to hidden stuff .....
    public static class ReflectionUtility
    {

        static object[] emptyArgs = { };
        static Type[] UTTypes = typeof(Editor).Assembly.GetTypes();

        const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        const BindingFlags InstanceNoFlat = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        const BindingFlags NewCreate = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance;

        /// <summary>
        /// call to an instance method or function
        /// </summary>
        public static object ICall(this object obj, string methodName)
        {
            var dyn = obj.GetType().GetMethod(methodName, Instance);
            return dyn.Invoke(obj, emptyArgs);
        }

        /// <summary>
        /// call to an instance method or function with arguments
        /// </summary>
        public static object ICall(this object obj, string methodName, params object[] args)
        {
            var dyn = obj.GetType().GetMethod(methodName, Instance);
            return dyn.Invoke(obj, args);
        }

        /// <summary>
        /// call to an instance method or function with arguments with specific type
        /// </summary>
        public static T ICall<T>(this object obj, string methodName, params object[] args)
        {
            var dyn = obj.GetType().GetMethod(methodName, Instance);
            return (T)dyn.Invoke(obj, args);
        }

        /// <summary>
        /// call to an overrided method
        /// </summary>
        public static object ICallOverride(this object obj, string methodName, params object[] args)
        {
            var dyn = obj.GetType().GetMethod(methodName, InstanceNoFlat);
            return dyn.Invoke(obj, args);
        }

        /// <summary>
        /// call to an instance field
        /// </summary>
        public static object IGetField(this object obj, string memberName)
        {
            var dyn = obj.GetType().GetField(memberName, Instance);
            return dyn.GetValue(obj);
        }

        /// <summary>
        /// call to an instance field as specific type
        /// </summary>
        public static T IGetField<T>(this object obj, string memberName)
        {
            var dyn = obj.GetType().GetField(memberName, Instance);
            return (T)dyn.GetValue(obj);
        }

        /// <summary>
        /// set instance field to a value
        /// </summary>
        public static void ISetField(this object obj, string memberName, object value)
        {
            var dyn = obj.GetType().GetField(memberName, Instance);
            dyn.SetValue(obj, value);
        }

        /// ---------------------------STATIC---------------------------------------

        /// <summary>
        /// call to new constructor
        /// </summary>
        public static object INewCall(string typeName, params object[] args)
        {
            var typ = Type.GetType(typeName, false) ?? GetTypeFromUT(typeName);
            var dyns = typ.GetConstructors();
            var dyn = dyns.First(x => x.GetParameters().Length == args.Length);
            return dyn.Invoke(null, args);
        }

        /// <summary>
        /// get static field
        /// </summary>
        public static object IGetStaticField(string typeName, string memberName)
        {
            var typ = Type.GetType(typeName, false) ?? GetTypeFromUT(typeName);
            var dyn = typ.GetField(memberName, Static);
            return dyn.GetValue(null);
        }

        public static void ISetStaticField(string typeName, string memberName, object value)
        {
            var typ = Type.GetType(typeName, false) ?? GetTypeFromUT(typeName);
            var dyn = typ.GetField(memberName, Static);
            dyn.SetValue(null, value);
        }

        public static object IStaticCall(string typeName, string methodName)
        {
            var typ = Type.GetType(typeName, false) ?? GetTypeFromUT(typeName);
            var dyn = typ.GetMethod(methodName, Static);
            return dyn.Invoke(null, emptyArgs);
        }

        public static object IStaticCall(string typeName, string methodName, params object[] args)
        {
            var typ = Type.GetType(typeName, false) ?? GetTypeFromUT(typeName);
            var dyn = typ.GetMethod(methodName, Static);
            return dyn.Invoke(null, args);
        }

        public static Type GetTypeFromUT(string typeName)
        {
            return UTTypes.FirstOrDefault(x => x.Name == typeName);
        }
    }

    static public class Utility
    {
        static public Rect ToRect(this Vector2 point)
        {
            return new Rect(point, Vector2.zero);
        }

        static public float ScalarMax(this Vector3 v)
        {
            return Mathf.Max(v.x, Mathf.Max(v.y, v.z));
        }

        static public float ScalarMin(this Vector3 v)
        {
            return Mathf.Min(v.x, Mathf.Min(v.y, v.z));
        }

        static public T FirstOrNull<T>(this T[] objs)
        {
            return objs.Length > 0 ? objs[0] : default(T);
        }

        static public int Repeat(int i, int max)
        {
            return (i % max + max) % max;
        }
        
        static public bool CloseEnough(float value, float target, float radius)
        {
            return Mathf.Abs(value - target) < radius;
        }

        static public bool GetFlag(this HideFlags en, HideFlags value) 
        {
            return  (en & value) == value;
        }
        
        static public HideFlags SetFlag(this HideFlags en, HideFlags value, bool yes)
        {
            if (yes)
                return en | value;
            else
                return en & ~value;
        }

        static public void GUITint (Color text, Color bg, Action gui)
        {
            if (Event.current.type != EventType.Repaint) { gui(); return; }

            Color t = GUI.contentColor, b = GUI.backgroundColor;
            GUI.contentColor = text; GUI.backgroundColor = bg;
            gui();
            GUI.contentColor = t; GUI.backgroundColor = b;
        }
    }
}