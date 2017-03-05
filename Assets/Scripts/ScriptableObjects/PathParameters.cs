using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Collection of parameters for creating a path.
    /// </summary>
    [CreateAssetMenu(fileName = "Data", menuName = "Data/Path Parameters", order = 10)]
    public class PathParameters : ScriptableObject
	{
        /// <summary>
        /// Root of the path.
        /// </summary>
        [HideInInspector] public Vector2 origin = Vector2.zero;

        /// <summary>
        /// Initial direction of the path, counter-clockwise. (0° == East == [+1, 0])
        /// [radians] {!: internally stored as degress, therefore set w/ degrees not radians.}
        /// </summary>
        public float InitialFacing
        {
            get { return _initialFacing * Mathf.Deg2Rad; }
            set { _initialFacing = value; }
        }
        // Serialized so clones keep the same value (i.e. through UnityEngine.Object.Instantiate()).
        [HideInInspector] [SerializeField] private float _initialFacing = 0f;

        /// <summary>
        /// Length of the path.
        /// [meters]
        /// </summary>
        [Range(5f, 250f)]
        public float length = 50f;

        /// <summary>
        /// Curvature of the path.
        /// [radians/meter] {!: internally stored as degress, therefore set w/ degrees not radians.}
        /// </summary>
        public float Curvature
        {
            get { return _curvature * Mathf.Deg2Rad; }
            set { _curvature = value; }
        }
        [Range(0f, 360f)]
        [SerializeField] private float _curvature = 15f;

        /// <summary>
        /// Distance between steps along the path.
        /// [meters]
        /// </summary>
        [Range(0.01f, 1f)]
        public float stepSize = 0.1f;

        [Header("Feature points:")]
        /// <summary>
        /// How often inflection points occur.
        /// [% chance of inflection point / meter]
        /// </summary>
        [Range(0f, 1f)]
        public float inflectionRate = 0.4f;

        /// <summary>
        /// How often bottleneck points occur.
        /// [% chance of bottleneck point / meter]
        /// </summary>
        [Range(0f, 1f)]
        public float bottleneckRate = 0.025f;

        /// <summary>
        /// Number of chambers in the path. Fractional values offer a chance for 1 extra chamber.
        /// [# chambers/path]
        /// </summary>
        [Range(0f, 5f)]
        public float chamberNumber = 2.5f;

        /// <summary>
        /// Number of forks in the path. Fractional values offer a chance for 1 extra fork.
        /// [# forks/path]
        /// </summary>
        [Range(0f, 5f)]
        public float forkNumber = 2.5f;

        [Header("Path size:")]
        /// <summary>
        /// The width range of the path.
        /// </summary>
        public RandomKnob choke;

        [Header("Cavity size")]
        /// <summary>
        /// The radius range of bottleneck regions.
        /// </summary>
        public RandomKnob bottleneck;

        /// <summary>
        /// The radius range of chamber rooms.
        /// </summary>
        public RandomKnob chamber;

        /// <summary>
        /// Useful for holding a range instead of single value and for defining a jitter.
        /// </summary>
        [Serializable]
        public class RandomKnob
        {
            [Range(0, 10)]
            public int min;
            [Range(0, 10)]
            public int max;

            /// <summary>
            /// Magnitude of jitter (i.e. jitterSize=2 -> value = value +/- 2;)
            /// </summary>
            [Range(0,5)]
            public int jitterSize;

            /// <summary>
            /// % of measurements which are affected by jitter.
            /// </summary>
            [Range(0f, 1f)]
            public float jitterRate;

            /// <summary>
            /// Get: current setting w/ chance for jitter.
            /// </summary>
            public int Value
            {
                // Returns the current setting w/ a chance for jitter.
                get
                {
                    var measurement = _value;
                    if (Random.value <= jitterRate)
                    {
                        measurement += Random.Range(-jitterSize, jitterSize + 1);
                    }
                    return measurement;
                }
                private set { _value = value; }
            }
            [HideInInspector] [SerializeField] private int _value;

            /// <summary>
            /// Sets the knob to a random value in [min, max].
            /// </summary>
            public void SetValue() { Value = Random.Range(min, max + 1); }
        }
    }
}
