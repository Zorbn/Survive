using Mirror;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Scripts
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyMovement : NetworkBehaviour
    {
        private const float AttackRangeMultiplier = 1.1f;
        private const float PathingUpdateTime = 0.2f;
        
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float stopRange = 2f;
        [SerializeField] private float accelerationMultiplier = 4f;
        [SerializeField] private float attackCooldown = 1f;

        private NavMeshAgent navMeshAgent;
        private GameNetworkManager networkManager;
        private Transform nearestPlayer;
        private Transform enemyTransform;
        private float attackTimer;
        private float pathingUpdateTimer;
        
        private void Start()
        {
            networkManager = GameObject.Find("NetworkManager").GetComponent<GameNetworkManager>();
            enemyTransform = transform;
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.stoppingDistance = stopRange;
            navMeshAgent.acceleration = moveSpeed * accelerationMultiplier;
        }

        private void FixedUpdate()
        {
            if (!isServer) return;

            pathingUpdateTimer -= Time.deltaTime;

            if (pathingUpdateTimer <= 0f)
            {
                UpdatePath();
                pathingUpdateTimer = PathingUpdateTime;
            }

            if (!nearestPlayer) return;
            
            attackTimer -= Time.deltaTime;
            
            if (attackTimer > 0f) return;
            if (Vector3.Distance(enemyTransform.position, nearestPlayer.position) >
                stopRange * AttackRangeMultiplier) return;
            
            Attack();
            attackTimer = attackCooldown;
        }

        private void UpdatePath()
        {
            FindNearestPlayer();

            if (!nearestPlayer) return;
            
            navMeshAgent.destination = nearestPlayer.position;
        }

        private void FindNearestPlayer()
        {
            float distance = Mathf.Infinity;
            
            foreach (GameObject player in networkManager.serverPlayerGameObjects)
            {
                float distToPlayer = Vector3.Distance(player.transform.position, enemyTransform.position);
                if (distToPlayer >= distance) continue;
                distance = distToPlayer;
                nearestPlayer = player.transform; 
            }
        }

        private void Attack()
        {
            var health = nearestPlayer.GetComponent<IHealth>();
            health.TakeDamage(20);
        }
    }
}
