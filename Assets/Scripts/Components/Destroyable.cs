using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class Destroyable : MonoBehaviour
	{
        public delegate void Killed();
        public static event Killed OnKilled;

        /// <summary>
        /// Number of hit points this GameObject has.
        /// </summary>
        [Range(1,5)]
        [SerializeField] private int _health;
        public int Health
        {
            get { return _health; }
            set
            {
                _health = value;
                if (OnKilled != null) { OnKilled(); }
            }
        }

        /// <summary>
        /// True if GameObject is immune to regular attacks (i.e. only damageable by Dynamite).
        /// </summary>
        [SerializeField] private bool _isHardTarget;
        public bool IsHardTarget
        {
            get { return _isHardTarget; }
            set { _isHardTarget = value; }
        }

        /// <summary>
        /// This GameObject takes the provided damage if correct to do so.
        /// </summary>
        /// <param name="damage">Amount of damage to take.</param>
        /// <param name="isHardAttack">True if this attack can damage hard targets.</param>
        public void TakeDamage(int damage, bool isHardAttack)
        {
            if (IsHardTarget)
            {
                Health -= isHardAttack ? damage : 0;
            }
            else
            {
                Health -= damage;
            }
        }
    }
}
