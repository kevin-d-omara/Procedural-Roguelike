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

        [Header("Darkness:")]
        [SerializeField]
        private Visibility ambientLight = Visibility.None;
        private bool ambientIsNotDark;

        private Dictionary<Vector2, AggregateVisibility> lightMap
          = new Dictionary<Vector2, AggregateVisibility>();

        [Header("Sight:")]
        [SerializeField]
        private Visibility previouslySeen = Visibility.Half;
        [SerializeField] private bool startsRevealed = false;
        [SerializeField] private bool hiddenEntities = true;

        /// <summary>
        /// Tiles which have been previously illuminated by a light source. Used to keep ambient
        /// objects in a minimum of dim light after being revealed (i.e. like Fog of War).
        /// </summary>
        private HashSet<Vector2> sightMap = new HashSet<Vector2>();

        [Header("Path parameters:")]
        [SerializeField]
        private List<PathParameters> essentialPathParameterSet;
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

            ambientIsNotDark = ambientLight > Visibility.None;
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
            //BakeLightMap();   // TODO: remove this unneeded line

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
        /// Fully illuminate the tiles surrounding location with a partially illuminated outer band.
        /// Spawn a Rock for tiles which are not defined by the cave system.
        /// </summary>
        /// <param name="location">Location in the world to center the light.</param>
        /// <param name="brightOffsets">Pattern of bright light to reveal.</param>
        /// /// <param name="dimOffsetsBand">Pattern of dim light to reveal.</param>
        public override void RevealDarkness(Vector2 location, List<Vector2> brightOffsets,
            List<Vector2> dimOffsetsBand)
        {
            // Fully reveal all tiles within Bright range.
            foreach (Vector2 brightOffset in brightOffsets)
            {
                var position = location + brightOffset;
                SetTileIllumination(position, Visibility.Full, true);
            }

            // Dimly reveal all tiles in the outer Dim band.
            foreach (Vector2 dimOffset in dimOffsetsBand)
            {
                var position = location + dimOffset;
                SetTileIllumination(position, Visibility.Half, true);
            }
        }

        /// <summary>
        /// Increase illumination of tiles near the end location and reduce illumination of tiles
        /// near the start location. Use when a light source moves.
        /// </summary>
        public override void RevealDarkness(
            Vector2 startLocation, List<Vector2> startBrightOffsets, List<Vector2> startDimOffsetsBand,
            Vector2 endLocation, List<Vector2> endBrightOffsets, List<Vector2> endDimOffsetsBand)
        {
            // Record position of all affected tiles.
            var startBrightPosition = GridAlgorithms.GetPositionsFrom(startBrightOffsets,  startLocation);
            var startDimPositions   = GridAlgorithms.GetPositionsFrom(startDimOffsetsBand, startLocation);
            var endBrightPosition   = GridAlgorithms.GetPositionsFrom(endBrightOffsets,    endLocation);
            var endDimPositions     = GridAlgorithms.GetPositionsFrom(endDimOffsetsBand,   endLocation);

            // Set tile illumination only where there is no overlap between start and end positions.
            SetTileIllumination(endBrightPosition, startBrightPosition, Visibility.Full, true);
            SetTileIllumination(startBrightPosition, endBrightPosition, Visibility.Full, false);
            SetTileIllumination(endDimPositions, startDimPositions, Visibility.Half, true);
            SetTileIllumination(startDimPositions, endDimPositions, Visibility.Half, false);
        }

        /// <summary>
        /// Sets lighting at each Target position which is not overlapping any Avoided position.
        /// </summary>
        /// <param name="targetPositions">Positions to attempt to light.</param>
        /// <param name="avoidedPositions">Positions to avoid lighting.</param>
        /// <param name="level">Level of light to apply.</param>
        /// <param name="isAddingContribution">True if adding light, false if removing.</param>
        private void SetTileIllumination(HashSet<Vector2> targetPositions,
                                         HashSet<Vector2> avoidedPositions,
                                         Visibility level, bool isAddingContribution)
        {
            foreach (Vector2 position in targetPositions)
            {
                if (!avoidedPositions.Contains(position))
                {
                    SetTileIllumination(position, level, isAddingContribution);
                }
            }
        }

        /// <summary>
        /// Sets all tiles at the position to the appropriate visibility level for its type.
        /// Spawn a Rock if the tile is not defined by the cave system.
        /// </summary>
        /// <param name="level">Visibility to set ambient objects to.</param>
        /// <param name="entity">Visibility to set entity objects to.</param>
        /// <param name="isAddingContribution">True if adding a contribution, false if removing a 
        /// contribution.</param>
        private void SetTileIllumination(Vector2 position, Visibility level,
            bool isAddingContribution)
        {
            // Update revealed tiles.
            if (!sightMap.Contains(position)) { sightMap.Add(position); }

            // Update light map.
            AggregateVisibility visibility;
            if (lightMap.TryGetValue(position, out visibility))
            {
                if (isAddingContribution) { visibility.AddContribution(level); }
                else { visibility.RemoveContribution(level); }
            }
            else
            {
                visibility = AddBakedToLightMap(position);

                if (isAddingContribution) { visibility.AddContribution(level); }
                else { visibility.RemoveContribution(level); }

                if (!sightMap.Contains(position) && visibility.VisibilityLevel > Visibility.None)
                {
                    sightMap.Add(position);
                }
            }

            // Update illumination of pre-existing tiles.
            if (caveFloor.Contains(position))
            {
                var gameObjects = Utility.FindObjectsAt(position);
                foreach (GameObject gObject in gameObjects)
                {
                    var visibleComponent = gObject.GetComponent<Visible>();
                    if (visibleComponent != null)
                    {
                        UpdateObjectVisibility(visibleComponent, position, visibility, true);
                    }
                }
            }
            // Make non-existing tiles Rocks.
            else
            {
                AddTile(rockPrefab, position, holders["Obstacles"]);
            }
        }

        /// <summary>
        /// Updates the visible component to match the light map and sight map.
        /// </summary>
        private void UpdateObjectVisibility(Visible component, Vector2 position)
        {
            AggregateVisibility light;
            lightMap.TryGetValue(position, out light);

            UpdateObjectVisibility(component, position, light, sightMap.Contains(position));
        }

        /// <summary>
        /// Update the visible component to match the light map and sight map.
        /// </summary>
        private void UpdateObjectVisibility(Visible component, Vector2 position,
                                            AggregateVisibility light, bool inSightMap)
        {
            switch (component.ObjectType)
            {
                case Visible.Type.Terrain:
                    component.VisibilityLevel = !inSightMap ?
                        light.VisibilityLevel :
                        (Visibility)Mathf.Max((int)previouslySeen,
                                              (int)light.VisibilityLevel);
                    break;
                case Visible.Type.Entity:
                    component.VisibilityLevel = hiddenEntities || !inSightMap ?
                        light.VisibilityLevel :
                        (Visibility)Mathf.Max((int)previouslySeen,
                                              (int)light.VisibilityLevel);
                    break;
                default:
                    throw new System.ArgumentException("Unsupported object type.");
            }
        }

        /// <summary>
        /// Change visibility of moving object to lightmap value at destination.
        /// </summary>
        private void UpdateEntityVisibility(GameObject movingObject, Vector2 destination)
        {
            var visibleComponent = movingObject.GetComponent<Visible>();
            if (visibleComponent != null)
            {
                // Beware: magic numbers ahead!
                var duration = 0.25f;
                var checks = 2;

                StartCoroutine(DoubleCheckObjectVisibility(visibleComponent, duration, checks));
            }
        }

        /// <summary>
        /// Repeatedly update object visibility for a short duration. This handles the case when:
        /// player moves -> object moves -> player moves, which can leave the object stuck in dim
        /// lighting even when it should be dark.
        /// </summary>
        /// <param name="visibleComponent">Visible component of the object to update.</param>
        private IEnumerator DoubleCheckObjectVisibility(Visible visibleComponent, float duration,
            int numberOfChecks)
        {
            // ex: duration = 1 second; checks = 3:
            //  |-----|-----|
            // 0.0   0.5   1.0
            var waitTime = numberOfChecks > 1 ? duration / (numberOfChecks - 1) : duration;

            for (int i = 0; i < numberOfChecks; ++i)
            {
                var position = Constrain(visibleComponent.transform.position);
                UpdateObjectVisibility(visibleComponent, position);

                yield return new WaitForSeconds(waitTime);
            }
        }

        /// <summary>
        /// Create a floor and rock tile. Add to the light map and sight map.
        /// </summary>
        protected override void OnTileNotFound(Vector2 position)
        {
            AddTile(rockPrefab, position, holders["Obstacles"]);
        }

        /// <summary>
        /// Creates a new tile at the position specified and add it to the list of cave entities.
        /// </summary>
        private GameObject AddTile(GameObject prefab, Vector2 position, Transform holder)
        // TODO: handle override^
        {
            if (!caveFloor.Contains(position)) { AddFloorTile(position); }

            var addedTile = base.AddTile(prefab, position, holder);
            caveEntity.Add(position);
            UpdateObjectVisibility(addedTile.GetComponent<Visible>(), position);
            return addedTile;
        }

        protected override GameObject AddFloorTile(Vector2 position)
        {
            if (!caveFloor.Contains(position)) { caveFloor.Add(position); }

            // Update light map and sight map.
            AggregateVisibility visibility;
            if (lightMap.TryGetValue(position, out visibility)) { }
            else { AddBakedToLightMap(position); }

            var addedTile = base.AddFloorTile(position);
            UpdateObjectVisibility(addedTile.GetComponent<Visible>(), position);
            return addedTile;
        }

        /// <summary>
        /// Add a new location to the light map and sight map. Initialized with baked ambient
        /// lighting.
        /// </summary>
        private AggregateVisibility AddBakedToLightMap(Vector2 position)
        {
            var visibility = new AggregateVisibility(ambientLight);
            lightMap.Add(position, visibility);

            if (startsRevealed || ambientIsNotDark) { sightMap.Add(position); }

            return visibility;
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
                            else { dy = 0; }
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
        /// Initialize light map to ambient lighting value, and optionally initialize the sight map.
        /// </summary>
        private void BakeLightMap()
        {
            foreach (Vector2 position in caveFloor)
            {
                AggregateVisibility visibility;
                if (lightMap.TryGetValue(position, out visibility)) { }
                else { AddBakedToLightMap(position); }
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
            var passage = AddTile(passagePrefab, caveEntrance, holders["Passages"]);
            var pController = passage.GetComponent<PassageController>();
            pController.HasBeenUsed = true;

            // Create the cave exit.
            var exitPt = level[0][0].terminusTile;
            AddTile(passagePrefab, exitPt, holders["Passages"]);
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
                                holders["Obstacles"]);
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

                            AddTile(items.RandomItem(), randPt, holders["Items"]);

                            validPts.Remove(randPt);
                        }

                        // Spawn enemies!!
                        for (int i = 0; i < numEnemies; ++i)
                        {
                            if (validPts.Count == 0) { break; }
                            var randomIndex = Random.Range(0, validPts.Count);
                            var randomPt = validPts[randomIndex];

                            AddTile(enemies.RandomItem(), randomPt, holders["Enemies"]);

                            validPts.Remove(randomPt);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fill the dungeon with random passages, obstacles, items, and enemies, according to the
        /// values this instance has.
        /// </summary>
        private void SpawnEntities()
        {
            // Prevent concurrent modification.
            var tmpCaveFloor = new HashSet<Vector2>(caveFloor);

            foreach (Vector2 position in tmpCaveFloor)
            {
                if (!caveEntity.Contains(position))
                {
                    // Place only non-blocking entities in tight pathways.
                    if (zeroChokeTiles.Contains(position))
                    {
                        if (Random.Range(0f, 1f) < passageDensity)
                        {
                            AddTile(passagePrefab, position, holders["Passages"]);
                        }
                        else if (Random.Range(0f, 1f) < obstacleDensity)
                        {
                            AddTile(bramblePrefab, position, holders["Obstacles"]);
                        }
                    }
                    // Wide pathways can have anything.
                    else
                    {
                        if (Random.Range(0f, 1f) < passageDensity)
                        {
                            AddTile(passagePrefab, position, holders["Passages"]);
                        }
                        else if (Random.Range(0f, 1f) < itemDensity)
                        {
                            AddTile(items.RandomItem(), position, holders["Items"]);
                        }
                        else if (Random.Range(0f, 1f) < enemyDensity)
                        {
                            AddTile(enemies.RandomItem(), position, holders["Enemies"]);
                        }
                        else if (Random.Range(0f, 1f) < obstacleDensity)
                        {
                            AddTile(obstacles.RandomItem(), position, holders["Obstacles"]);
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
