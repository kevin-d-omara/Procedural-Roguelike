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

        /// <summary>
        /// Transforms X into -1, 0, or 1.
        /// </summary>
        /// <returns>Negative one, zero, or positive one.</returns>
        public static int MakeUnitLength(float x)
        {
            return Mathf.Abs(x) > 0 ? (int)Mathf.Sign(x) : 0;
        }

        /// <summary>
        /// Transforms V into a vector whose components x, y = -1, 0, or 1.
        /// </summary>
        /// <returns>Vector with x, y ϵ {-1, 0, 1}.</returns>
        public static Vector2 MakeUnitLength(Vector2 V)
        {
            return new Vector2
                (
                    Mathf.Abs(V.x) > 0 ? Mathf.Sign(V.x) : 0,
                    Mathf.Abs(V.y) > 0 ? Mathf.Sign(V.y) : 0
                );
        }
    }
}
