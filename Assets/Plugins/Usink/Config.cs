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
            public const KeyCode Down = KeyCode.Keypad2;
            public const KeyCode Left = KeyCode.Keypad4;
            public const KeyCode Right = KeyCode.Keypad6;
            public const KeyCode Mode = KeyCode.Keypad5;
            public const KeyCode In = KeyCode.KeypadPlus;
            public const KeyCode Out = KeyCode.KeypadMinus;
            public const KeyCode Camera = KeyCode.Keypad0;
        }

        public static class Tool
        {
            public const bool Active = true;
            public const bool Aggresive = false;
            public const KeyCode Move = KeyCode.W;
            public const KeyCode Rotate = KeyCode.E;
            public const KeyCode Scale = KeyCode.R;
        }

        public static class Mask
        {
            public const bool Active = true;
            public const KeyCode SelectedOnly = KeyCode.KeypadEnter;
        }

        public static class Hint
        {
            public const bool Active = true;
        }

        public static class Jump
        {
            public const bool Active = true;
            public const KeyCode Show = KeyCode.Space;
        }

    }
}