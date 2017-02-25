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
        /// The BoxCollider2D of the attached GameObject.
        /// </summary>
        private BoxCollider2D boxCollider;

        /// <summary>
        /// Deals damage to the first target struck if it is damageable.
        /// </summary>
        /// <param name="direction">Direction to make the attack.</param>
        /// <param name="hit">Object dealt damage or null if none.</param>
        /// <returns>True if something was dealt damage.</returns>
        public bool Attack(Vector2 direction, out RaycastHit2D hit)
        {
            // Raycast to check if target exits.
            boxCollider.enabled = false;
            hit = Physics2D.Raycast(transform.position, direction, range);
            boxCollider.enabled = true;

            if (hit.transform != null)
            {

                // check for IDamageable
                    // deal damage
                    // return true
                // return false
            }

            return false;
        }

        /// <summary>
        /// Attempts to make an attack in the specified direction.
        /// </summary>
        /// <param name="direction">Direction to make the attack.</param>
        /// <returns>True if the attack was started successfully, false otherwise.</returns>
        public bool AttemptAttack(Vector2 direction)
        {
            if (IsAttacking) { return false; }

            RaycastHit2D hit;
            bool canAttack = Attack(direction, out hit);

            if (canAttack)
            {
                IsAttacking = true;
                return true;
            }

            return true;
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
