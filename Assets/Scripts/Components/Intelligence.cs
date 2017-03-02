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
        /// Maximum distance from target to still take actions.
        /// </summary>
        [Range(1f, 10f)]
        public float wanderDistance = 5f;

        /// <summary>
        /// Time that must be waited between making decisions while within wanderDistance to target,
        /// but outside threatDistance.
        /// </summary>
        [Range(0.01f, 5f)]
        public float wanderCooldown = 0.1f;

        /// <summary>
        /// Maximum distance from target to draw murderous attention.
        /// </summary>
        [Range(1f, 10f)]
        public float threatDistance = 2f;

        /// <summary>
        /// Time that must be waited between making decisions while within threatDistance to target.
        /// </summary>
        [Range(0.01f, 3f)]
        public float threatCooldown = 0.1f;

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
            StartCoroutine(ThinkThreadThreat());
            StartCoroutine(ThinkThreadWander());
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

                var distanceToTarget = Vector3.Distance(target.transform.position, gameObject.transform.position);
                var cooldown = distanceToTarget < threatDistance ? threatCooldown : wanderCooldown;

                yield return new WaitForSeconds(cooldown);

                IsOnCooldown = false;
            }
        }

        public IEnumerator ThinkThreadWander()
        {
            while (true)
            {
                if (!IsOnCooldown)
                {
                    if (OnMakeDecision != null) { OnMakeDecision(); }
                    IsOnCooldown = true;
                }
                yield return new WaitForSeconds(wanderCooldown);
                IsOnCooldown = false;
            }
        }

        public IEnumerator ThinkThreadThreat()
        {
            while (true)
            {
                var distanceToTarget = Vector3.Distance(target.transform.position, gameObject.transform.position);

                if (!IsOnCooldown && distanceToTarget <= threatDistance)
                {
                    if (OnMakeDecision != null) { OnMakeDecision(); }
                    IsOnCooldown = true;
                }
                yield return new WaitForSeconds(threatCooldown);
                IsOnCooldown = false;
            }
        }
    }
}
