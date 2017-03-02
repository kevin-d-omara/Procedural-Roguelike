using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    public class OverWorldManager : BoardManager
    {
        // Obstacle parameters (weights and types).
        [Range(0f, 1f)]
        [SerializeField] private float obstacleDensity = 0.15f;
        [Range(0f,1f)]
        [SerializeField] private float dungeonEntranceDensity = 0.01f;

        /// <summary>
        /// Creates a new obstacle at the position specified.
        /// </summary>
        private void AddObstacleTile(Vector2 position, GameObject prefab)
        {
            var positionV3 = new Vector3(position.x, position.y, 0);

            var instance = Instantiate(prefab, positionV3, Quaternion.identity)
                as GameObject;
            instance.transform.SetParent(obstacles.holder);
            obstacles.existing.Add(positionV3, instance);
        }

        /// <summary>
        /// Checks the tiles surrounding 'location' and creates tiles which are not yet illuminated.
        /// </summary>
        /// <param name="location">Location in the world to center the offsets.</param>
        /// <param name="offsets">List of offsets specifying pattern to reveal.</param>
        public override void RevealFogOfWar(Vector2 location, List<Vector2> offsets)
        {
            foreach (Vector2 offset in offsets)
            {
                var position = location + offset;

                GameObject tile;
                if (!floor.existing.TryGetValue(position, out tile))
                {
                    AddFloorTile(position);
                    if (Random.Range(0f, 1f) < dungeonEntranceDensity)
                    {
                        //AddObstacleTile(position, "DungeonEntrance");
                    }
                    else if (Random.Range(0f, 1f) < obstacleDensity)
                    {
                        AddObstacleTile(position, obstacles.Randomizer.RandomItem());
                    }
                }
            }
        }
    }
}
