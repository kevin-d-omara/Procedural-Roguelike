using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Set of points describing a dungeon path.
    /// </summary>
	public class Path
	{
        /// <summary>
        /// Points creating the path. Discludes branches.
        /// </summary>
		public Vector2[] Main { get; private set; }

        /// <summary>
        /// Inflection points on the main path.
        /// </summary>
        public List<Vector2> InflectionPts { get; private set; }

        /// <summary>
        /// Bottleneck points on the main path.
        /// </summary>
        public List<Vector2> BottleneckPts { get; private set; }

        public Path(PathParameters p)
        {
            // Allocate storage.
            Main = new Vector2[Mathf.RoundToInt(p.length * 1f / p.stepSize)];
            Main[0] = p.origin;
            InflectionPts = new List<Vector2>();
            BottleneckPts = new List<Vector2>();

            // Convert degrees to radians.
            p.initialFacing  *= Mathf.Deg2Rad;
            p.curvature *= Mathf.Deg2Rad;

            // Scale parameters to stepSize.
            var inflectionChance = p.inflectionRate * p.stepSize;
            var bottleneckChance = p.bottleneckRate * p.stepSize;
            var dTheta = p.curvature * p.stepSize;

            var theta = p.initialFacing;
            for (int i = 2; i < Main.Length; ++i)
            {
                if (Random.value < inflectionChance)
                {
                    dTheta = -dTheta;
                    InflectionPts.Add(Main[i-1]);
                }
                else if (Random.value < bottleneckChance)
                {
                    BottleneckPts.Add(Main[i - 1]);
                }

                theta += dTheta;
                var dV = new Vector2(p.stepSize * Mathf.Cos(theta), p.stepSize * Mathf.Sin(theta));
                Main[i] = Main[i - 1] + dV;
            }
        }

        private void CreatePath()
        {

        }
	}
}
