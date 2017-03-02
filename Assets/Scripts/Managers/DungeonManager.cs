using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    public class DungeonManager : BoardManager
    {
        [SerializeField] private GameObject exitPrefab;
        [SerializeField] private Transform exitHolder;

        public override void SetupEntrance(Vector2 size, Vector2 position)
        {
            base.SetupEntrance(size, position);

            // Temporary exit spawning to test transition between Dungeon => OverWorld
            var positionV3 = new Vector3(position.x + 2, position.y + 2, 0);
            GameObject instance = Instantiate(exitPrefab, positionV3, Quaternion.identity)
                as GameObject;
            instance.transform.SetParent(exitHolder);
        }

        public override void RevealFogOfWar(Vector2 location, List<Vector2> offsets)
        {
            //throw new NotImplementedException();
            // do nothing
        }
    }
}
