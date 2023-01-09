using UnityEngine;

namespace Game.Scripts
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/WeaponObject", order = 1)]
    public class WeaponObject : ScriptableObject
    {
        [Header("Attack")]
        public int attackDamage = 10;
        public float attackRange = 10f;
        
        [Header("Recoil")]
        public float recoilSpeed = 1f;
        public float recoilDistance = 1f;
        public float recoilVertDistance = 1f;
        public float recoilAngle = 90f;
    }
}
