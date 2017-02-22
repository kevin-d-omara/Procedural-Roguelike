using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class OverWorld : MonoBehaviour
	{
        [SerializeField] private GameObject floorTiles;
        // Child references for keeping an organized heirarchy.
        private Transform floorHolder;

        private void Awake()
        {
            // Get child references.
            floorHolder = transform.Find("Floor");
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
                    GameObject instance = Instantiate(floorTiles, new Vector3(x, y, 0),
                        Quaternion.identity) as GameObject;
                    instance.transform.SetParent(floorHolder);
                }
            }
        }
	}
}
