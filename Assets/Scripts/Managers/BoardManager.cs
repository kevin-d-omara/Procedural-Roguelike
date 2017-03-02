using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Any class extending BoardManager must have the following:
    ///     
    /// </summary>
    public abstract class BoardManager : MonoBehaviour
    {
        // ScriptableObject data.
        [SerializeField] protected FloorInfo floor;
        [SerializeField] protected ObstacleInfo obstacles;
        [SerializeField] protected EnemyInfo enemies;

        // Parents to place instantiated tiles under for organization.
        protected Dictionary<string, Transform> holders = new Dictionary<string, Transform>();

        protected virtual void Awake()
        {
            // Create new instance of each ScriptableObject
            floor = Instantiate(floor) as FloorInfo;
            obstacles = Instantiate(obstacles) as ObstacleInfo;
            enemies = Instantiate(enemies) as EnemyInfo;

            // Wire up holder GameObjects for organizational parenting.
            holders.Add("Floor", transform.Find("Floor"));
            holders.Add("Obstacles", transform.Find("Obstacles"));
            holders.Add("Enemies", transform.Find("Enemies"));
        }

        protected virtual void OnEnable()
        {
            LightSource.OnIlluminate += RevealFogOfWar;
        }

        protected virtual void OnDisable()
        {
            LightSource.OnIlluminate -= RevealFogOfWar;
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
                    if (!floor.existing.ContainsKey(newPosition))
                    {
                        AddFloorTile(newPosition);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new floor tile at the position specified.
        /// </summary>
        protected virtual void AddFloorTile(Vector2 position)
        {
            var positionV3 = new Vector3(position.x, position.y, 0);

            var instance = Instantiate(floor.prefab, positionV3, Quaternion.identity)
                as GameObject;
            instance.transform.SetParent(holders["Floor"]);
            floor.existing.Add(positionV3, instance);
        }

        /// <summary>
        /// Checks the tiles surrounding 'location' and creates tiles which are not yet revealed.
        /// </summary>
        /// <param name="location">Location in the world to center the offsets.</param>
        /// <param name="offsets">List of offsets specifying pattern to reveal.</param>
        public abstract void RevealFogOfWar(Vector2 location, List<Vector2> offsets);
    }
}
