using UnityEngine;

namespace Game.Scripts
{
    public class PlayerBuilding : MonoBehaviour
    {
        private const float BuildRange = 10f;
        private const float SnapCollisionPadding = 0.5f;

        public GameObject[] buildingParts;
        private int selectedPartIndex;
        private GameObject selectedPart;
        private SnapPointSetObject selectedSnapPointSet;
        
        private static int BuildingMask;
        private static int BuildOnMask;

        private Camera cam;

        private void Start()
        {
            cam = Camera.main;
            
            if (BuildingMask == 0) BuildingMask = LayerMask.GetMask("Building");
            if (BuildOnMask == 0) BuildOnMask = LayerMask.GetMask("Building", "Ground", "Obstacle");
            
            UpdateSelectedPart(0);
        }

        private void Update()
        {
            if (Input.GetButtonDown("Fire2"))
            {
                PlacePart();
            }

            if (Input.GetButtonDown("Fire3"))
            {
                RemovePart();
            }

            if (Input.GetButtonDown("Jump"))
            {
                UpdateSelectedPart();
            }
        }

        private void RemovePart()
        {
            if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, BuildRange)) return;
            if (!hit.collider.CompareTag("Building")) return;
            
            Destroy(hit.collider.gameObject);
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
            
            Instantiate(selectedPart, snapPos, snapRotation);
        }

        private void SpawnPart(Vector3 position)
        {
            if (Physics.CheckBox(position, GetExtents(selectedSnapPointSet), Quaternion.identity, BuildingMask)) return;
            Instantiate(buildingParts[selectedPartIndex], position, selectedPart.transform.rotation);
        }

        private static Vector3 GetExtents(SnapPointSetObject snapPointSet)
        {
            print(snapPointSet.bounds);
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