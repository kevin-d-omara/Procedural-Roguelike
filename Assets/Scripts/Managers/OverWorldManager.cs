using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    public class OverWorldManager : BoardManager
    {
        protected override void Awake()
        {
            base.Awake();

            // Wire up holder GameObjects for organizational parenting.
            holders.Add("CaveEntrance", transform.Find("CaveEntrance"));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            GameManager.OnIncreaseDifficulty += OnIncreaseDifficulty;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            GameManager.OnIncreaseDifficulty -= OnIncreaseDifficulty;
        }

        /// <summary>
        /// Checks the tiles surrounding 'location' and creates tiles which are not yet illuminated.
        /// </summary>
        /// <param name="location">Location in the world to center the offsets.</param>
        /// <param name="offsets">List of offsets specifying pattern to reveal.</param>
        public override void RevealDarkness(Vector2 location, List<Vector2> offsets)
        {
            foreach (Vector2 offset in offsets)
            {
                var position = location + offset;

                GameObject tile;
                if (!floorTiles.TryGetValue(position, out tile))
                {
                    AddFloorTile(position);
                    if (Random.Range(0f, 1f) < passageDensity)
                    {
                        AddTile(passagePrefab, position, holders["CaveEntrance"]);
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

        private void OnIncreaseDifficulty(int difficulty)
        {
            // Increase obstacle density by multiplier (cumulative).
            obstacleDensity *= 1.2f;
        }
    }
}
