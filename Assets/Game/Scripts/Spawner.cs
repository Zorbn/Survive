using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Game.Scripts
{
    public class Spawner : NetworkBehaviour
    {
        [SerializeField] private GameObject entity;
        [SerializeField] private float range = 10f;
        [SerializeField] private int waveSize = 10;
        [SerializeField] private float waveDelay = 3f;
        [SerializeField] private float tickTime = 0.5f;
        
        private readonly List<GameObject> entities = new();
        private float waveTimer;
        private float tickTimer;
        private Transform spawnerTransform;
        private bool needsSpawn = true;
        
        private void Start()
        {
            spawnerTransform = transform;
        }

        private void Update()
        {
            if (!isServer) return;

            tickTimer += Time.deltaTime;
            if (needsSpawn) waveTimer += Time.deltaTime;

            while (tickTimer > tickTime)
            {
                tickTimer -= tickTime;
                Tick();
            }
        }

        private void Tick()
        {
            if (!needsSpawn)
            {
                for (int i = entities.Count - 1; i >= 0; i--)
                {
                    if (entities[i]) continue;
                    entities.RemoveAt(i);
                }

                if (entities.Count == 0)
                {
                    needsSpawn = true;
                }
            }

            if (!needsSpawn || waveTimer < waveDelay) return;

            waveTimer = 0f;
            needsSpawn = false;

            while (entities.Count < waveSize)
            {
                Vector3 spawnPos = spawnerTransform.position + new Vector3(Random.Range(-range, range), 0f,
                    Random.Range(-range, range));
                GameObject newEntity = Instantiate(entity, spawnPos, Quaternion.identity);
                entities.Add(newEntity);
                NetworkServer.Spawn(newEntity);
            }
        }
    }
}
