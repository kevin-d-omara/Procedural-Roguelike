using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    public class CaveManager : BoardManager
    {
        // TODO - remove all testing code (references to PlotPoints(), plotPrefab, etc.)
        // TESTING
        public GameObject plotPrefab;

        private void PlotPoint(Vector2 position, Color color)
        {
            var plotPt = Instantiate(plotPrefab, position, Quaternion.identity);
            plotPt.GetComponent<SpriteRenderer>().color = color;
        }
        // END TESTING

        [Header("Path parameters:")]
        [SerializeField] private List<PathParameters> essentialPathParameterSet;
        [SerializeField] private List<PathParameters> majorPathParameterSet;
        [SerializeField] private List<PathParameters> minorPathParameterSet;
        [SerializeField] [Range(0f, 1f)] private float majorLevelChance = 1f;
        [SerializeField] [Range(0f, 1f)] private float minorLevelChance = 1f;

        /// <summary>
        /// Parameters for each level of the cave. [0] == level 0, [1] == level 1, etc.
        /// </summary>
        private List<PathParameters> pathParameters = new List<PathParameters>();

        /// <summary>
        /// Each element holds a PathInfo for each path on that level.
        /// </summary>
        private List<PathInfo>[] level;
        private List<Bounds> bottlenecks = new List<Bounds>();

        /// <summary>
        /// Contains already placed tiles.
        /// </summary>
        private Dictionary<Vector2, Tile> tiles = new Dictionary<Vector2, Tile>();


        protected override void Awake()
        {
            base.Awake();

            // Wire up holder GameObjects for organizational parenting.
            holders.Add("CaveExit", transform.Find("CaveExit"));

            // For each set of path parameters, pick a single one for this CaveManager instance.
            pathParameters.Add(essentialPathParameterSet[Random.Range(0,
                essentialPathParameterSet.Count)]);
            if (Random.value <= majorLevelChance)
            {
                pathParameters.Add(majorPathParameterSet[Random.Range(0,
                    majorPathParameterSet.Count)]);
            }
            if (Random.value <= minorLevelChance)
            {
                pathParameters.Add(minorPathParameterSet[Random.Range(0,
                    minorPathParameterSet.Count)]);
            }
            level = new List<PathInfo>[pathParameters.Count];
            for (int i = 0; i < level.Length; ++i) { level[i] = new List<PathInfo>(); }

            // Create copy so original Asset is not modified.
            pathParameters[0] = UnityEngine.Object.Instantiate(pathParameters[0]);

            // Set all knobs to a random value.
            for (int i = 0; i < pathParameters.Count; ++i)
            {
                pathParameters[i].choke.SetValue();
                pathParameters[i].bottleneck.SetValue();
                pathParameters[i].chamber.SetValue();
            }
        }

        /// <summary>
        /// Check the tiles surrounding 'location' and creates tile which are defined the the cave
        /// system, or Rocks otherwise.
        /// </summary>
        /// <param name="location">Location in the world to center the offsets.</param>
        /// <param name="offsets">List of offsets specifying pattern to reveal.</param>
        public override void RevealDarkness(Vector2 location, List<Vector2> offsets)
        {
            // TODO: reveal only rocks
        }

        /// <summary>
        /// Pairs a tile's coordinates with it's choke.
        /// </summary>
        private class Tile
        {
            public readonly Vector2 position;
            public int choke;

            public Tile(Vector2 position)
            {
                this.position = position;
            }
        }

        /// <summary>
        /// Holds coordinates for each floor tile and feature tile on this path.
        /// </summary>
        private class PathInfo
        {
            public Path path;
            public List<Tile> tiles = new List<Tile>();
            public Vector2 originTile;
            public Vector2 terminusTile;

            // Coordinates of each feature tile.
            public List<Vector2> inflectionTiles = new List<Vector2>();
            public List<Vector2> bottleneckTiles = new List<Vector2>();
            public List<Vector2> forkTiles       = new List<Vector2>();
            public List<Vector2> chamberTiles    = new List<Vector2>();
        }

        private Vector2 Constrain(Vector2 pt)
        {
            return new Vector2(Mathf.Round(pt.x), Mathf.Round(pt.y));
        }

        /// <summary>
        /// Creates all the GameObjects defined by the cave system (i.e. Floor, Obstacles, Enemies,
        /// Chests, etc.).
        /// </summary>
        /// <param name="location">Location in the world to center the entrance passage.</param>
        public void SetupCave(Vector2 position)
        {
            // Instantiate paths for entire cave system.
            pathParameters[0].origin = position;
            pathParameters[0].InitialFacing = Random.Range(0f, 360f);
            level[0].Add(new PathInfo());
            level[0][0].path = new Path(pathParameters);

            // Get pointers to each path on each level.
            for (int i = 1; i < level.Length; ++i)
            {
                foreach (PathInfo pathInfo in level[i - 1])
                {
                    foreach (Path path in pathInfo.path.Forks)
                    {
                        var newPathInfo = new PathInfo();
                        newPathInfo.path = path;
                        level[i].Add(newPathInfo);
                    }
                }
            }

            // Place path tiles for each level before descending to the next.
            for (int lvl = 0; lvl < level.Length; ++lvl)
            {
                foreach (PathInfo pathInfo in level[lvl])
                {
                    // Lay floor tiles
                    var points = pathInfo.path.Main;
                    for (int i = 0; i < points.Length; ++i)
                    {
                        var pt = Constrain(points[i].Pt);
                        Tile tile;
                        if (!tiles.TryGetValue(pt, out tile))
                        {
                            LayFloorTile(pt, pathInfo.tiles);
                        }
                    }

                    // Record feature tile locations
                    pathInfo.originTile = pathInfo.tiles[0].position;
                    pathInfo.terminusTile = pathInfo.tiles[pathInfo.tiles.Count - 1].position;
                    MarkFeaturePoints(pathInfo.path.InflectionPts, pathInfo.inflectionTiles);
                    MarkFeaturePoints(pathInfo.path.BottleneckPts, pathInfo.bottleneckTiles);
                    MarkFeaturePoints(pathInfo.path.ForkPts, pathInfo.forkTiles);
                    MarkFeaturePoints(pathInfo.path.ChamberPts, pathInfo.chamberTiles);

                    // Mark bottleneck regions
                    foreach (Vector2 bottleneckPt in pathInfo.bottleneckTiles)
                    {
                        var bounds = pathParameters[lvl].bottleneck.Value;
                        if (bounds > 0)
                        {
                            bottlenecks.Add(new Bounds(bottleneckPt,
                                new Vector3(bounds, bounds, bounds)));
                            PlotPoint(bottleneckPt, Color.magenta);
                        }
                    }
                }
            }

            // Fill in chambers (except where overlapped with bottleneck regions)
            for (int lvl = 0; lvl < level.Length; ++lvl)
            {
                foreach (PathInfo pathInfo in level[lvl])
                {
                    foreach (Vector2 chamberPt in pathInfo.chamberTiles)
                    {
                        PlotPoint(chamberPt, Color.blue);

                        var sizeOfRegion = pathParameters[lvl].chamber.Value;
                        var region = new LineOfSight(sizeOfRegion);
                        foreach (Vector2 offset in region.Offsets)
                        {
                            var point = chamberPt + offset;

                            Tile tile;
                            if (!tiles.TryGetValue(point, out tile))
                            {
                                var shouldLayTile = true;

                                foreach (Bounds bound in bottlenecks)
                                {
                                    if (bound.Contains(point))
                                    {
                                        shouldLayTile = false;
                                        break;
                                    }
                                }

                                if (shouldLayTile)
                                {
                                    LayFloorTile(point, pathInfo.tiles);
                                }
                            }
                        }
                    }
                }
            }

            // Widen each path.
            for (int lvl = 0; lvl < level.Length; ++lvl)
            {
                foreach (PathInfo pathInfo in level[lvl])
                {
                    for (int i = 0; i < pathInfo.tiles.Count; ++i)
                    {

                    }
                }
            }

            //      Expand essential paths via Choke & Jitter

            // --[ Fill the dungeon with loot and denizens. ]--
            //      (optional) randomly pick entrance and exit locations (@ ForkPts and/or PathEnds)
            //      Place entrance and exit tiles
            //      Spawn gems
            //      Spawn chests
            //      Spawn enemies
            //      Spawn obstacles
            //          do not place non-bramble on choke=0 tiles

        }

        /// <summary>
        /// Create a new floortile at the position specified and bookmark it in the dictionary.
        /// </summary>
        /// <param name="constrainedPt">Point which has already been constrained.</param>
        /// <param name="tileList">i.e. pathInfo.tiles</param>
        private void LayFloorTile(Vector2 constrainedPt, List<Tile> tileList)
        {
            AddFloorTile(constrainedPt);
            var tile = new Tile(constrainedPt);
            tiles.Add(constrainedPt, tile);
            tileList.Add(tile);
        }

        /// <summary>
        /// Wire up the set of Feature Points to the Feature Tiles.
        /// </summary>
        /// <param name="featurePoints">i.e. pathInfo.path.InflectionPts</param>
        /// <param name="featureTiles">i.e. pathInfo.inflectionTiles</param>
        private void MarkFeaturePoints(List<Path.FeaturePoint> featurePoints, List<Vector2> featureTiles)
        {
            foreach (Path.FeaturePoint featurePt in featurePoints)
            {
                var pt = Constrain(featurePt.Pt);

                Tile tile;
                if (tiles.TryGetValue(pt, out tile))
                {
                    if (!featureTiles.Contains(pt))
                    {
                        featureTiles.Add(pt);
                    }
                }
            }
        }

        /// <summary>
        /// Wires up the set of Feature Points (Vector2) to the Feature Tiles.
        /// </summary>
        /// <param name="featurePoints">i.e. pathInfo.path.ChamberPts</param>
        /// <param name="featureTiles">i.e. pathInfo.chamberTiles</param>
        private void MarkFeaturePoints(List<Vector2> featurePoints, List<Vector2> featureTiles)
        {
            foreach (Vector2 featurePt in featurePoints)
            {
                var pt = Constrain(featurePt);

                Tile tile;
                if (tiles.TryGetValue(pt, out tile))
                {
                    if (!featureTiles.Contains(pt))
                    {
                        featureTiles.Add(pt);
                    }
                }
            }
        }
    }
}
