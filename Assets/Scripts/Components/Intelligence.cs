using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class Intelligence : MonoBehaviour
	{
        public delegate void MakeDecision();
        public event MakeDecision OnMakeDecision;

        public bool IsOnCooldown { get; private set; }

        /// <summary>
        /// Time that must be waited between making decisions.
        /// </summary>
        public float cooldown = 0.1f;

        /// <summary>
        /// Maximum distance from Player to still take actions.
        /// </summary>
        public float maxDistance = 5f;

        /// <summary>
        /// Maximum distance from Player to draw murderous attention.
        /// </summary>
        public float threatDistance = 2f;

        /// <summary>
        /// GameObject to pursue and fight;
        /// </summary>
        [HideInInspector] public GameObject target;

        protected virtual void Awake()
        {
            IsOnCooldown = false;
        }

        protected virtual void Start()
        {
            target = GameManager.Instance.Player;
            StartCoroutine(Think());
        }

        /// <summary>
        /// Begin a repeating loop which broadcasts "OnMakeDecision" each 'cooldown' seconds.
        /// </summary>
        public IEnumerator Think()
        {
            while (true)
            {
                if (OnMakeDecision != null) { OnMakeDecision(); }
                IsOnCooldown = true;

                yield return new WaitForSeconds(cooldown);

                IsOnCooldown = false;
            }
        }
	}
}
