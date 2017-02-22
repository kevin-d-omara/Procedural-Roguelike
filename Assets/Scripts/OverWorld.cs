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

        private void Start()
        {
            // Get child references.
            floorHolder = transform.Find("Floor");

            SetupOverWorld(new Vector2(15, 15), new Vector2(0, 0));
        }

        /// <summary>
        /// Creates the starting location of the OverWorld. This is an "m x n" rectangle of floor
        /// tiles centered at the specified position.
        /// </summary>
        /// <param name="size">Width and Height of the starting location.</param>
        /// <param name="position">Position to center starting location at.</param>
		public void SetupOverWorld(Vector2 size, Vector2 position)
        {
            var halfHeight = (int)size.y / 2;
            var halfWidth = (int)size.x / 2;

            // Place floor tiles in "m x n" rectangle.
            for (int y = -halfHeight; y <= halfHeight; ++y)
            {
                for (int x = -halfWidth; x <= halfWidth; ++x)
                {
                    GameObject instance = Instantiate(floorTiles, new Vector3(x, y, 0),
                        Quaternion.identity) as GameObject;
                    instance.transform.SetParent(floorHolder);
                }
            }
        }
	}
}
