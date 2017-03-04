using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        [HideInInspector] public Vector2 origin;

        /// <summary>
        /// Initial direction of the path, counter-clockwise. (0° == East == [+1, 0])
        /// [degrees]
        /// </summary>
        [HideInInspector] public float initialFacing;

        /// <summary>
        /// Length of the path.
        /// [meters]
        /// </summary>
        [Range(5f, 250f)]
        public float length = 50f;

        /// <summary>
        /// Curvature of the path.
        /// [degrees/meter]
        /// </summary>
        [Range(0f, 360f)]
        public float curvature = 15f;

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
        /// Number of branches in the path. Fractional values offer a chance for 1 extra branch.
        /// [# branches/path]
        /// </summary>
        [Range(0f, 1f)]
        public float branchNumber = 2.5f;

        /// <summary>
        /// Distance between steps along the path.
        /// [meters]
        /// </summary>
        [Range(0.01f, 1f)]
        public float stepSize = 0.1f;
    }
}
