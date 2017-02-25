using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    public class Health : MonoBehaviour
    {
        public delegate void Killed();
        public static event Killed OnKilled;

        /// <summary>
        /// Number of hit points this GameObject has.
        /// </summary>
        [Range(1, 5)]
        [SerializeField]
        private int _hitPoints;
        public int HitPoints
        {
            get { return _hitPoints; }
            set
            {
                _hitPoints = value;
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
                HitPoints -= isHardAttack ? damage : 0;
            }
            else
            {
                HitPoints -= damage;
            }
        }
    }
}
