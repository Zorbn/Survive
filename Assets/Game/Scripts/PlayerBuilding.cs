using Unity.Mathematics;
using UnityEngine;

namespace Game.Scripts
{
    public class PlayerBuilding : MonoBehaviour
    {
        private const float BuildRange = 10f;
        private const float SnapCollisionPadding = 0.5f;

        public GameObject[] buildingParts;
        private int selectedPartIndex;
        private static int BuildingMask;

        private Camera cam;

        private void Start()
        {
            cam = Camera.main;
            BuildingMask = LayerMask.GetMask("Building");
        }

        private void Update()
        {
            if (Input.GetButtonDown("Fire2"))
            {
                // TODO: Check to make sure the new building part won't intersect existing ones.
                PlacePart();
            }

            if (Input.GetButtonDown("Jump"))
            {
                selectedPartIndex = (selectedPartIndex + 1) % buildingParts.Length;
            }
        }

        private void PlacePart()
        {
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, BuildRange))
            {
                GameObject selectedPart = buildingParts[selectedPartIndex];

                if (hit.collider.CompareTag("Building"))
                {
                    SnapPart(selectedPart, hit.point, hit.collider.gameObject);
                    return;
                }
                
                SpawnPart(selectedPart, hit.point);
            }
        }

        private void SnapPart(GameObject selectedPart, Vector3 targetPos, GameObject snapObject)
        {
            Transform snapTransform = snapObject.transform;
            Vector3 snapCenter = snapTransform.position;
            SnapPointSetObject snapPoints = snapObject.GetComponent<BuildingPart>().SnapPointSet;

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
            snapPoints = selectedPart.GetComponent<BuildingPart>().SnapPointSet;
            snapPos -= snapRotation * snapPoints.root;

            if (Physics.CheckBox(snapPos, GetExtents(selectedPart) * SnapCollisionPadding, snapRotation, BuildingMask)) return;
            
            Instantiate(selectedPart, snapPos, snapRotation);
        }

        private void SpawnPart(GameObject selectedPart, Vector3 position)
        {
            if (Physics.CheckBox(position, GetExtents(selectedPart), Quaternion.identity, BuildingMask)) return;
            Instantiate(buildingParts[selectedPartIndex], position, quaternion.identity);
        }

        private static Vector3 GetExtents(GameObject part)
        {
            Vector3 projectionExtents = part.GetComponent<MeshFilter>().sharedMesh.bounds.extents;
            Vector3 projectionScale = part.transform.localScale;
            projectionExtents.x *= projectionScale.x;
            projectionExtents.y *= projectionScale.y;
            projectionExtents.z *= projectionScale.z;
            return projectionExtents;
        }
    }
}