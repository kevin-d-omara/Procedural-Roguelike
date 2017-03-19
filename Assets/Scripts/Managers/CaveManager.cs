using System;
using System.Linq;
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

        [Header("Light Shaft Parameters:")]
        [SerializeField] private LightShaftParameters lightShaft;

        [Serializable]
        private class LightShaftParameters
        {
            [Range(0f, 1f)] public float density = 0.03f;
            [Range(0f, 1f)] public float chainChance = 0.5f;
            public RandomKnobInt clusterSize;
            public RandomKnobFloat scatterDistance;
            public Illumination illumination = Illumination.Low;

            public void Initialize()
            {
                clusterSize.SetValue();
                scatterDistance.SetValue();
            }
        }

        [Header("Darkness:")]
        [SerializeField]
        private Illumination ambientLight = Illumination.None;
        private bool ambientIsNotDark;

        private Dictionary<Vector2, AggregateIllumination> lightMap
          = new Dictionary<Vector2, AggregateIllumination>();

        [Header("Sight:")]
        [SerializeField]
        private Illumination previouslySeen = Illumination.Half;
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
            for (int i = 0; i < pathParameters.Count; ++i) { pathParameters[i].Initialize(); }
            lightShaft.Initialize();

            ambientIsNotDark = ambientLight > Illumination.None;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Moveable.OnStartMove += UpdateMovingObjectIllumination;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Moveable.OnStartMove -= UpdateMovingObjectIllumination;
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

            // Create physical cave
            LayCaveFloor();
            // (optional) randomly pick entrance and exit locations (@ ForkPts and/or TerminusPts)
            LayPassages();
            LayBottleneckObstacles();
            // SpawnGems()
            PopulateChambers();
            SpawnEntities();
            UnblockEssentialPath();
            LaySunShafts();


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
                AlterTileIllumination(position, Illumination.Full, true);
            }

            // Dimly reveal all tiles in the outer Dim band.
            foreach (Vector2 dimOffset in dimOffsetsBand)
            {
                var position = location + dimOffset;
                AlterTileIllumination(position, Illumination.Half, true);
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
            AlterTileIllumination(endBrightPosition, startBrightPosition, Illumination.Full, true);
            AlterTileIllumination(startBrightPosition, endBrightPosition, Illumination.Full, false);
            AlterTileIllumination(endDimPositions, startDimPositions, Illumination.Half, true);
            AlterTileIllumination(startDimPositions, endDimPositions, Illumination.Half, false);
        }

        /// <summary>
        /// Add or remove a light contribution at each Target position which is not overlapping any
        /// Avoided positions. Update illuminateable components at each valid position. Spawn a Rock
        /// if the position is not yet defined by the cave system.
        /// </summary>
        /// <param name="targetPositions">Positions to attempt to light.</param>
        /// <param name="avoidedPositions">Positions to avoid lighting.</param>
        /// <param name="level">Level of light to apply.</param>
        /// <param name="isAddingContribution">True if adding light, false if removing.</param>
        private void AlterTileIllumination(HashSet<Vector2> targetPositions,
                                           HashSet<Vector2> avoidedPositions,
                                           Illumination level, bool isAddingContribution)
        {
            foreach (Vector2 position in targetPositions)
            {
                if (!avoidedPositions.Contains(position))
                {
                    AlterTileIllumination(position, level, isAddingContribution);
                }
            }
        }

        /// <summary>
        /// Add or remove a light contribution at the position. Update illuminateable components at
        /// the position. Spawn a Rock if the position is not yet defined by the cave system.
        /// </summary>
        private void AlterTileIllumination(Vector2 position, Illumination contribution,
            bool isAddingContribution)
        {
            // Update sight map.
            if (!sightMap.Contains(position)) { sightMap.Add(position); }

            // Update light map.
            AggregateIllumination light;
            if (lightMap.TryGetValue(position, out light))
            {
                if (isAddingContribution) { light.AddContribution(contribution); }
                else                      { light.RemoveContribution(contribution); }
            }
            else
            {
                light = AddBakedToLightMap(position);

                if (isAddingContribution) { light.AddContribution(contribution); }
                else                      { light.RemoveContribution(contribution); }
            }

            // Update illumination of pre-existing tiles.
            if (caveFloor.Contains(position))
            {
                UpdateObjectIllumination(position);
            }
            // Or create a Rock if no tile yet exists.
            else
            {
                AddTile(rockPrefab, position, holders["Obstacles"]);
            }
        }

        /// <summary>
        /// Update the illuminateable component to match the light map and sight map.
        /// </summary>
        private void UpdateObjectIllumination(Illuminateable component, Vector2 position,
            AggregateIllumination light, bool inSightMap)
        {
            if (component == null) { return; }

            switch (component.ObjectType)
            {
                case Illuminateable.Type.Terrain:
                    component.Brightness = !inSightMap ?
                        light.Brightness :
                        (Illumination)Mathf.Max((int)previouslySeen,
                                                (int)light.Brightness);
                    break;
                case Illuminateable.Type.Entity:
                    component.Brightness = hiddenEntities || !inSightMap ?
                        light.Brightness :
                        (Illumination)Mathf.Max((int)previouslySeen,
                                                (int)light.Brightness);
                    break;
                default:
                    throw new System.ArgumentException("Unsupported object type.");
            }
        }

        /// <summary>
        /// Update all illuminateable objects at the position.
        /// </summary>
        private void UpdateObjectIllumination(Vector2 position)
        {
            var components = Utility.FindComponentsAt<Illuminateable>(position);
            foreach (Illuminateable component in components)
            {
                UpdateObjectIllumination(component, position);
            }
        }

        /// <summary>
        /// Update the illuminateable component to match the light map and sight map.
        /// </summary>
        private void UpdateObjectIllumination(Illuminateable component, Vector2 position)
        {
            AggregateIllumination light;
            lightMap.TryGetValue(position, out light);

            UpdateObjectIllumination(component, position, light, sightMap.Contains(position));
        }

        /// <summary>
        /// Update the illuminateable component to match the light map and sight map.
        /// </summary>
        protected override void UpdateObjectIllumination(Illuminateable component)
        {
            var position = Utility.Constrain(component.transform.position);
            UpdateObjectIllumination(component, position);
        }

        /// <summary>
        /// Change illumination of moving object to lightmap value at destination.
        /// </summary>
        private void UpdateMovingObjectIllumination(GameObject movingObject, Vector2 destination)
        {
            var component = movingObject.GetComponent<Illuminateable>();
            if (component != null)
            {
                // Beware: magic numbers ahead!
                var duration = 0.25f;
                var checks = 2;

                StartCoroutine(DoubleCheckObjectIllumination(component, duration, checks));
            }
        }

        /// <summary>
        /// Repeatedly update object illumination for a short duration. This handles the case when:
        /// player moves -> object moves -> player moves, which can leave the object stuck in dim
        /// lighting even when it should be dark.
        /// </summary>
        private IEnumerator DoubleCheckObjectIllumination(Illuminateable component, float duration,
            int numberOfChecks)
        {
            // ex: duration = 1 second; checks = 3:
            //  |-----|-----|
            // 0.0   0.5   1.0
            var waitTime = numberOfChecks > 1 ? duration / (numberOfChecks - 1) : duration;

            for (int i = 0; i < numberOfChecks; ++i)
            {
                // Check if object has been destroyed.
                if (component == null) { break; }

                var position = Utility.Constrain(component.transform.position);
                UpdateObjectIllumination(component, position);

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
        protected override GameObject AddTile(GameObject prefab, Vector2 position, Transform holder)
        {
            if (!caveFloor.Contains(position)) { AddFloorTile(position); }

            var addedTile = base.AddTile(prefab, position, holder);
            caveEntity.Add(position);
            UpdateObjectIllumination(addedTile.GetComponent<Illuminateable>(), position);
            return addedTile;
        }

        protected override GameObject AddFloorTile(Vector2 position)
        {
            if (!caveFloor.Contains(position)) { caveFloor.Add(position); }

            // Update light map and sight map.
            AggregateIllumination light;
            if (lightMap.TryGetValue(position, out light)) { }
            else { AddBakedToLightMap(position); }

            var addedTile = base.AddFloorTile(position);
            UpdateObjectIllumination(addedTile.GetComponent<Illuminateable>(), position);
            return addedTile;
        }

        /// <summary>
        /// Add a new location to the light map and sight map. Initialized with baked ambient
        /// lighting.
        /// </summary>
        private AggregateIllumination AddBakedToLightMap(Vector2 position)
        {
            var light = new AggregateIllumination(ambientLight);
            lightMap.Add(position, light);

            if (startsRevealed || ambientIsNotDark) { sightMap.Add(position); }

            return light;
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
        /// Wire up the set of Feature Points (Vector2) to the Feature Tiles.
        /// </summary>
        /// <param name="featurePoints">i.e. pathInfo.path.InflectionPts</param>
        /// <param name="featureTiles">i.e. pathInfo.inflectionTiles</param>
        private void MarkFeaturePoints(List<Path.FeaturePoint> featurePoints, List<Vector2> featureTiles)
        {
            foreach (Path.FeaturePoint featurePt in featurePoints)
            {
                var pt = Utility.Constrain(featurePt.Pt);

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
                var pt = Utility.Constrain(featurePt);

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
        /// Instantiate the Paths belonging to this cave.
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
                        var pt = Utility.Constrain(points[i].Pt);
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
                        pathInfo.originTile = Utility.Constrain(pathInfo.path.Main[0].Pt);
                        pathInfo.terminusTile = Utility.Constrain(pathInfo.path.Main[
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
                AggregateIllumination light;
                if (lightMap.TryGetValue(position, out light)) { }
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

        /// <summary>
        /// Place groupings of sun shafts throughout the cave.
        /// </summary>
        private void LaySunShafts()
        {
            var numMainShafts = Mathf.CeilToInt((float)caveFloor.Count * lightShaft.density);
            var validPositions = caveFloor.ToList();
            var occupiedPositions = new HashSet<Vector2>();

            // Create each grouping.
            for (int i = 0; i < numMainShafts; ++i)
            {
                lightShaft.Initialize();
                var index = Random.Range(0, validPositions.Count - 1);
                var startPosition = validPositions[index];
                var isChain = Random.value <= lightShaft.chainChance;
                var offset = Vector2.zero;

                // Create each shaft in the grouping.
                var numSubShafts = lightShaft.clusterSize.Value;
                for (int j = 0; j < numSubShafts; ++j)
                {
                    var position = startPosition + offset;
                    if (occupiedPositions.Contains(position)) { continue; }

                    if (lightShaft.illumination > Illumination.None)
                    {
                        AlterTileIllumination(position, lightShaft.illumination, true);
                    }
                    else
                    {
                        if (!sightMap.Contains(position)) { sightMap.Add(position); }
                        UpdateObjectIllumination(position);
                    }
                    
                    if (isChain) { startPosition = position; }
                    var distance = lightShaft.scatterDistance.Value;
                    var angle = Random.Range(0f, 2 * Mathf.PI);
                    var offsetX = distance * Mathf.Cos(angle);
                    var offsetY = distance * Mathf.Sin(angle);
                    offset = Utility.Constrain(new Vector2(offsetX, offsetY));
                }
            }
        }
    }
}
