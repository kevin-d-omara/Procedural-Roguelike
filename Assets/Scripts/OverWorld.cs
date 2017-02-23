using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class OverWorld : MonoBehaviour
	{
        [SerializeField] private GameObject floorTilePrefab;
        // Child references for keeping an organized heirarchy.
        private Transform floorHolder;

        Dictionary<Vector3, GameObject> floorTiles = new Dictionary<Vector3, GameObject>();

        private void Awake()
        {
            // Get child references.
            floorHolder = transform.Find("Floor");
        }

        private void OnEnable()
        {
            Player.OnSuccessfulMove += RevealFogOfWar;
        }

        private void OnDisable()
        {
            Player.OnSuccessfulMove -= RevealFogOfWar;
        }

        /// <summary>
        /// Creates the starting location of the OverWorld. This is an "m x n" rectangle of floor
        /// tiles centered at the specified position.
        /// </summary>
        /// <param name="size">Width and Height of the starting location.</param>
        /// <param name="position">Position to center starting location at.</param>
		public void SetupOverWorld(Vector2 size, Vector2 position)
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
        /// Creates a new floor tile at the position specified and handles bookkeeping.
        /// </summary>
        private void AddFloorTile(Vector2 position)
        {
            var positionV3 = new Vector3(position.x, position.y, 0);

            GameObject instance = Instantiate(floorTilePrefab, positionV3, Quaternion.identity)
                as GameObject;
            instance.transform.SetParent(floorHolder);
            floorTiles.Add(positionV3, instance);
        }

        /// <summary>
        /// Checks the tiles surrounding 'location' and creates tiles which are not yet revealed.
        /// </summary>
        /// <param name="location">Location in the world to center the offsets.</param>
        /// <param name="offsets">List of offsets specifying pattern to reveal.</param>
        public void RevealFogOfWar(Vector2 location, List<Vector2> offsets)
        {
            foreach (Vector2 offset in offsets)
            {
                var position = location + offset;

                GameObject tile;
                if (floorTiles.TryGetValue(position, out tile))
                { }
                else
                {
                    AddFloorTile(position);
                }
            }
        }
	}
}
