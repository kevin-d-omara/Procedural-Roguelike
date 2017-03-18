using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Useful for holding a range of (positive) values affected by jitter.
    /// </summary>
    [Serializable]
    public class RandomKnobInt
    {
        [Range(0, 10)] public int min;
        [Range(0, 10)] public int max;

        /// <summary>
        /// Magnitude of jitter (i.e. jitterSize=2 -> value = value +/- 2;)
        /// </summary>
        [Range(0, 5)] public int jitterSize;

        /// <summary>
        /// % of measurements which are affected by jitter.
        /// </summary>
        [Range(0f, 1f)] public float jitterRate;

        /// <summary>
        /// Value of the current setting w/ a chance for jitter. Value always >= 0.
        /// </summary>
        public int Value
        {
            // Returns the current setting w/ a chance for jitter.
            get
            {
                var measurement = _value;
                if (Random.value < jitterRate)
                {
                    measurement += Random.Range(-jitterSize, jitterSize + 1);
                }
                return measurement > 0 ? measurement : 0;
            }
            private set { _value = value; }
        }
        [HideInInspector] [SerializeField] private int _value;

        /// <summary>
        /// Sets the knob to a random value in [min, max].
        /// </summary>
        public void SetValue() { Value = Random.Range(min, max + 1); }
    }

    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// Useful for holding a range of (positive) values affected by jitter.
    /// </summary>
    [Serializable]
    public class RandomKnobFloat
    {
        [Range(0f, 10f)] public float min;
        [Range(0f, 10f)] public float max;

        /// <summary>
        /// Magnitude of jitter (i.e. jitterSize=2 -> value = value +/- 2;)
        /// </summary>
        [Range(0f, 5f)] public float jitterSize;

        /// <summary>
        /// % of measurements which are affected by jitter.
        /// </summary>
        [Range(0f, 1f)] public float jitterRate;

        /// <summary>
        /// Value of the current setting w/ a chance for jitter. Value always >= 0.
        /// </summary>
        public float Value
        {
            // Returns the current setting w/ a chance for jitter.
            get
            {
                var measurement = _value;
                if (Random.value < jitterRate)
                {
                    measurement += Random.Range(-jitterSize, jitterSize);
                }
                return measurement > 0 ? measurement : 0;
            }
            private set { _value = value; }
        }
        [HideInInspector] [SerializeField] private float _value;

        /// <summary>
        /// Sets the knob to a random value in [min, max].
        /// </summary>
        public void SetValue() { Value = Random.Range(min, max + 1); }
    }
}
