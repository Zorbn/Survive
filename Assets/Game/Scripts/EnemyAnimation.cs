using Mirror;
using UnityEngine;

namespace Game.Scripts
{
    public class EnemyAnimation : NetworkBehaviour
    {
        public Animator animator;

        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private Vector3 oldPosition;
        private Transform enemyTransform;

        private void Start()
        {
            enemyTransform = transform;
            oldPosition = enemyTransform.position;
        }

        private void FixedUpdate()
        {
            var isMoving = false;
            
            if (enemyTransform.position != oldPosition)
            {
                isMoving = true;
                oldPosition = enemyTransform.position;
            }
            
            animator.SetBool(IsMoving, isMoving);
        }
    }
}
