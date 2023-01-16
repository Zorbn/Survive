using System;
using Mirror;
using UnityEngine;

namespace Game.Scripts
{
    public class PlayerHealth : NetworkBehaviour, IHealth
    {
        [SerializeField] private int health = IHealth.DefaultMaxHealth;

        private Transform playerTransform;
        private static ParticleSystem HitParticles;

        private void Start()
        {
            if (!HitParticles) HitParticles = GameObject.Find("BloodParticles").GetComponent<ParticleSystem>();
            playerTransform = transform;
        }

        public bool TakeDamage(int damage)
        {
            if (!isServer) throw new MethodAccessException("Cannot call TakeDamage on a client!");
            
            RpcTakeDamage(damage);
            return LocalTakeDamage(damage);
        }

        [ClientRpc]
        private void RpcTakeDamage(int damage)
        {
            if (!isServer)
            {
                LocalTakeDamage(damage);
            }

            if (health > 0 || !isLocalPlayer) return;
            
            playerTransform.position = Vector3.zero;
            CmdRespawn();
        }

        private bool LocalTakeDamage(int damage)
        {
            health -= damage;
            
            var emitParams = new ParticleSystem.EmitParams
            {
                position = playerTransform.position,
                applyShapeToPosition = true
            };

            HitParticles.Emit(emitParams, 5);

            return health <= 0;
        }

        [Command]
        private void CmdRespawn()
        {
            health = IHealth.DefaultMaxHealth;
        }

        [ClientRpc]
        private void RpcRespawn()
        {
            if (isServer) return;
            
            health = IHealth.DefaultMaxHealth;
        }
        
        public int GetHealth() => health;
    }
}