using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    public class CaveManager : BoardManager
    {
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
            public List<Vector2> chamberTiles    = new List<Vector2>();
            public List<Vector2> forkTiles       = new List<Vector2>();
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
            // Create set of paths for entire cave system.
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

            // --[ Fill in floor tiles for each path and chamber. ]--
            // Starting with the root level, fill in all floor tiles.

            // Place all tiles for a given level before descending to a lower level.
            for (int lvl = 0; lvl < level.Length; ++lvl)
            {
                foreach (PathInfo pathInfo in level[lvl])
                {
                    // Lay floor tiles
                    var points = pathInfo.path.Main;
                    for (int i = 0; i < points.Length; ++i)
                    {
                        var pt = Constrain(points[i]);
                        Tile tile;
                        if (!tiles.TryGetValue(pt, out tile))
                        {
                            AddFloorTile(pt);
                            tile = new Tile(pt);
                            tiles.Add(pt, tile);
                            pathInfo.tiles.Add(tile);
                        }
                    }

                    // Record feature tile locations

                    // record origin tile
                    // record terminus tile
                    // record inflection tiles
                    // record bottleneck tiles
                    // record chamber tiles
                    // record fork tiles
                }
            }



            //      Mark bottleneck regions
            //      Fill in chambers (except where overlap with bottleneck regions)
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
    }
}
