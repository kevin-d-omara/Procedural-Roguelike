using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Component describing the Health of the attached GameObject.
    /// </summary>
    public class Health : MonoBehaviour
    {
        public delegate void LostHitPoints(int damage);
        public event LostHitPoints OnLostHitPoints;

        public delegate void GainedHitPoints(int hitpoints);
        public event GainedHitPoints OnGainedHitPoints;

        public delegate void Killed();
        public event Killed OnKilled;

        /// <summary>
        /// Total number of hit points this GameObject can have.
        /// </summary>
        [Range(1, 5)]
        [SerializeField] private int _maxHitPoints;
        public int MaxHitPoints
        {
            get { return _maxHitPoints; }
            set
            {
                // Increase both MaxHitPoints and HitPoints.
                var deltaHP = _maxHitPoints - HitPoints;
                _maxHitPoints = value;
                HitPoints += deltaHP;
            }
        }

        /// <summary>
        /// Number of remaining hit points. Range (-inf, MaxHitPoints]
        /// </summary>
        private int _hitPoints;
        public int HitPoints
        {
            get { return _hitPoints; }
            set
            {
                var deltaHP = _hitPoints - value;

                if (deltaHP > 0)
                {
                    // Lost hit points.
                    _hitPoints = value;
                    if (_hitPoints <= 0)
                    {
                        if (OnKilled != null) { OnKilled(); }
                    }
                    if (OnLostHitPoints != null) { OnLostHitPoints(deltaHP); }
                }
                else if (deltaHP < 0)
                {
                    // Gained hit points
                    _hitPoints = value < MaxHitPoints ? value : MaxHitPoints;
                    if (OnGainedHitPoints != null) { OnGainedHitPoints(deltaHP); }
                }
            }
        }

        /// <summary>
        /// True if GameObject is immune to regular attacks (i.e. only damageable by Dynamite).
        /// </summary>
        public bool isHardTarget;

        private void Awake()
        {
            HitPoints = MaxHitPoints;
        }

        /// <summary>
        /// This GameObject takes the provided damage if correct to do so.
        /// </summary>
        /// <param name="damage">Amount of damage to take.</param>
        /// <param name="isHardAttack">True if this attack can damage hard targets.</param>
        public void TakeDamage(int damage, bool isHardAttack)
        {
            if (isHardTarget)
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
