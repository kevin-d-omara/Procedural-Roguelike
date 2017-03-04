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

        // Randomizers.
        protected WeightedRandomSet<GameObject> obstacles = new WeightedRandomSet<GameObject>();
        protected WeightedRandomSet<GameObject> enemies = new WeightedRandomSet<GameObject>();

        // Randomizer parameters.
        [Header("Density values:")]
        [Range(0f, 1.01f)] [SerializeField] protected float obstacleDensity = 0.15f;
        [Range(0f, 1.01f)] [SerializeField] protected float enemyDensity = 0.02f;
        [Range(0f, 1f)]    [SerializeField] protected float passageDensity = 0.005f;

        // Parents to place instantiated tiles under for organization.
        protected Dictionary<string, Transform> holders = new Dictionary<string, Transform>();

        protected virtual void Awake()
        {
            // Select a single obstacle set and enemy set for this BoardManager instance.
            var obstacleSet = obstacleSets[Random.Range(0, obstacleSets.Count)];
            var enemySet    = enemySets   [Random.Range(0, enemySets.Count)];

            // Transform each WeightedSet into a WeightedRandomSet
            foreach (WeightedPairGO pair in obstacleSet.list)
            {
                obstacles.Add(pair.item, pair.weight);
            }
            foreach (WeightedPairGO pair in enemySet.list)
            {
                enemies.Add(pair.item, pair.weight);
            }

            // Wire up holder GameObjects for organizational parenting.
            holders.Add("Floor", transform.Find("Floor"));
            holders.Add("Obstacles", transform.Find("Obstacles"));
            holders.Add("Enemies", transform.Find("Enemies"));
        }

        protected virtual void OnEnable()
        {
            LightSource.OnIlluminate += RevealDarkness;
        }

        protected virtual void OnDisable()
        {
            LightSource.OnIlluminate -= RevealDarkness;
        }


        /// <summary>
        /// Creates an entrance location of to board. This is an "m x n" rectangle of floor
        /// tiles centered at the specified position.
        /// </summary>
        /// <param name="size">Width and Height of the starting location.</param>
        /// <param name="position">Position to center starting location at.</param>
        public virtual void SetupEntrance(Vector2 size, Vector2 position)
        {
            // Clear blocking tiles at center position.
            var blockingObjects = Utility.FindObjectsAt(position);
            foreach (GameObject blockingObject in blockingObjects)
            {
                if (blockingObject.tag != "Player")
                {
                    Destroy(blockingObject);
                }
            }

            // Center tile placement on position.
            var startY = 1 - ((int)size.y + 1) / 2 + (int)position.y;
            var startX = 1 - ((int)size.x + 1) / 2 + (int)position.x;
            var endY = (int)size.y - ((int)size.y + 1) / 2 + (int)position.y;
            var endX = (int)size.x - ((int)size.x + 1) / 2 + (int)position.x;

            // Place floor tiles in "m x n" rectangle.
            for (int y = startY; y <= endY; ++y)
            {
                for (int x = startX; x <= endX; ++x)
                {
                    var newPosition = new Vector2(x, y);
                    if (!floorTiles.ContainsKey(newPosition))
                    {
                        AddFloorTile(newPosition);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new tile at the position specified.
        /// </summary>
        protected void AddTile(GameObject prefab, Vector2 position, Transform holder)
        {
            var instance = Instantiate(prefab, position, Quaternion.identity)  as GameObject;
            instance.transform.SetParent(holder);
        }

        /// <summary>
        /// Creates a new floor tile at the position specified.
        /// </summary>
        protected virtual void AddFloorTile(Vector2 position)
        {
            var instance = Instantiate(floorPrefab, position, Quaternion.identity) as GameObject;
            instance.transform.SetParent(holders["Floor"]);
            floorTiles.Add(position, instance);
        }

        /// <summary>
        /// Checks the tiles surrounding 'location' and creates tiles which are not yet revealed.
        /// </summary>
        /// <param name="location">Location in the world to center the offsets.</param>
        /// <param name="offsets">List of offsets specifying pattern to reveal.</param>
        public abstract void RevealDarkness(Vector2 location, List<Vector2> offsets);
    }
}
