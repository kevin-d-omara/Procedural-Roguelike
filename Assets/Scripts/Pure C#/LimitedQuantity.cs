using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Storage for a value which must be limited between 0 and a maximum value.
    /// </summary>
    [Serializable]
    public class LimitedQuantityInt
    {
        public delegate void LostQuantity(int deltaQuantity);
        public event LostQuantity OnLostQuantity;

        public delegate void GainedQuantity(int deltaQuantity);
        public event GainedQuantity OnGainedQuantity;

        public delegate void Depleted();
        public event Depleted OnDepleted;

        /// <summary>
        /// Maximum value this quantity may have.
        /// </summary>
        public int MaxQuantity
        {
            get { return _maxQuantity; }
            set
            {
                // Increase both MaxQuantity and Quantity.
                var deltaHP = _maxQuantity - Quantity;
                _maxQuantity = value;
                Quantity += deltaHP;
            }
        }
        [Range(1, 100)]
        [SerializeField] private int _maxQuantity;

        /// <summary>
        /// Quantity remaining.
        /// </summary>
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                var deltaQuantity = _quantity - value;

                if (deltaQuantity > 0)
                {
                    // Lose quantity.
                    _quantity = Mathf.Max(value, 0);
                    if (_quantity <= 0)
                    {
                        if (OnDepleted != null) { OnDepleted(); }
                    }
                    if (OnLostQuantity != null) { OnLostQuantity(deltaQuantity); }
                }
                else if (deltaQuantity < 0)
                {
                    // Gain quantity.
                    _quantity = value < MaxQuantity ? value : MaxQuantity;
                    if (OnGainedQuantity != null) { OnGainedQuantity(deltaQuantity); }
                }
            }
        }
        private int _quantity;
    }

    /// <summary>
    /// Storage for a value which must be limited between 0 and a maximum value.
    /// </summary>
    [Serializable]
    public class LimitedQuantityFloat
    {
        public delegate void LostQuantity(float deltaQuantity);
        public event LostQuantity OnLostQuantity;

        public delegate void GainedQuantity(float deltaQuantity);
        public event GainedQuantity OnGainedQuantity;

        public delegate void Depleted();
        public event Depleted OnDepleted;

        /// <summary>
        /// Maximum value this quantity may have.
        /// </summary>
        [SerializeField]
        public float MaxQuantity
        {
            get { return _maxQuantity; }
            set
            {
                // Increase both MaxQuantity and Quantity.
                var deltaHP = _maxQuantity - Quantity;
                _maxQuantity = value;
                Quantity += deltaHP;
            }
        }
        [Range(1.0f, 100.0f)]
        [SerializeField] private float _maxQuantity;

        /// <summary>
        /// Quantity remaining.
        /// </summary>
        public float Quantity
        {
            get { return _quantity; }
            set
            {
                var deltaQuantity = _quantity - value;

                if (deltaQuantity > 0)
                {
                    // Lose quantity.
                    _quantity = Mathf.Max(value, 0);
                    if (_quantity <= 0)
                    {
                        if (OnDepleted != null) { OnDepleted(); }
                    }
                    if (OnLostQuantity != null) { OnLostQuantity(deltaQuantity); }
                }
                else if (deltaQuantity < 0)
                {
                    // Gain quantity.
                    _quantity = value < MaxQuantity ? value : MaxQuantity;
                    if (OnGainedQuantity != null) { OnGainedQuantity(deltaQuantity); }
                }
            }
        }
        private float _quantity;
    }
}
