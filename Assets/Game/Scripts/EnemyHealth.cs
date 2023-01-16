using System;
using Mirror;
using UnityEngine;

namespace Game.Scripts
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class EnemyHealth : NetworkBehaviour, IHealth
    {
        [SerializeField] private int health = IHealth.DefaultMaxHealth;

        private Transform enemyTransform;
        private CapsuleCollider capsuleCollider;
        private static ParticleSystem HitParticles;

        private void Start()
        {
            if (!HitParticles) HitParticles = GameObject.Find("BloodParticles").GetComponent<ParticleSystem>();
            enemyTransform = transform;
            capsuleCollider = GetComponent<CapsuleCollider>();
        }

        public bool TakeDamage(int damage)
        {
            if (!isServer) throw new MethodAccessException("Cannot call TakeDamage on a client!");
            
            LocalTakeDamage(damage);
            RpcTakeDamage(damage);

            if (health > 0) return false;
            
            NetworkServer.Destroy(gameObject);
            return true;
        }

        [ClientRpc]
        private void RpcTakeDamage(int damage)
        {
            if (isServer) return;
            
            LocalTakeDamage(damage);
        }

        private void LocalTakeDamage(int damage)
        {
            health -= damage;
            
            var emitParams = new ParticleSystem.EmitParams
            {
                position = enemyTransform.position + capsuleCollider.center,
                applyShapeToPosition = true
            };

            HitParticles.Emit(emitParams, 5);
        }
    }
}