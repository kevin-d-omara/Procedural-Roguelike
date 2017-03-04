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

        [SerializeField] private PathParameters essentialPathParameters;

        private Path essentialPath;
        private Dictionary<Vector2, GameObject> tiles = new Dictionary<Vector2, GameObject>();

        private void Awake()
        {
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));

            // Create copy to avoid changing original ScriptableObject
            essentialPathParameters = Object.Instantiate(essentialPathParameters);

            // Get origin and initialFacing from Player.
            essentialPathParameters.origin = Vector2.zero;
            essentialPathParameters.initialFacing = Random.Range(0f, 360f);

            essentialPath = new Path(essentialPathParameters);
        }

        private void Start()
        {
            PlotConstrainedPoints();
        }

        private void PlotConstrainedPoints()
        {
            // Mark origin.
            var origin = Instantiate(plotPoint, essentialPath.Main[0], Quaternion.identity);
            origin.GetComponent<SpriteRenderer>().color = Color.red;
            tiles.Add(Constrain(essentialPath.Main[0]), origin);

            // Plot path.
            foreach (Vector2 pt in essentialPath.Main)
            {
                var constrainedPt = Constrain(pt);

                if (!tiles.ContainsKey(constrainedPt))
                {
                    var tile = Instantiate(plotPoint, constrainedPt, Quaternion.identity);
                    tiles.Add(constrainedPt, tile);
                }
            }

            // Color inflection points.
            foreach (Vector2 pt in essentialPath.InflectionPts)
            {
                var constrainedPt = Constrain(pt);

                GameObject tile;
                if (tiles.TryGetValue(constrainedPt, out tile))
                {
                    tile.GetComponent<SpriteRenderer>().color = Color.green;
                }
            }

            // Color bottleneck points.
            foreach (Vector2 pt in essentialPath.BottleneckPts)
            {
                var constrainedPt = Constrain(pt);

                GameObject tile;
                if (tiles.TryGetValue(constrainedPt, out tile))
                {
                    tile.GetComponent<SpriteRenderer>().color = Color.magenta;
                }
            }
        }

        private Vector2 Constrain(Vector2 pt)
        {
            return new Vector2(Mathf.Round(pt.x), Mathf.Round(pt.y));
        }
    }
}
