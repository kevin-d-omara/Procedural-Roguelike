using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public static class Utility
	{
        /// <summary>
        /// Finds all GameObjects at the specified position. Discludes objects on the
        /// IgnoreRaycastLayer.
        /// </summary>
        /// <returns>Array of all GameObjects at position.</returns>
		public static GameObject[] FindObjectsAt(Vector2 position)
        {
            var hits = Physics2D.OverlapBoxAll(position, new Vector2(0.25f, 0.25f), 0f);

            GameObject[] objects = new GameObject[hits.Length];
            for (int i = 0; i < hits.Length; ++i)
            {
                objects[i] = hits[i].gameObject;
            }
            return objects;
        }
	}
}
