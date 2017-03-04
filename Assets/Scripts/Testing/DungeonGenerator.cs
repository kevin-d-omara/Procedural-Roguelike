using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Creates a dungeon based on the Inspector-assigned values.
    /// </summary>
	public class DungeonGenerator : MonoBehaviour
    {
        public GameObject plotPoint;

        [SerializeField] private List<PathParameters> branchParameters;
        private Path dungeon;

        private Dictionary<Path, Dictionary<Vector2, GameObject>> Tiles = new Dictionary<Path, Dictionary<Vector2, GameObject>>();

        private void Awake()
        {
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));

            // Create copy so original Asset is not modified.
            branchParameters[0] = UnityEngine.Object.Instantiate(branchParameters[0]);

            // Get origin and initialFacing from Player.
            branchParameters[0].origin = Vector2.zero;
            branchParameters[0].InitialFacing = Random.Range(0f, 360f);

           // Create essential path.
           dungeon = new Path(branchParameters);
        }

        private void Start()
        {
            RecursivelyPlotConstrainedPoints(dungeon);
        }

        private void RecursivelyPlotConstrainedPoints(Path path)
        {
            Tiles.Add(path, new Dictionary<Vector2, GameObject>());
            PlotConstrainedPoints(path, Tiles[path]);

            foreach (Path p in path.Branches)
            {
                RecursivelyPlotConstrainedPoints(p);
            }
        }

        private void PlotConstrainedPoints(Path path, Dictionary<Vector2, GameObject> tiles)
        {
            // Mark origin.
            var origin = Instantiate(plotPoint, Constrain(path.Main[0]), Quaternion.identity);
            origin.GetComponent<SpriteRenderer>().color = Color.red;
            tiles.Add(Constrain(path.Main[0]), origin);

            // Plot path.
            foreach (Vector2 pt in path.Main)
            {
                var constrainedPt = Constrain(pt);

                if (!tiles.ContainsKey(constrainedPt))
                {
                    var tile = Instantiate(plotPoint, constrainedPt, Quaternion.identity);
                    tile.GetComponent<SpriteRenderer>().color = Color.white;
                    tiles.Add(constrainedPt, tile);
                }
            }

            // Color inflection points.
            ColorFeaturePoints(path.InflectionPts, Color.yellow, tiles);

            // Color bottleneck points.
            ColorFeaturePoints(path.BottleneckPts, Color.magenta, tiles);

            // Color branch points.
            ColorFeaturePoints(path.BranchPts, Color.red, tiles);
        }

        private void ColorFeaturePoints(List<Path.FeaturePoint> list, Color color, Dictionary<Vector2, GameObject> tiles)
        {
            foreach (Path.FeaturePoint featurePt in list)
            {
                var constrainedPt = Constrain(featurePt.Pt);

                GameObject tile;
                if (tiles.TryGetValue(constrainedPt, out tile))
                {
                    tile.GetComponent<SpriteRenderer>().color = color;
                }
            }
        }

        private Vector2 Constrain(Vector2 pt)
        {
            return new Vector2(Mathf.Round(pt.x), Mathf.Round(pt.y));
        }
    }
}
