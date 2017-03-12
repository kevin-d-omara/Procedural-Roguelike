using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    /// <summary>
    /// 
    /// </summary>
    public partial class CaveManager : BoardManager
    {
        // Base class extra fields -----------------------------------------------------------------
        // Prefabs
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private GameObject bramblePrefab;

        // ScriptableObject data.
        [SerializeField] private List<WeightedSet> bottleneckObstacleSets;

        // Randomizers.
        protected WeightedRandomSet<GameObject> bottleneckObstacles
            = new WeightedRandomSet<GameObject>();

        // Randomizer parameters.
        [Range(0f, 1.0f)]
        [SerializeField]
        protected float bottleneckObstacleDensity = 0.65f;
        // -----------------------------------------------------------------------------------------

        [Header("Darkness level:")]
        [SerializeField] private Visibility ambientVisibility = Visibility.Full;
        [SerializeField] private Visibility entityVisibility  = Visibility.None;
        [SerializeField] private Visibility unobservedAmbient = Visibility.Half;
        [SerializeField] private Visibility unobservedEntity  = Visibility.None;

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
        /// Location of the entrance to this cave. This is where the Player spawns.
        /// </summary>
        private Vector2 caveEntrance;

        /// <summary>
        /// Set of all positions which make up the cave floor. No cave tiles may be placed outside
        /// of this set (i.e. 
        /// </summary>
        private HashSet<Vector2> caveFloor = new HashSet<Vector2>();

        /// <summary>
        /// Set of all positions which have an entity (obstacle, enemy, etc.).
        /// </summary>
        private HashSet<Vector2> caveEntity = new HashSet<Vector2>();

        /// <summary>
        /// Set of all positions on all essential paths.
        /// </summary>
        private HashSet<Vector2> caveEssentialPath = new HashSet<Vector2>();

        /// <summary>
        /// Set of all tiles with zero choke. Don't place blocking entities on these tiles.
        /// </summary>
        private HashSet<Vector2> zeroChokeTiles = new HashSet<Vector2>();

        /// <summary>
        /// Set of all bottleneck regions
        /// </summary>
        private List<Bounds> bottleneckRegions = new List<Bounds>();

        /// <summary>
        /// Tiles which have been illuminated by a light source.
        /// </summary>
        private Dictionary<Vector2, Visibility> lightMap = new Dictionary<Vector2, Visibility>();

        /// <summary>
        /// Pairs a tile's coordinates with it's choke and facing.
        /// </summary>
        private class Tile
        {
            public Vector2 position;
            public float facing;
            public int choke;

            public Tile(Vector2 position, float facing)
            {
                this.position = position;
                this.facing = facing;
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
            public List<Vector2> forkTiles = new List<Vector2>();
            public List<Vector2> chamberTiles = new List<Vector2>();

            // Coordinates of each chamber tile.
            public Dictionary<Vector2, List<Vector2>> chamberRegions
                = new Dictionary<Vector2, List<Vector2>>();
        }

        protected override void Awake()
        {
            base.Awake();

            // Select a single bottleneck set for this CaveManager instance.
            var bottleneckSet = bottleneckObstacleSets[Random.Range(0,
                bottleneckObstacleSets.Count)];

            // Transform each WeightedSet into a WeightedRandomSet
            foreach (WeightedPairGO pair in bottleneckSet.list)
            {
                bottleneckObstacles.Add(pair.item, pair.weight);
            }

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
                pathParameters[i].chamberItemNumber.SetValue();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Moveable.OnStartMove += UpdateEntityVisibility;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Moveable.OnStartMove -= UpdateEntityVisibility;
        }

        // Cave creation ---------------------------------------------------------------------------

        /// <summary>
        /// Creates all the GameObjects defined by the cave system (i.e. Floor, Obstacles, Enemies,
        /// Chests, etc.).
        /// </summary>
        /// <param name="location">Location in the world to center the entrance passage.</param>
        public void SetupCave(Vector2 position)
        {
            // DEBUG:
            SetRandomState();

            // Create logical cave
            caveEntrance = position;
            InstantiateCave();
            RecordEssentialPathAndDetails();
            SmoothDiagonalSteps();
            RecordChamberTiles();
            WidenPaths();
            BakeLightMap();

            // Create physical cave
            LayCaveFloor();
            // (optional) randomly pick entrance and exit locations (@ ForkPts and/or TerminusPts)
            LayPassages();
            LayBottleneckObstacles();
            // SpawnGems()
            PopulateChambers();
            SpawnEntities();
            UnblockEssentialPath();


            // DEBUG:
            //PlotPaintedCave(false);
        }

        // Runtime functions -----------------------------------------------------------------------

        /// <summary>
        /// Change visibility of moving object to lightmap value at destination.
        /// </summary>
        private void UpdateEntityVisibility(GameObject movingObject, Vector2 destination)
        {
            var visibleComponenet = movingObject.GetComponent<Visible>();
            if (visibleComponenet != null)
            {
                Visibility _;
                // Move into light (or half-light).
                if (lightMap.TryGetValue(destination, out _))
                {
                    visibleComponenet.VisibilityLevel = lightMap[destination];
                }
                // Move into darkness.
                else
                {
                    visibleComponenet.VisibilityLevel = Visibility.None;
                }
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
            foreach (Vector2 offset in offsets)
            {
                var position = location + offset;

                // Update light map.
                Visibility _;
                if (lightMap.TryGetValue(position, out _))
                {
                    lightMap[position] = Visibility.Full;
                }
                else
                {
                    lightMap.Add(position, Visibility.Full);
                }

                // Make pre-existing tiles visible.
                if (caveFloor.Contains(position))
                {
                    var gameObjects = Utility.FindObjectsAt(position);
                    foreach (GameObject gObject in gameObjects)
                    {
                        var visibleComponenet = gObject.GetComponent<Visible>();
                        if (visibleComponenet != null)
                        {
                            visibleComponenet.VisibilityLevel = Visibility.Full;
                        }
                    }
                }
                // Make non-existing tiles Rocks.
                else
                {
                    AddFloorTile(position);
                    caveFloor.Add(position);
                    AddTile(rockPrefab, position, holders["Obstacles"], Visibility.Full);
                }
            }
        }

        /// <summary>
        /// Increase illumination of tiles near the end location and reduce illumination of tiles
        /// near the start location. Use when a light source moves.
        /// </summary>
        public override void RevealDarkness(Vector2 startLocation, List<Vector2> startOffsets,
                                            Vector2 endLocation, List<Vector2> endOffsets)
        {
            // Reduce illumination in starting location.
            foreach (Vector2 offset in startOffsets)
            {
                var position = startLocation + offset;

                // Update light map.
                Visibility _;
                if (lightMap.TryGetValue(position, out _))
                {
                    lightMap[position] = unobservedEntity;
                }

                // Make pre-existing tiles un-illuminated.
                if (caveFloor.Contains(position))
                {
                    var gameObjects = Utility.FindObjectsAt(position);
                    foreach (GameObject gObject in gameObjects)
                    {
                        var visibleComponenet = gObject.GetComponent<Visible>();
                        if (visibleComponenet != null)
                        {
                            switch(visibleComponenet.ObjectType)
                            {
                                case Visible.Type.Ambient:
                                    visibleComponenet.VisibilityLevel = unobservedAmbient;
                                    break;
                                case Visible.Type.Entity:
                                    visibleComponenet.VisibilityLevel = unobservedEntity;
                                    break;
                                default:
                                    throw new System.ArgumentException("Unsupported Visible.Type.");
                            }
                        }
                    }
                }
            }

            // Increase illumination in ending location.
            RevealDarkness(endLocation, endOffsets);
        }

        /// <summary>
        /// Create a floor and rock tile. Set the visibility to none or lightmap.
        /// </summary>
        protected override void OnTileNotFound(Vector2 position)
        {
            var floorTile = AddFloorTile(position);
            Visibility visibility;
            if (lightMap.TryGetValue(position, out visibility)) { }
            else
            {
                visibility = Visibility.None;
            }
            floorTile.GetComponent<Visible>().VisibilityLevel = visibility;
            AddTile(rockPrefab, position, holders["Obstacles"], visibility);
            caveFloor.Add(position);
        }

        /// <summary>
        /// Creates a new tile at the position specified and add it to the list of cave entities.
        /// </summary>
        private GameObject AddTile(GameObject prefab, Vector2 position, Transform holder,
            Visibility visibility)
        {
            var addedTile = base.AddTile(prefab, position, holder);
            caveEntity.Add(position);

            addedTile.GetComponent<Visible>().VisibilityLevel = visibility;

            return addedTile;
        }

        protected override GameObject AddFloorTile(Vector2 position)
        {
            var addedTile = base.AddFloorTile(position);
            addedTile.GetComponent<Visible>().VisibilityLevel = ambientVisibility;

            return addedTile;
        }

        // Utility functions -----------------------------------------------------------------------

        /// <summary>
        /// Determine if a point is inside a bottleneck region.
        /// </summary>
        /// <returns>True if the point is inside a bottleneck region, false otherwise.</returns>
        private bool IsInBottleneckRegion(Vector2 point)
        {
            foreach (Bounds bound in bottleneckRegions)
            {
                if (bound.Contains(point)) { return true; }
            }
            return false;
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

                if (caveFloor.Contains(pt))
                {
                    if (!featureTiles.Contains(pt))
                    {
                        featureTiles.Add(pt);
                    }
                }
            }
        }

        /// <summary>
        /// Wire up the set of Feature Points (Vector2) to the Feature Tiles.
        /// </summary>
        /// <param name="featurePoints">i.e. pathInfo.path.ChamberPts</param>
        /// <param name="featureTiles">i.e. pathInfo.chamberTiles</param>
        private void MarkFeaturePoints(List<Vector2> featurePoints, List<Vector2> featureTiles)
        {
            foreach (Vector2 featurePt in featurePoints)
            {
                var pt = Constrain(featurePt);

                if (caveFloor.Contains(pt))
                {
                    if (!featureTiles.Contains(pt))
                    {
                        featureTiles.Add(pt);
                    }
                }
            }
        }

        // Procedural subroutines. Each is called once during SetupCave() --------------------------

        /// <summary>
        /// Instantiate the Path's belonging to this cave.
        /// </summary>
        /// <param name="location">Location in the world to center the entrance passage.</param>
        private void InstantiateCave()
        {
            // Instantiate paths for entire cave system.
            pathParameters[0].origin = caveEntrance;
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
        }

        /// <summary>
        /// Beginning with the top level, record the locations of the essential path and feature
        /// tiles. Also record the bounds of each bottleneck region.
        /// </summary>
        private void RecordEssentialPathAndDetails()
        {
            // Record essential path and feature tiles for each level before descending to the next.
            for (int lvl = 0; lvl < level.Length; ++lvl)
            {
                foreach (PathInfo pathInfo in level[lvl])
                {
                    // Record essential path locations.
                    var points = pathInfo.path.Main;
                    for (int i = 0; i < points.Length; ++i)
                    {
                        var pt = Constrain(points[i].Pt);
                        if (!caveFloor.Contains(pt))
                        {
                            caveFloor.Add(pt);
                            caveEssentialPath.Add(pt);
                            pathInfo.tiles.Add(new Tile(pt, points[i].Facing));
                        }
                    }

                    // Record feature tile locations.
                    if (pathInfo.tiles.Count > 0)
                    {
                        pathInfo.originTile = Constrain(pathInfo.path.Main[0].Pt);
                        pathInfo.terminusTile = Constrain(pathInfo.path.Main[
                            pathInfo.path.Main.Length - 1].Pt);
                    }
                    MarkFeaturePoints(pathInfo.path.InflectionPts, pathInfo.inflectionTiles);
                    MarkFeaturePoints(pathInfo.path.BottleneckPts, pathInfo.bottleneckTiles);
                    MarkFeaturePoints(pathInfo.path.ForkPts, pathInfo.forkTiles);
                    MarkFeaturePoints(pathInfo.path.ChamberPts, pathInfo.chamberTiles);

                    // Record bottleneck regions.
                    foreach (Vector2 bottleneckPt in pathInfo.bottleneckTiles)
                    {
                        var bounds = (pathParameters[lvl].bottleneck.Value * 2) + 1;
                        if (bounds > 0)
                        {
                            bottleneckRegions.Add(new Bounds(bottleneckPt,
                                new Vector3(bounds, bounds, bounds)));

                            // DEBUG: record bottlenecks
                            if (!featurePlots.ContainsKey(bottleneckPt)) { featurePlots.Add(bottleneckPt, Feature.Bottleneck); }
                        }
                    }

                    // DEBUG: record forks
                    foreach (Vector2 forkPt in pathInfo.forkTiles) { if (!featurePlots.ContainsKey(forkPt)) { featurePlots.Add(forkPt, Feature.Fork); } }
                }
            }
        }

        /// <summary>
        /// Thicken path along diagonal crossing points. The player cannot move diagonally, so this
        /// ensures he/she can follow the essential path.
        /// </summary>
        private void SmoothDiagonalSteps()
        {
            for (int lvl = 0; lvl < level.Length; ++lvl)
            {
                foreach (PathInfo pathInfo in level[lvl])
                {
                    var lengthOfPath = pathInfo.tiles.Count;
                    if (lengthOfPath < 2) { continue; }

                    Vector2 prev = pathInfo.tiles[0].position;
                    for (int i = 1; i < lengthOfPath; ++i)
                    {
                        var curr = pathInfo.tiles[i].position;

                        var dxAbs = (int)Mathf.Abs(curr.x - prev.x);
                        var dyAbs = (int)Mathf.Abs(curr.y - prev.y);

                        // Thicken essential path between diagonal steps.
                        if (dxAbs == 1 && dyAbs == 1)
                        {
                            var dx = (int)(curr.x - prev.x);
                            var dy = (int)(curr.y - prev.y);

                            if (Random.Range(0.0f, 1.0f) <= 0.5f) { dx = 0; }
                            else                                  { dy = 0; }
                            var newVec = prev + new Vector2(dx, dy);
                                caveEssentialPath.Add(newVec);
                        }

                        prev = curr;
                    }
                }
            }
        }

        /// <summary>
        /// Record the location of all tiles in all chambers.
        /// </summary>
        private void RecordChamberTiles()
        {
            for (int lvl = 0; lvl < level.Length; ++lvl)
            {
                foreach (PathInfo pathInfo in level[lvl])
                {
                    foreach (Vector2 chamberPt in pathInfo.chamberTiles)
                    {
                        var sizeOfRegion = pathParameters[lvl].chamber.Value;
                        var region = GridAlgorithms.GetCircularOffsets(sizeOfRegion);
                        var regionTiles = new List<Vector2>();
                        foreach (Vector2 offset in region)
                        {
                            var point = chamberPt + offset;
                            if (!caveFloor.Contains(point)) { caveFloor.Add(point); }
                            regionTiles.Add(point);
                        }
                        pathInfo.chamberRegions.Add(chamberPt, regionTiles);


                        // DEBUG: record chambers
                        if (!featurePlots.ContainsKey(chamberPt)) { featurePlots.Add(chamberPt, Feature.Chamber); }
                    }
                }
            }
        }

        /// <summary>
        /// Make each essential path wider according to the choke parameter for its level.
        /// </summary>
        private void WidenPaths()
        {
            // Record widened path.
            var directions = new char[] { 'H', 'D', 'V', 'D', 'H', 'D', 'V', 'D', 'H' };
            for (int lvl = 0; lvl < level.Length; ++lvl)
            {
                foreach (PathInfo pathInfo in level[lvl])
                {
                    var numTiles = pathInfo.tiles.Count;
                    for (int i = 0; i < numTiles; ++i)
                    {
                        var choke = pathParameters[lvl].choke.Value;

                        var degrees = (int)(pathInfo.tiles[i].facing * Mathf.Rad2Deg);
                        degrees = Mathf.Abs(degrees) % 360;
                        var index = (degrees + 23) / 45;

                        var centerPt = pathInfo.tiles[i].position;
                        if (choke == 0) { zeroChokeTiles.Add(centerPt); }

                        switch (directions[index])
                        {
                            // horizontal
                            case 'H':
                                for (int y = -choke; y <= choke; ++y)
                                {
                                    var newPt = centerPt + new Vector2(0, y);
                                    if (!caveFloor.Contains(newPt)) { caveFloor.Add(newPt); }
                                }
                                break;

                            // vertical
                            case 'V':
                                for (int x = -choke; x <= choke; ++x)
                                {
                                    var newPt = centerPt + new Vector2(x, 0);
                                    if (!caveFloor.Contains(newPt)) { caveFloor.Add(newPt); }
                                }
                                break;

                            // diagonal
                            default:
                                var region = GridAlgorithms.GetCircularOffsets(choke);

                                foreach (Vector2 offset in region)
                                {
                                    var newPt = centerPt + offset;
                                    if (!caveFloor.Contains(newPt)) { caveFloor.Add(newPt); }
                                }
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initialize the light map to entity visibility level.
        /// </summary>
        private void BakeLightMap()
        {
            foreach (Vector2 position in caveFloor)
            {
                Visibility visibility;
                if (lightMap.TryGetValue(position, out visibility))
                {
                    visibility = entityVisibility;
                }
                else
                {
                    lightMap.Add(position, visibility = entityVisibility);
                }
            }
        }

        /// <summary>
        /// Instantiates floor tiles to lay the cave footprint.
        /// </summary>
        private void LayCaveFloor()
        {
            foreach (Vector2 pt in caveFloor)
            {
                AddFloorTile(pt);
            }
        }

        /// <summary>
        /// Instantiates the entrance and exit passages.
        /// TODO: multiple exits
        /// </summary>
        private void LayPassages()
        {
            // Create the cave entrance, then collapse it!
            var passage = AddTile(passagePrefab, caveEntrance, holders["Passages"],
                Visibility.Full);
            var pController = passage.GetComponent<PassageController>();
            pController.HasBeenUsed = true;

            // Create the cave exit.
            var exitPt = level[0][0].terminusTile;
            AddTile(passagePrefab, exitPt, holders["Passages"], entityVisibility);
        }

        /// <summary>
        /// Instantiates obstacles within all bottleneck regions.
        /// </summary>
        private void LayBottleneckObstacles()
        {
            foreach (Bounds bound in bottleneckRegions)
            {
                var centerPt = (Vector2)bound.center;
                var radius = (int)bound.extents.x;
                var region = GridAlgorithms.GetCircularOffsets(radius);

                // TODO: add jitter to *outer* size
                foreach (Vector2 offset in region)
                {
                    var position = centerPt + offset;

                    if (Random.value < bottleneckObstacleDensity && !caveEntity.Contains(position))
                    {
                        if (!caveEssentialPath.Contains(position))
                        {
                            AddTile(bottleneckObstacles.RandomItem(), position,
                                holders["Obstacles"], ambientVisibility);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Instantiates items in two locations: (1) in chambers and (2) randomly.
        /// </summary>
        private void PopulateChambers()
        {
            // Spawn loot in chambers.
            for (int lvl = 0; lvl < level.Length; ++lvl)
            {
                foreach (PathInfo pathInfo in level[lvl])
                {
                    foreach (KeyValuePair<Vector2, List<Vector2>> chamberRegion in pathInfo.chamberRegions)
                    {
                        var numItems = pathParameters[lvl].chamberItemNumber.Value;
                        var numEnemies = pathParameters[lvl].chamberItemNumber.Value;
                        if (numItems == 0 || numEnemies == 0) { continue; }

                        // Collect valid placement locations.
                        var validPts = new List<Vector2>();
                        foreach (Vector2 pt in chamberRegion.Value)
                        {
                            if (!caveEntity.Contains(pt) && !zeroChokeTiles.Contains(pt))
                            {
                                validPts.Add(pt);
                            }
                        }

                        // Spawn loot!
                        for (int i = 0; i < numItems; ++i)
                        {
                            if (validPts.Count == 0) { break; }
                            var randIdx = Random.Range(0, validPts.Count);
                            var randPt = validPts[randIdx];

                            AddTile(items.RandomItem(), randPt, holders["Items"], entityVisibility);

                            validPts.Remove(randPt);
                        }

                        // Spawn enemies!!
                        for (int i = 0; i < numEnemies; ++i)
                        {
                            if (validPts.Count == 0) { break; }
                            var randomIndex = Random.Range(0, validPts.Count);
                            var randomPt = validPts[randomIndex];

                            AddTile(enemies.RandomItem(), randomPt, holders["Enemies"],
                                entityVisibility);

                            validPts.Remove(randomPt);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fills the dungeon with random passages, obstacles, items, and enemies, according to the
        /// values this instance has.
        /// </summary>
        private void SpawnEntities()
        {
            foreach (Vector2 position in caveFloor)
            {
                if (!caveEntity.Contains(position))
                {
                    // Place only non-blocking entities in tight pathways.
                    if (zeroChokeTiles.Contains(position))
                    {
                        if (Random.Range(0f, 1f) < passageDensity)
                        {
                            AddTile(passagePrefab, position, holders["Passages"], entityVisibility);
                        }
                        else if (Random.Range(0f, 1f) < obstacleDensity)
                        {
                            AddTile(bramblePrefab, position, holders["Obstacles"],
                                ambientVisibility);
                        }
                    }
                    // Wide pathways can have anything.
                    else
                    {
                        if (Random.Range(0f, 1f) < passageDensity)
                        {
                            AddTile(passagePrefab, position, holders["Passages"], entityVisibility);
                        }
                        else if (Random.Range(0f, 1f) < itemDensity)
                        {
                            AddTile(items.RandomItem(), position, holders["Items"],
                                entityVisibility);
                        }
                        else if (Random.Range(0f, 1f) < enemyDensity)
                        {
                            AddTile(enemies.RandomItem(), position, holders["Enemies"],
                                entityVisibility);
                        }
                        else if (Random.Range(0f, 1f) < obstacleDensity)
                        {
                            AddTile(obstacles.RandomItem(), position, holders["Obstacles"],
                                ambientVisibility);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove all blocking objects along the essential path.
        /// </summary>
        private void UnblockEssentialPath()
        {
            foreach (Vector2 position in caveEssentialPath)
            {
                var gameObjects = Utility.FindObjectsAt(position);
                foreach (GameObject gObject in gameObjects)
                {
                    if (gObject.tag != "Floor" && gObject.tag != "Bramble" &&
                        gObject.tag != "Passage" && gObject.tag != "Player")
                    {
                        Destroy(gObject);
                    }
                }
            }
        }
    }
}
