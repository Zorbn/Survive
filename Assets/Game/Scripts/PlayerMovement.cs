using System;
using Mirror;
using UnityEngine;

namespace Game.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float mouseSensitivity = 2.0f;
        [SerializeField] private float moveSpeed = 400f;
        [SerializeField] private float sprintMultiplier = 1.2f;
        [SerializeField] private float sprintFovMultiplier = 1.1f;
        [SerializeField] private float jumpForce = 6f;
        
        [SerializeField] private float cameraFollowSpeed = 40f;
        [SerializeField] private float cameraRoll = 1f;
        [SerializeField] private float cameraPitch = 2f;
        [SerializeField] private float cameraTiltSpeed = 10f;
        [SerializeField] private float cameraFovSpeed = 10f;

        private static readonly Vector3 GroundCheckHalfExtent = new(0.5f, 0.1f, 0.5f); 
        private static readonly Vector3 CameraOffset = new(0f, 0.6f, 0f);

        private Rigidbody rb;
        private Camera cam;
        private Transform camTransform;
        private Transform playerTransform;
        
        private float vertRot;
        private float horRot;
        private float camRoll;
        private float camPitch;
        private float camDefaultFov;
        
        private bool tryJump;
        public bool isSprinting { get; private set; }
        private Vector2 moveDir;
    
        public override void OnStartLocalPlayer()
        {
            cam = Camera.main;

            if (cam is null) throw new ArgumentNullException(nameof(cam));

            camDefaultFov = cam.fieldOfView;
            camTransform = cam.transform;

            playerTransform = transform;
            rb = GetComponent<Rigidbody>();

            Cursor.lockState = CursorLockMode.Locked;
        }

        private static bool PollLockState()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (Input.GetMouseButton(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }

                return false;
            }

            if (Input.GetButton("Cancel"))
            {
                Cursor.lockState = CursorLockMode.None;
            }

            return true;
        }

        private void RotateCamera()
        {
            float horizontalRotation = mouseSensitivity * Input.GetAxis("Mouse X");
            float verticalRotation = -mouseSensitivity * Input.GetAxis("Mouse Y");
            
            horRot += horizontalRotation;
            playerTransform.rotation = Quaternion.Euler(0f, horRot, 0f);
            vertRot = Math.Clamp(vertRot + verticalRotation, -89f, 89f);

            camRoll = moveDir.x switch
            {
                > 0 => Mathf.Lerp(camRoll, -cameraRoll, cameraTiltSpeed * Time.deltaTime),
                < 0 => Mathf.Lerp(camRoll, cameraRoll, cameraTiltSpeed * Time.deltaTime),
                _ => Mathf.Lerp(camRoll, 0f, cameraTiltSpeed * Time.deltaTime)
            };
            
            camPitch = moveDir.y switch
            {
                > 0 => Mathf.Lerp(camPitch, cameraPitch, cameraTiltSpeed * Time.deltaTime),
                < 0 => Mathf.Lerp(camPitch, -cameraPitch, cameraTiltSpeed * Time.deltaTime),
                _ => Mathf.Lerp(camPitch, 0f, cameraTiltSpeed * Time.deltaTime)
            };

            camTransform.rotation = Quaternion.Euler(vertRot + camPitch, horRot, camRoll);
        }

        private void Move()
        {
            Vector3 forwardVel = playerTransform.forward * moveDir.y;
            Vector3 rightVel = playerTransform.right * moveDir.x;
            Vector3 velocity = rb.velocity;
            
            float yVel = velocity.y;
            if (tryJump && Physics.BoxCast(playerTransform.position, GroundCheckHalfExtent,
                    Vector3.down, Quaternion.identity, 1.35f))
            {
                yVel = jumpForce;
            }

            float currentSpeed = moveSpeed;

            if (isSprinting && moveDir.magnitude != 0f)
            {
                currentSpeed *= sprintMultiplier;
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, camDefaultFov * sprintFovMultiplier, cameraFovSpeed * Time.deltaTime);
            }
            else
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, camDefaultFov, cameraFovSpeed * Time.deltaTime);
            }
            
            velocity = Time.deltaTime * currentSpeed * (forwardVel + rightVel);
            velocity = new Vector3(velocity.x, yVel, velocity.z);
            rb.velocity = velocity;
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            if (!PollLockState()) return;

            RotateCamera();
            
            moveDir.x = Input.GetAxis("Horizontal");
            moveDir.y = Input.GetAxis("Vertical");
            moveDir.Normalize();
            tryJump = Input.GetButton("Jump");
            isSprinting = Input.GetButton("Sprint");
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;
            
            Move();
        }

        private void LateUpdate()
        {
            if (!isLocalPlayer) return;

            camTransform.position = Vector3.Lerp(camTransform.position, playerTransform.position + CameraOffset, cameraFollowSpeed * Time.deltaTime);
        }
    }
}
