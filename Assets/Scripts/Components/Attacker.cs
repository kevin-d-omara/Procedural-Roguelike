using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class Attacker : MonoBehaviour
	{
        public bool IsAttacking { get; private set; }

        /// <summary>
        /// Time that must be waited between making attacks.
        /// </summary>
        [Range(0f, 2f)]
        public float speed;

        /// <summary>
        /// Time after starting an attack before the damage is dealt (i.e. the wind-up).
        /// </summary>
        [Range(0f, 0.5f)]
        public float delay;

        [Range(1, 5)]
        public int damage;

        /// <summary>
        /// How far the attack raycasts to check for a target.
        /// </summary>
        [Range(0.75f, 2f)]
        public float range;

        /// <summary>
        /// True if this attack can damage hard targets (i.e. Rocks).
        /// </summary>
        [SerializeField] private bool _isHardAttack;
        public bool IsHardAttack
        {
            get { return _isHardAttack; }
            set { _isHardAttack = value; }
        }

        /// <summary>
        /// The BoxCollider2D of the attached GameObject.
        /// </summary>
        private BoxCollider2D boxCollider;

        /// <summary>
        /// Deals damage to the first target struck if it is damageable.
        /// </summary>
        /// <param name="direction">Direction to make the attack.</param>
        public void DoAttack(Vector2 direction)
        {
            if (IsAttacking) { return; }
            IsAttacking = true;

            // TODO - delay attack w/ 'delay' for "wind-up" portion of attack.

            // Raycast to check if target exits.
            boxCollider.enabled = false;
            var hit = Physics2D.Raycast(transform.position, direction, range);
            boxCollider.enabled = true;

            if (hit.transform != null)
            {
                // Check if target has Health.
                var health = hit.transform.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage, IsHardAttack);
                }
            }
        }

        private void Awake()
        {
            IsAttacking = false;
        }

        private void Start()
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }
    }
}
