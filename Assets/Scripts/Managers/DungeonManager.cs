using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    public class DungeonManager : BoardManager
    {
        public override void SetupBoard(Vector2 size, Vector2 position)
        {
            base.SetupBoard(size, position);

            // Temporary exit spawning to test transition between Dungeon => OverWorld
            var positionV3 = new Vector3(position.x + 5, position.y + 5, 0);
            GameObject instance = Instantiate(tiles["DungeonExit"].Prefab, positionV3, Quaternion.identity)
                as GameObject;
            instance.transform.SetParent(tiles["DungeonExit"].Holder);
            tiles["DungeonExit"].Tiles.Add(positionV3, instance);
        }

        public override void RevealFogOfWar(Vector2 location, List<Vector2> offsets)
        {
            //throw new NotImplementedException();
            // do nothing
        }
    }
}
