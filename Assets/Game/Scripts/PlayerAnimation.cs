using Mirror;
using UnityEngine;

namespace Game.Scripts
{
    public class PlayerAnimation : NetworkBehaviour
    {
        public GameObject model;
        public Animator animator;

        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private Vector3 oldPosition;
        private Transform playerTransform;

        private void Start()
        {
            playerTransform = transform;
            oldPosition = playerTransform.position;
        }

        public override void OnStartLocalPlayer()
        {
            model.SetActive(false);
        }

        private void Update()
        {
            if (isLocalPlayer) return;

            var isMoving = false;
            
            if (playerTransform.position != oldPosition)
            {
                isMoving = true;
                oldPosition = playerTransform.position;
            }
            
            animator.SetBool(IsMoving, isMoving);
        }
    }
}
