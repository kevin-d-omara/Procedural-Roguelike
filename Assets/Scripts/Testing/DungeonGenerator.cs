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

        [SerializeField] private List<PathParameters> pathParameters;
        private Path dungeon;

        private Dictionary<Path, Dictionary<Vector2, GameObject>> Tiles = new Dictionary<Path, Dictionary<Vector2, GameObject>>();

        private void Awake()
        {
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));

            // Create copy so original Asset is not modified.
            pathParameters[0] = UnityEngine.Object.Instantiate(pathParameters[0]);

            // Get origin and initialFacing from Player.
            pathParameters[0].origin = Vector2.zero;
            pathParameters[0].InitialFacing = Random.Range(0f, 360f);

           // Create essential path.
           dungeon = new Path(pathParameters);
        }

        private void Start()
        {
            RecursivelyPlotConstrainedPoints(dungeon);
        }

        private void RecursivelyPlotConstrainedPoints(Path path)
        {
            Tiles.Add(path, new Dictionary<Vector2, GameObject>());
            PlotConstrainedPoints(path, Tiles[path]);

            foreach (Path p in path.Forks)
            {
                RecursivelyPlotConstrainedPoints(p);
            }
        }

        private void PlotConstrainedPoints(Path path, Dictionary<Vector2, GameObject> tiles)
        {
            // Mark origin.
            var origin = Instantiate(plotPoint, Constrain(path.Main[0].Pt), Quaternion.identity);
            origin.GetComponent<SpriteRenderer>().color = Color.red;
            tiles.Add(Constrain(path.Main[0].Pt), origin);

            // Plot path.
            PlotPoints(path.Main, Color.white, tiles);

            // Color inflection points.
            ColorFeaturePoints(path.InflectionPts, Color.yellow, tiles);

            // Color bottleneck points.
            ColorFeaturePoints(path.BottleneckPts, Color.magenta, tiles);

            // Color chamber points.
            ColorPoints(path.ChamberPts, Color.blue, tiles);

            // Color fork points.
            ColorFeaturePoints(path.ForkPts, Color.red, tiles);
        }

        private void PlotPoints(Path.Point[] array, Color color, Dictionary<Vector2, GameObject> tiles)
        {
            foreach (Path.Point pt in array)
            {
                var constrainedPt = Constrain(pt.Pt);

                if (!tiles.ContainsKey(constrainedPt))
                {
                    var tile = Instantiate(plotPoint, constrainedPt, Quaternion.identity);
                    tile.GetComponent<SpriteRenderer>().color = color;
                    tiles.Add(constrainedPt, tile);
                }
            }
        }

        private void ColorPoints(List<Vector2> list, Color color, Dictionary<Vector2, GameObject> tiles)
        {
            foreach (Vector2 pt in list)
            {
                var constrainedPt = Constrain(pt);

                GameObject tile;
                if (tiles.TryGetValue(constrainedPt, out tile))
                {
                    tile.GetComponent<SpriteRenderer>().color = color;
                }
            }
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
