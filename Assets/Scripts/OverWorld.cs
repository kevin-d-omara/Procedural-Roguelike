using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class OverWorld : MonoBehaviour
	{
        // % of tiles that are Wall or Bramble instead of Floor.
        [SerializeField] private float obstacleDensity;

        /// <summary>
        /// Container class for related info about tiles.
        /// </summary>
        [Serializable]
        public class TileInfo
        {
            public GameObject Prefab { get { return _prefab; } }
            public Transform Holder { get { return _holder; } }
            public Dictionary<Vector3, GameObject> Tiles { get; private set; }

            [SerializeField] private GameObject _prefab;
            [SerializeField] private Transform _holder;
            
            public TileInfo()
            {
                Tiles = new Dictionary<Vector3, GameObject>();
            }
        }
        [Serializable] public class SerializableDictionaryOf_string_TileInfo
            : SerializableDictionary<string, TileInfo> { }

        private Dictionary<string, TileInfo> tiles = new Dictionary<string, TileInfo>();
        [SerializeField] private SerializableDictionaryOf_string_TileInfo tileInfo;

        private void Awake()
        {
            tiles = tileInfo.DeSerialize();
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

            GameObject instance = Instantiate(tiles["Floor"].Prefab, positionV3, Quaternion.identity)
                as GameObject;
            instance.transform.SetParent(tiles["Floor"].Holder);
            tiles["Floor"].Tiles.Add(positionV3, instance);
        }

        /// <summary>
        /// Creates a new obstacle tile (Bramble or Rock) at the position specified and handles
        /// bookeeping.
        /// </summary>
        private void AddObstacleTile(Vector2 position, string type)
        {
            var positionV3 = new Vector3(position.x, position.y, 0);

            GameObject instance = Instantiate(tiles[type].Prefab, positionV3, Quaternion.identity)
                as GameObject;
            instance.transform.SetParent(tiles[type].Holder);
            tiles[type].Tiles.Add(positionV3, instance);
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
                if (tiles["Floor"].Tiles.TryGetValue(position, out tile))
                { }
                else
                {
                    AddFloorTile(position);
                    AddObstacleTile(position, "Bramble");
                }
            }
        }
	}
}
