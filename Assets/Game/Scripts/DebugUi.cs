using System;
using TMPro;
using UnityEngine;

namespace Game.Scripts
{
    public class DebugUi : MonoBehaviour
    {
        [SerializeField] private float updateTime = 0.5f;
        [SerializeField] private TMP_Text fpsText;

        private float updateTimer;

        private void Update()
        {
            updateTimer -= Time.deltaTime;

            if (!(updateTimer <= 0f)) return;
            
            updateTimer = updateTime;
            UpdateUi();
        }

        private void UpdateUi()
        {
            int fps = Mathf.RoundToInt(1f / Time.deltaTime);
            fpsText.text = $"{fps}fps";
        }
    }
}