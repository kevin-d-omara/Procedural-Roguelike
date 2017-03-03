using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
	public class Intelligence : MonoBehaviour
	{
        public enum Mode { Wander, Threat, None }

        public delegate void MakeDecision(Mode mode, float distanceToTarget);
        public event MakeDecision OnMakeDecision;

        public bool IsOnCooldown { get; private set; }

        [Header("Wander:")]
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

        [Header("Threat:")]
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
        [HideInInspector] public Transform target;

        protected virtual void Awake()
        {
            IsOnCooldown = false;
        }

        protected virtual void Start()
        {
            target = GameManager.Instance.Player.transform;
            StartCoroutine(ThinkThreadThreat());
            StartCoroutine(ThinkThreadWander());
        }

        public IEnumerator ThinkThreadWander()
        {
            //yield return new WaitForSeconds(Random.Range(0f, 1f));

            while (true)
            {
                var distanceToTarget = Vector3.Distance(target.position, transform.position);

                if (!IsOnCooldown && distanceToTarget <= wanderDistance
                                  && distanceToTarget > threatDistance)
                {
                    if (OnMakeDecision != null) { OnMakeDecision(Mode.Wander, distanceToTarget); }
                    IsOnCooldown = true;
                }
                yield return new WaitForSeconds(wanderCooldown);
                IsOnCooldown = false;
            }
        }

        public IEnumerator ThinkThreadThreat()
        {
            //yield return new WaitForSeconds(Random.Range(0f, 1f));

            while (true)
            {
                var distanceToTarget = Vector3.Distance(target.position, transform.position);

                if (!IsOnCooldown && distanceToTarget <= threatDistance)
                {
                    if (OnMakeDecision != null) { OnMakeDecision(Mode.Threat, distanceToTarget); }
                    IsOnCooldown = true;
                }
                yield return new WaitForSeconds(threatCooldown);
                IsOnCooldown = false;
            }
        }
    }
}
