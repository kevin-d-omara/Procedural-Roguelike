using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public static class Utility
	{
        /// <summary>
        /// Return all GameObjects at the specified position. Discludes objects on the
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
        /// Return all components (objects) of type T at the specified position. Discludes objects
        /// on the IgnoreRayCastLayer.
        /// </summary>
        /// <returns>List of all Components at position.</returns>
        public static List<T> FindComponentsAt<T>(Vector2 position)
        {
            var components = new List<T>();

            var gameObjects = FindObjectsAt(position);
            foreach (GameObject gameObject in gameObjects)
            {
                var component = gameObject.GetComponent<T>();
                if (component != null)
                {
                    components.Add(component);
                }
            }

            return components;
        }

        /// <summary>
        /// Enable/Disable all BoxColliders at the specified position.
        /// </summary>
        public static void SetActiveBoxColliders(GameObject[] gameObjects, bool shouldEnable)
        {
            foreach (GameObject gObject in gameObjects)
            {
                var objectBoxCollider = gObject.GetComponent<BoxCollider2D>();
                if (objectBoxCollider != null) { objectBoxCollider.enabled = shouldEnable; }
            }
        }

        /// <summary>
        /// Transform X into -1, 0, or 1.
        /// </summary>
        /// <returns>Negative one, zero, or positive one.</returns>
        public static int MakeUnitLength(float x)
        {
            return Mathf.Abs(x) > 0 ? (int)Mathf.Sign(x) : 0;
        }

        /// <summary>
        /// Transform V into a vector whose components x, y = -1, 0, or 1.
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

        /// <summary>
        /// Snap point to nearest whole value x & y (i.e. [2.7, -3.7] -> [3.0, -4.0]).
        /// </summary>
        public static Vector2 Constrain(Vector2 pt)
        {
            return new Vector2(Mathf.Round(pt.x), Mathf.Round(pt.y));
        }
    }
}
