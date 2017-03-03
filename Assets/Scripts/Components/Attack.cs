using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Component describing a single Attack option for the attached GameObject.
    /// </summary>
    public class Attack : MonoBehaviour
    {
        /// <summary>
        /// Denotes the start of the backswing portion of this attack.
        /// </summary>
        public delegate void StartBackswing();
        public event StartBackswing OnStartBackswing;

        /// <summary>
        /// Denotes the forward swing portion of this attack.
        /// </summary>
        public delegate void StartSwing();
        public event StartSwing OnStartSwing;

        public bool IsOnCooldown { get; private set; }

        /// <summary>
        /// Time that must be waited between making attacks.
        /// </summary>
        [Range(0f, 2f)]
        public float cooldown;

        /// <summary>
        /// Time after starting an attack before the damage is dealt (i.e. the backswing).
        /// </summary>
        [Range(0f, 0.5f)]
        public float attackDelay;

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

        private void Awake()
        {
            IsOnCooldown = false;
        }

        /// <summary>
        /// Deals damage to the first target struck if it is damageable.
        /// </summary>
        /// <param name="direction">Direction to make the attack.</param>
        /// <returns>True if attack was started,false otherwise.</returns>
        public bool DoAttack(Vector2 direction)
        {
            if (IsOnCooldown) { return false; }
            IsOnCooldown = true;
            StartCoroutine(RefreshCooldown());
            StartCoroutine(StartAttackSequence(direction));
            return true;
        }

        private IEnumerator RefreshCooldown()
        {
            yield return new WaitForSeconds(cooldown + attackDelay);
            IsOnCooldown = false;
        }

        /// <summary>
        /// First waits for the backswing to finish, then deals damage to the first target struck by
        /// a raycast.
        /// </summary>
        /// <param name="direction">X & Y directions to attack. Constrained to a unit value.</param>
        /// <returns></returns>
        private IEnumerator StartAttackSequence(Vector2 direction)
        {
            if (OnStartBackswing != null) { OnStartBackswing(); }
            yield return new WaitForSeconds(attackDelay);

            if (OnStartSwing != null) { OnStartSwing(); }

            // Constrain direction vector to unit value (-1, 0, or 1).
            direction = Utility.MakeUnitLength(direction);

            // Raycast to check if target exits.
            boxCollider.enabled = false;
            var hit = Physics2D.Raycast(transform.position, direction, range);
            boxCollider.enabled = true;

            if (hit.transform != null)
            {
                // Check if target has Health.
                var healthComponent = hit.transform.GetComponent<Health>();
                if (healthComponent != null)
                {
                    healthComponent.TakeDamage(damage, IsHardAttack);
                }
            }
        }

        private void Start()
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }
    }
}
