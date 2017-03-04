using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    /// <summary>
    /// A set of points and feature points describing a path in a dungeon.
    /// </summary>
	public class Path
	{
        /// <summary>
        /// Point along the Main path which is notable for some feature (inflection, branch, etc.).
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
        /// Points creating the main path and on which feature points sit.
        /// </summary>
		public Vector2[] Main { get; private set; }

        public List<FeaturePoint> InflectionPts { get; private set; }
        public List<FeaturePoint> BottleneckPts { get; private set; }
        public List<FeaturePoint> BranchPts { get; private set; }

        /// <summary>
        /// Pointer to child offshoot branches.
        /// </summary>
        public List<Path> Branches { get; private set; }

        /// <summary>
        /// Recursively creates a full tree from the list of parameters. Each set of parameters is
        /// used for a given branch level. The deepest level will be either (A) the # of parameters
        /// or (B) the level on which all branches terminate (have no branch points).
        /// </summary>
        /// <param name="pList">Parameters for each branch level.</param>
        public Path(List<PathParameters> parameterList) : this(parameterList, 0) { }

        /// <summary>
        /// Recursively creates the tree.
        /// </summary>
        /// <param name="index">Index into parameter list for the current level.</param>
        private Path(List<PathParameters> parameterList, int index)
        {
            if (parameterList.Count <= 0)
            {
                throw new ArgumentException("parameterList cannot be empty");
            }

            // Create copy to avoid changing original ScriptableObject
            parameterList[index] = UnityEngine.Object.Instantiate(parameterList[index]);

            // Create main path.
            CreatePath(parameterList[index]);

            // Terminating level reached.
            if (index == parameterList.Count - 1) { return; }

            // Recursively created child branches.
            foreach (FeaturePoint featurePt in BranchPts)
            {
                parameterList[index + 1].origin = featurePt.Pt;
                parameterList[index + 1].InitialFacing = Random.Range(0f, 360f);

                Branches.Add(new Path(parameterList, index + 1));
            }
        }

        /// <summary>
        /// Creates a single path along with its feature points.
        /// </summary>
        public Path(PathParameters parameters)
        {
            CreatePath(parameters);
        }

        /// <summary>
        /// Creates a single path along with its feature points.
        /// </summary>
        private void CreatePath(PathParameters p)
        {
            // Allocate storage.
            Main = new Vector2[Mathf.RoundToInt(p.length * 1f / p.stepSize)];
            Main[0] = p.origin;
            InflectionPts = new List<FeaturePoint>();
            BottleneckPts = new List<FeaturePoint>();
            BranchPts = new List<FeaturePoint>();
            Branches = new List<Path>();

            // Convert degrees to radians.
            p.InitialFacing *= Mathf.Deg2Rad;

            // Scale parameters to stepSize.
            var inflectionChance = p.inflectionRate * p.stepSize;
            var bottleneckChance = p.bottleneckRate * p.stepSize;
            var dTheta = p.Curvature * p.stepSize;

            // Create path.
            var theta = p.InitialFacing;
            for (int i = 1; i < Main.Length; ++i)
            {
                // Inflection points.
                if (Random.value < inflectionChance)
                {
                    dTheta = -dTheta;
                    InflectionPts.Add(new FeaturePoint(Main[i - 1], i));
                }
                // Bottleneck points.
                else if (Random.value < bottleneckChance)
                {
                    BottleneckPts.Add(new FeaturePoint(Main[i - 1], i));
                }

                // Move one step along the path.
                theta += dTheta;
                var dV = new Vector2(p.stepSize * Mathf.Cos(theta), p.stepSize * Mathf.Sin(theta));
                Main[i] = Main[i - 1] + dV;
            }

            // Mark branch points. Must have at least 2 inflection points.
            if (InflectionPts.Count > 1)
            {
                var branchNumber = Mathf.FloorToInt(p.branchNumber);
                branchNumber += Random.value < (p.branchNumber % 1) ? 1 : 0;

                for (int i = 0; i < branchNumber; ++i)
                {
                    // Branch points are always midway between two inflection points.
                    var rIdx = Random.Range(0, InflectionPts.Count - 1);
                    var index1 = InflectionPts[rIdx].Index;
                    var index2 = InflectionPts[rIdx + 1].Index;
                    var midIndex = (index2 + index1) / 2;

                    BranchPts.Add(new FeaturePoint(Main[midIndex], midIndex));
                }
            }
        }
	}
}
