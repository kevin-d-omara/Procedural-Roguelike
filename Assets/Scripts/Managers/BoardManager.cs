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
        /// Creates a new tile at the position specified.
        /// </summary>
        protected GameObject AddTile(GameObject prefab, Vector2 position, Transform holder)
        {
            var instance = Instantiate(prefab, position, Quaternion.identity)  as GameObject;
            instance.transform.SetParent(holder);
            return instance;
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
