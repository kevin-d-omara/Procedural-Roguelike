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
        /// Point along the Main path which is notable for some feature.
        /// </summary>
        public class FeaturePoint
        {
            /// <summary>
            /// Where this point is.
            /// </summary>
            public Vector2 Pt { get; private set; }

            /// <summary>
            /// Index into Path.Main where this point is.
            /// </summary>
            public int Index { get; private set; }

            public FeaturePoint(Vector2 pt, int index)
            {
                Pt = pt;
                Index = index;
            }
        }

        /// <summary>
        /// Points creating the path. Discludes branches.
        /// </summary>
		public Vector2[] Main { get; private set; }

        /// <summary>
        /// Inflection points on the main path.
        /// </summary>
        public List<FeaturePoint> InflectionPts { get; private set; }

        /// <summary>
        /// Bottleneck points on the main path.
        /// </summary>
        public List<FeaturePoint> BottleneckPts { get; private set; }

        /// <summary>
        /// Branching points on the main path.
        /// </summary>
        public List<FeaturePoint> BranchPts { get; private set; }

        public Path(PathParameters p)
        {
            // Allocate storage.
            Main = new Vector2[Mathf.RoundToInt(p.length * 1f / p.stepSize)];
            Main[0] = p.origin;
            InflectionPts = new List<FeaturePoint>();
            BottleneckPts = new List<FeaturePoint>();
            BranchPts     = new List<FeaturePoint>();

            // Convert degrees to radians.
            p.initialFacing  *= Mathf.Deg2Rad;
            p.curvature *= Mathf.Deg2Rad;

            // Scale parameters to stepSize.
            var inflectionChance = p.inflectionRate * p.stepSize;
            var bottleneckChance = p.bottleneckRate * p.stepSize;
            var dTheta = p.curvature * p.stepSize;

            // Create path.
            var theta = p.initialFacing;
            for (int i = 2; i < Main.Length; ++i)
            {
                if (Random.value < inflectionChance)
                {
                    dTheta = -dTheta;
                    InflectionPts.Add(new FeaturePoint(Main[i-1], i));
                }
                else if (Random.value < bottleneckChance)
                {
                    BottleneckPts.Add(new FeaturePoint(Main[i - 1], i));
                }

                theta += dTheta;
                var dV = new Vector2(p.stepSize * Mathf.Cos(theta), p.stepSize * Mathf.Sin(theta));
                Main[i] = Main[i - 1] + dV;
            }

            // Mark branch points.
            var branchNumber = Mathf.FloorToInt(p.branchNumber);
            branchNumber += Random.value < (p.branchNumber % 1) ? 1 : 0;

            for (int i = 0; i < branchNumber; ++i)
            {
                int rIdx = Random.Range(0, InflectionPts.Count);
            }
        }

        private void CreatePath()
        {

        }
	}
}
