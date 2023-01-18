using System;
using Mirror;
using UnityEngine;

namespace Game.Scripts
{
    public class PlayerBuilding : NetworkBehaviour
    {
        private const float BuildRange = 10f;
        private const float SnapCollisionPadding = 0.5f;
        // TODO: Change slope shape to be more like "Fortnite" to help with clipping.
        // Ie: It shouldn't have a base, it should just be the slope part, not half of a cube.

        public GameObject[] buildingParts;
        private int selectedPartIndex;
        private GameObject selectedPart;
        private SnapPointSetObject selectedSnapPointSet;
        
        private static int BuildingMask;
        private static int BuildOnMask;

        private Camera cam;
        private Transform camTransform;

        private void Start()
        {
            cam = Camera.main;
            if (cam is null) throw new ArgumentNullException(nameof(cam));
            camTransform = cam.transform;
            
            if (BuildingMask == 0) BuildingMask = LayerMask.GetMask("Building");
            if (BuildOnMask == 0) BuildOnMask = LayerMask.GetMask("Building", "Ground", "Obstacle");
            
            UpdateSelectedPart(0);
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            
            if (Input.GetButtonDown("Fire2"))
            {
                PlacePart();
            }

            if (Input.GetButtonDown("Fire3"))
            {
                CmdRemovePart(camTransform.position, camTransform.forward);
            }

            if (Input.GetButtonDown("Jump"))
            {
                UpdateSelectedPart();
            }
        }

        [Command]
        private void CmdRemovePart(Vector3 position, Vector3 rotation)
        {
            if (!Physics.Raycast(position, rotation, out RaycastHit hit, BuildRange)) return;
            if (!hit.collider.CompareTag("Building")) return;

            var health = hit.collider.gameObject.GetComponent<IHealth>();
            health.TakeDamage(health.GetHealth());
        }

        private void PlacePart()
        {
            if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, BuildRange,
                    BuildOnMask)) return;
            
            if (hit.collider.CompareTag("Building"))
            {
                SnapPart(hit.point, hit.normal, hit.collider.gameObject);
                return;
            }
                
            SpawnPart(hit.point);
        }

        private void SnapPart(Vector3 targetPos, Vector3 targetNormal, GameObject snapObject)
        {
            Transform snapTransform = snapObject.transform;
            Vector3 snapCenter = snapTransform.position;
            SnapPointSetObject snapPoints = snapObject.GetComponent<BuildingPart>().snapPointSet;

            float minDistance = Mathf.Infinity;
            var minIndex = 0;

            for (var i = 0; i < snapPoints.snapOffsets.Length; i++)
            {
                float distance = Vector3.Distance(targetPos, snapObject.transform.rotation * snapPoints.snapOffsets[i] + snapCenter);

                if (distance >= minDistance) continue;
                
                minIndex = i;
                minDistance = distance;
            }

            Quaternion snapRotation = snapTransform.rotation;
            if (minIndex < snapPoints.spawnAngles.Length)
            {
                Vector3 snapRotationEuler = snapRotation.eulerAngles;
                snapRotationEuler.y += snapPoints.spawnAngles[minIndex];
                snapRotation.eulerAngles = snapRotationEuler;
            }

            Vector3 spawnOffset = snapPoints.snapOffsets[minIndex];
            if (snapPoints.spawnOffsets.Length > minIndex)
            {
                spawnOffset = snapPoints.spawnOffsets[minIndex];
            }
            
            Vector3 snapPos = snapCenter + snapObject.transform.rotation * spawnOffset;
            Vector3 root = selectedSnapPointSet.root;

            if (targetNormal.y < 0f)
            {
                root.y *= -1f;
            }
            
            snapPos -= snapRotation * root;
            
            if (Physics.CheckBox(snapPos, GetExtents(selectedSnapPointSet) * SnapCollisionPadding, snapRotation, BuildingMask)) return;
            
            CmdSpawnPart(selectedPartIndex, snapPos, snapRotation);
        }

        private void SpawnPart(Vector3 position)
        {
            Quaternion rotation = selectedPart.transform.rotation * Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f);
            if (Physics.CheckBox(position, GetExtents(selectedSnapPointSet), rotation, BuildingMask)) return;
            CmdSpawnPart(selectedPartIndex, position, rotation);
        }

        [Command]
        private void CmdSpawnPart(int partIndex, Vector3 position, Quaternion rotation)
        {
            GameObject newPart = Instantiate(buildingParts[partIndex], position, rotation);
            NetworkServer.Spawn(newPart);
        }

        private static Vector3 GetExtents(SnapPointSetObject snapPointSet)
        {
            return snapPointSet.bounds * 0.5f;
        }

        private void UpdateSelectedPart(int increment = 1)
        {
            selectedPartIndex = (selectedPartIndex + increment) % buildingParts.Length;
            selectedPart = buildingParts[selectedPartIndex];
            selectedSnapPointSet = selectedPart.GetComponent<BuildingPart>().snapPointSet;
        }
    }
}