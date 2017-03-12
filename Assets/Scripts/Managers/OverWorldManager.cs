using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    public class OverWorldManager : BoardManager
    {
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
        /// Creates an entrance location of to board. This is an "m x n" rectangle of floor
        /// tiles centered at the specified position.
        /// </summary>
        /// <param name="size">Width and Height of the starting location.</param>
        /// <param name="position">Position to center starting location at.</param>
        public void SetupEntrance(Vector2 size, Vector2 position)
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

            // Create (and then collapse) a passage at the entrance location.
            var passage = AddTile(passagePrefab, position, holders["Passages"]);
            var pController = passage.GetComponent<PassageController>();
            pController.HasBeenUsed = true;

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

        /// <summary>
        /// Increase illumination of tiles near the end location and reduce illumination of tiles
        /// near the start location. Use when a light source moves.
        /// </summary>
        public override void RevealDarkness(Vector2 startLocation, List<Vector2> startOffsets,
                                            Vector2 endLocation, List<Vector2> endOffsets)
        {
            RevealDarkness(endLocation, endOffsets);
        }

        private void OnIncreaseDifficulty(int difficulty)
        {
            // Increase obstacle density by multiplier (cumulative).
            obstacleDensity *= 1.2f;
        }

        protected override void OnTileNotFound(Vector2 position)
        {
            AddFloorTile(position);
        }
    }
}
