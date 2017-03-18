using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    public abstract class BoardManager : MonoBehaviour
    {
        // Floor
        [Header("Individual prefabs:")]
        [SerializeField] protected GameObject floorPrefab;
        protected Dictionary<Vector3, GameObject> floorTiles = new Dictionary<Vector3, GameObject>();
        [SerializeField] protected GameObject passagePrefab;

        // ScriptableObject data.
        [Header("Tile sets:")]
        [SerializeField] protected List<WeightedSet> obstacleSets;
        [SerializeField] protected List<WeightedSet> enemySets;
        [SerializeField] protected List<WeightedSet> itemSets;

        // Randomizers.
        protected WeightedRandomSet<GameObject> obstacles = new WeightedRandomSet<GameObject>();
        protected WeightedRandomSet<GameObject> enemies   = new WeightedRandomSet<GameObject>();
        protected WeightedRandomSet<GameObject> items     = new WeightedRandomSet<GameObject>();

        // Randomizer parameters.
        [Header("Density values:")]
        [Range(0f, 1f)] [SerializeField] protected float obstacleDensity = 0.15f;
        [Range(0f, 1f)] [SerializeField] protected float enemyDensity    = 0.02f;
        [Range(0f, 1f)] [SerializeField] protected float itemDensity     = 0.005f;
        [Range(0f, 1f)] [SerializeField] protected float passageDensity  = 0.005f;

        // Parents to place instantiated tiles under for organization.
        protected Dictionary<string, Transform> holders = new Dictionary<string, Transform>();

        protected virtual void Awake()
        {
            // Select a single obstacle set and enemy set for this BoardManager instance.
            var obstacleSet = obstacleSets[Random.Range(0, obstacleSets.Count)];
            var enemySet    = enemySets   [Random.Range(0, enemySets.Count)];
            var itemSet     = itemSets    [Random.Range(0, itemSets.Count)];

            // Transform each WeightedSet into a WeightedRandomSet
            foreach (WeightedPairGO pair in obstacleSet.list)
            {
                obstacles.Add(pair.item, pair.weight);
            }
            foreach (WeightedPairGO pair in enemySet.list)
            {
                enemies.Add(pair.item, pair.weight);
            }
            foreach (WeightedPairGO pair in itemSet.list)
            {
                items.Add(pair.item, pair.weight);
            }

            // Wire up holder GameObjects for organizational parenting.
            holders.Add("Floor",     transform.Find("Floor"));
            holders.Add("Obstacles", transform.Find("Obstacles"));
            holders.Add("Enemies",   transform.Find("Enemies"));
            holders.Add("Items",     transform.Find("Items"));
            holders.Add("Passages",  transform.Find("Passages"));
        }

        protected virtual void OnEnable()
        {
            LightSource.OnIlluminate += RevealDarkness;
            LightSource.OnLightSourceMoved += RevealDarkness;
            Moveable.OnTileNotFound += OnTileNotFound;
        }

        protected virtual void OnDisable()
        {
            LightSource.OnIlluminate -= RevealDarkness;
            LightSource.OnLightSourceMoved -= RevealDarkness;
            Moveable.OnTileNotFound -= OnTileNotFound;
        }

        /// <summary>
        /// Creates a new tile at the position specified.
        /// </summary>
        protected virtual GameObject AddTile(GameObject prefab, Vector2 position, Transform holder)
        {
            var instance = Instantiate(prefab, position, Quaternion.identity) as GameObject;
            instance.transform.SetParent(holder);
            return instance;
        }

        /// <summary>
        /// Creates a new floor tile at the position specified.
        /// </summary>
        protected virtual GameObject AddFloorTile(Vector2 position)
        {
            var instance = Instantiate(floorPrefab, position, Quaternion.identity) as GameObject;
            instance.transform.SetParent(holders["Floor"]);
            floorTiles.Add(position, instance);
            return instance;
        }

        /// <summary>
        /// Creates a new floor tile and possibly an entity.
        /// </summary>
        protected abstract void OnTileNotFound(Vector2 position);

        /// <summary>
        /// Checks the tiles surrounding 'location' and creates tiles which are not yet revealed.
        /// </summary>
        /// <param name="location">Location in the world to center the offsets.</param>
        /// <param name="brightOffsets">Pattern of full light to reveal.</param>
        /// /// <param name="dimOffsetsBand">Pattern of dim light to reveal.</param>
        public abstract void RevealDarkness(Vector2 location, List<Vector2> brightOffsets,
            List<Vector2> dimOffsetsBand);

        /// <summary>
        /// Increase illumination of tiles near the end location and reduce illumination of tiles
        /// near the start location. Use when a light source moves.
        /// </summary>
        /// <param name="startLocation">Location in the world to de-illuminate.</param>
        /// <param name="startOffsets">Pattern to de-illuminate.</param>
        /// <param name="endLocation">Location in the world to illuminate.</param>
        /// <param name="endOffsets">Pattern to illuminate.</param>
        public abstract void RevealDarkness(
            Vector2 startLocation, List<Vector2> startOffsets, List<Vector2> startDimOffsetsBand,
            Vector2   endLocation, List<Vector2>   endOffsets, List<Vector2>   endDimOffsetsBand);

        /// <summary>
        /// Snap point to nearest whole value x & y (i.e. [2.7, -3.7] -> [3.0, -4.0]).
        /// </summary>
        public static Vector2 Constrain(Vector2 pt)
        {
            return new Vector2(Mathf.Round(pt.x), Mathf.Round(pt.y));
        }

        /// <summary>
        /// Snap point to nearest whole value x & y (i.e. [2.7, -3.7] -> [3.0, -4.0]).
        /// </summary>
        public static Vector2 Constrain(float x, float y)
        {
            return new Vector2(Mathf.Round(x), Mathf.Round(y));
        }
    }
}
