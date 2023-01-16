using UnityEngine;

namespace Game.Scripts
{
    [CreateAssetMenu(fileName = "SnapPointSet", menuName = "ScriptableObjects/SnapPointSetObject", order = 1)]
    public class SnapPointSetObject : ScriptableObject
    {
        public Vector3 root;
        public Vector3 bounds;
        public Vector3[] snapOffsets;
        public Vector3[] spawnOffsets;
        public float[] spawnAngles;
    }
}