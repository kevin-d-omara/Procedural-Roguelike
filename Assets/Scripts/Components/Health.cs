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
        // Wrap events raised from hitPoints.
        public event LimitedQuantityInt.LostQuantity OnLostHitPoints
        {
            add    { hitPoints.OnLostQuantity += value; }
            remove { hitPoints.OnLostQuantity -= value; }
        }

        public event LimitedQuantityInt.GainedQuantity OnGainedHitPoints
        {
            add    { hitPoints.OnGainedQuantity += value; }
            remove { hitPoints.OnGainedQuantity -= value; }
        }

        public event LimitedQuantityInt.Depleted OnKilled
        {
            add    { hitPoints.OnDepleted += value; }
            remove { hitPoints.OnDepleted += value; }
        }

        /// <summary>
        /// Hit points (quantity and maximum allowed).
        /// </summary>
        public LimitedQuantityInt hitPoints;

        /// <summary>
        /// True if GameObject is immune to regular attacks (i.e. only damageable by Dynamite).
        /// </summary>
        public bool isHardTarget;

        private void Awake()
        {
            hitPoints.Quantity = hitPoints.MaxQuantity;
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
                hitPoints.Quantity -= isHardAttack ? damage : 0;
            }
            else
            {
                hitPoints.Quantity -= damage;
            }
        }
    }
}
