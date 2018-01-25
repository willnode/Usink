using UnityEditor;
using UnityEngine;
using System;

namespace Usink
{
    /// <summary>
    /// A collection of settings that configurable via editor
    /// </summary>
    public static class Config
    {
        public static class View
        {
            public const bool Active = true;
            public const KeyCode Up = KeyCode.Keypad8;
            public const KeyCode Down = KeyCode.Keypad5;
            public const KeyCode Left = KeyCode.Keypad4;
            public const KeyCode Right = KeyCode.Keypad6;
            public const KeyCode TiltLeft = KeyCode.Keypad7;
            public const KeyCode TiltRight = KeyCode.Keypad9;
            public const KeyCode In = KeyCode.Keypad3;
            public const KeyCode Out = KeyCode.Keypad1;
            public const KeyCode Ortho = KeyCode.Keypad2;
            public const KeyCode GoToCamera = KeyCode.Keypad0;
            public const KeyCode GoToCanvas = KeyCode.KeypadPeriod;
        }

        public static class Tool
        {
            public const bool Active = true;

            //Q
            //W
            //E
            //R
            //T
            //Y
            //U
            //I
            public const KeyCode OpenScene = KeyCode.O;
            public const KeyCode SelectParent = KeyCode.P;
            public const KeyCode SelectPrev = KeyCode.LeftBracket;
            public const KeyCode SelectNext = KeyCode.RightBracket;
            //Backslash
            public const KeyCode SelectNone = KeyCode.A;
            public const KeyCode RemoveComp = KeyCode.S;
            public const KeyCode AddObject = KeyCode.D;
            //F
            public const KeyCode SetGizmo = KeyCode.G;
            public const KeyCode SetActive = KeyCode.H;
            public const KeyCode SetLock = KeyCode.J;
            public const KeyCode SelectOperation = KeyCode.K;
            public const KeyCode SelectLinked = KeyCode.L;
            //Semicolon
            //Quote
            //Z
            //X
            //C
            //V
            //B
            //N
            //M
            public const KeyCode OpenLayerMask = KeyCode.Comma;
            public const KeyCode OpenLayout = KeyCode.Period;
            //Slash

            //Misc...
            public const KeyCode OpenQuery = KeyCode.Space;
            public const KeyCode OpenRename = KeyCode.F2;
            public const KeyCode ClearConsole = KeyCode.None; //overrided by menuitem below
            public const bool RightClickSample = true;
            public const bool DeleteThenReselect = true;
        }

        [MenuItem("Tools/Usink/Clear Dev Console _F9")]
        public static void ClearDevConsole() { Extras.ClearDeveloperConsole(); }

        public static class Hint
        {
            public const bool Active = true;
        }

    }

}