using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    public class OverWorldManager : BoardManager
    {
        [SerializeField] private GameObject entrancePrefab;

        // Randomizer parameters.
        [Range(0f, 1.01f)]
        [SerializeField] private float obstacleDensity = 0.15f;
        [Range(0f,1f)]
        [SerializeField] private float dungeonEntranceDensity = 0.01f;

        protected override void Awake()
        {
            base.Awake();

            // Wire up holder GameObjects for organizational parenting.
            holders.Add("DungeonEntrance", transform.Find("DungeonEntrance"));
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
                if (!floorTiles.TryGetValue(position, out tile))
                {
                    AddFloorTile(position);
                    if (Random.Range(0f, 1f) < dungeonEntranceDensity)
                    {
                        AddTile(entrancePrefab, position, holders["DungeonEntrance"]);
                    }
                    else if (Random.Range(0f, 1f) < obstacleDensity)
                    {
                        AddTile(obstacles.RandomItem(), position, holders["Obstacles"]);
                    }
                }
            }
        }
    }
}
