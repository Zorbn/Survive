using System;
using Mirror;
using UnityEngine;

namespace Game.Scripts
{
    public class EntityHealth : NetworkBehaviour, IHealth
    {
        [SerializeField] private int health = IHealth.DefaultMaxHealth;
        [SerializeField] private string hitParticleName;
        [SerializeField] private int hitParticleCount;
        [SerializeField] private Vector3 hitParticleOffset;

        private Transform entityTransform;
        private ParticleSystem hitParticles;

        private void Start()
        {
            hitParticles = GameObject.Find(hitParticleName).GetComponent<ParticleSystem>();
            entityTransform = transform;
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
            if (health > 0) EmitParticles();
        }
        
        public int GetHealth() => health;
        
        private void OnDestroy()
        {
            EmitParticles();
        }

        private void EmitParticles()
        {
            if (!hitParticles) return;
            
            var emitParams = new ParticleSystem.EmitParams
            {
                position = entityTransform.position + hitParticleOffset,
                applyShapeToPosition = true
            };

            hitParticles.Emit(emitParams, hitParticleCount);
        }
    }
}