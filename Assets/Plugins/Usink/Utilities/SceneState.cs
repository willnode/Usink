using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Usink
{

    [System.Serializable]
    public struct SceneState
    {

        public Vector3 pivot;
        public Quaternion rotation;
        public float size;

        public SceneState(SceneView view)
        {
            // to get the accurate (not the ongoing/current state) result we need to do it this way
            pivot = view.IGetField<AnimVector3>("m_Position").target;
            rotation = view.IGetField<AnimQuaternion>("m_Rotation").target;
            size = view.IGetField<AnimFloat>("m_Size").target;
        }

        public void Apply(SceneView view)
        {
            view.LookAt(pivot, rotation, size);
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj is SceneState)
            {
                var o = (SceneState)obj;
                return o.pivot == pivot && o.rotation == rotation && o.size == size;
            }
            else
                return false;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(SceneState lhs, SceneState rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(SceneState lhs, SceneState rhs)
        {
            return !lhs.Equals(rhs);
        }

    }
}
