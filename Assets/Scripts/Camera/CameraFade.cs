using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    [RequireComponent(typeof(Camera))]
	public class CameraFade : MonoBehaviour
	{
        [Range(0f, 10f)]
        [SerializeField] private float fadeTime = 2f;

        private void Awake()
        {
            iTween.CameraFadeAdd();
        }

        private void OnEnable()
        {
            PassageController.OnEnterDungeon += FadeOut;
            PassageController.OnExitDungeon += FadeOut;
        }

        private void OnDisable()
        {
            PassageController.OnEnterDungeon -= FadeOut;
            PassageController.OnExitDungeon -= FadeOut;
        }

        private void FadeOut(Vector2 _)
        {
            iTween.CameraFadeTo(1.0f, fadeTime);
        }

        private void FadeIn(Vector2 _)
        {
            iTween.CameraFadeTo(0.0f, fadeTime);
        }
    }
}
