using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
	{
        /// <summary>
        /// Duration of fade in/out sequence.
        /// </summary>
        public float FadeTime { get; private set; }
        [Range(0f, 10f)]
        [SerializeField] private float _fadeTime = 2f;

        private void Awake()
        {
            FadeTime = _fadeTime;
            iTween.CameraFadeAdd();
        }

        /// <summary>
        /// Fade the camera out to black.
        /// </summary>
        public void FadeOut()
        {
            iTween.CameraFadeTo(1.0f, FadeTime);
        }

        /// <summary>
        /// Fade the camera in from black.
        /// </summary>
        public void FadeIn()
        {
            iTween.CameraFadeTo(0.0f, FadeTime);
        }
    }
}
