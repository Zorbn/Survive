using System;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Scripts
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerInteraction : NetworkBehaviour
    {
        [SerializeField] private ParticleSystem muzzleFlashParticles;
        [SerializeField] private AudioSource fireAudioSource;
        [SerializeField] private float weaponIdleBobPeriod = 1f;
        [SerializeField] private float weaponMovingBobPeriod = 1f;
        [SerializeField] private float weaponSprintingBobMultiplier = 1.2f;
        [SerializeField] private float weaponBobAmplitude = 0.02f;
        [SerializeField] private WeaponObject weapon;

        private Rigidbody rb;
        private Camera cam;
        private Transform camTransform;
        private PlayerMovement playerMovement;
        private Transform weaponTransform;
        private Vector3 weaponPos;
        private float bobTime;
        private float recoil;

        public override void OnStartLocalPlayer()
        {
            rb = GetComponent<Rigidbody>();
            cam = Camera.main;

            if (cam == null) throw new NullReferenceException("Camera is null!");
            
            camTransform = cam.transform;
            playerMovement = GetComponent<PlayerMovement>();

            Transform view = GameObject.Find("View").transform;

            weaponTransform = view.Find("Weapon");
            weaponPos = weaponTransform.position;

            muzzleFlashParticles = view.Find("MuzzleFlashParticles").GetComponent<ParticleSystem>();
            fireAudioSource = weaponTransform.gameObject.GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            float weaponBobPeriod = rb.velocity.magnitude > 0.1f ? weaponMovingBobPeriod : weaponIdleBobPeriod;
            if (playerMovement.isSprinting) weaponBobPeriod *= weaponSprintingBobMultiplier;
            
            bobTime += Time.deltaTime * weaponBobPeriod;
            weaponTransform.position = weaponPos + new Vector3(0f,
                Mathf.Sin(bobTime) * weaponBobAmplitude - recoil * weapon.recoilVertDistance,
                -recoil * weapon.recoilDistance);
            weaponTransform.rotation = Quaternion.Euler(recoil * weapon.recoilAngle, 180f, 0f);

            recoil = Mathf.Max(recoil - Time.deltaTime * weapon.recoilSpeed, 0f);

            if (recoil != 0f || !Input.GetButton("Fire1")) return;
            
            recoil = 1f;
            CmdFire(camTransform.position, camTransform.forward);
        }

        [Command]
        private void CmdFire(Vector3 camPosition, Vector3 camDirection)
        {
            RpcFire();

            if (!Physics.Raycast(camPosition, camDirection, out RaycastHit hit,
                    weapon.attackRange)) return;
            if (!hit.collider.CompareTag("Enemy")) return;

            var health = hit.collider.GetComponent<IHealth>();
            health.TakeDamage(weapon.attackDamage);
        }

        [ClientRpc]
        private void RpcFire()
        {
            muzzleFlashParticles.Play();
            fireAudioSource.pitch = Random.Range(0.9f, 1.0f);
            fireAudioSource.Play();
        }
    }
}