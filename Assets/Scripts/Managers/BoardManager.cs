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
        [SerializeField] protected FloorInfo floor;
        [SerializeField] protected ObstacleInfo obstacles;
        [SerializeField] protected EnemyInfo enemies;

        [SerializeField] private Transform floorHolder;
        [SerializeField] private Transform obstacleHolder;
        [SerializeField] private Transform enemyHolder;

        protected virtual void Awake()
        {
            // Create new instance of each ScriptableObject
            floor = Instantiate(floor) as FloorInfo;
            obstacles = Instantiate(obstacles) as ObstacleInfo;
            enemies = Instantiate(enemies) as EnemyInfo;

            // Wire up holder GameObjects for organizational parenting.
            floor.holder = floorHolder;
            obstacles.holder = obstacleHolder;
            enemies.holder = enemyHolder;
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
        /// Creates the starting location of the board. This is an "m x n" rectangle of floor
        /// tiles centered at the specified position.
        /// </summary>
        /// <param name="size">Width and Height of the starting location.</param>
        /// <param name="position">Position to center starting location at.</param>
        public virtual void SetupBoard(Vector2 size, Vector2 position)
        {
            // Center tiles on position.
            var startY = 1 - ((int)size.y + 1) / 2 + (int)position.y;
            var startX = 1 - ((int)size.x + 1) / 2 + (int)position.x;
            var endY = (int)size.y - ((int)size.y + 1) / 2 + (int)position.y;
            var endX = (int)size.x - ((int)size.x + 1) / 2 + (int)position.x;

            // Place floor tiles in "m x n" rectangle.
            for (int y = startY; y <= endY; ++y)
            {
                for (int x = startX; x <= endX; ++x)
                {
                    AddFloorTile(new Vector2(x, y));
                }
            }
        }

        /// <summary>
        /// Creates a new floor tile at the position specified (if not already present).
        /// </summary>
        protected virtual void AddFloorTile(Vector2 position)
        {
            var positionV3 = new Vector3(position.x, position.y, 0);

            if (!floor.existing.ContainsKey(positionV3))
            {
                var instance = Instantiate(floor.prefab, positionV3, Quaternion.identity)
                    as GameObject;
                instance.transform.SetParent(floor.holder);
                floor.existing.Add(positionV3, instance);
            }
        }

        /// <summary>
        /// Checks the tiles surrounding 'location' and creates tiles which are not yet revealed.
        /// </summary>
        /// <param name="location">Location in the world to center the offsets.</param>
        /// <param name="offsets">List of offsets specifying pattern to reveal.</param>
        public abstract void RevealFogOfWar(Vector2 location, List<Vector2> offsets);
    }
}
