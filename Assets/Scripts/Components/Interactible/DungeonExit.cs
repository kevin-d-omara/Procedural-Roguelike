using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class DungeonExit : Interactable
	{
        public delegate void ExitDungeon(Vector2 position);
        public static event ExitDungeon OnExitDungeon;

        public override void OnBlockObject(GameObject blockedObject)
        {
            if (blockedObject.tag == "Player")
            {
                // "Seal the exit" so this Dungeon cannot be visited again.
                Destroy(gameObject);

                if (OnExitDungeon != null) { OnExitDungeon(transform.position); }
            }
        }
    }
}
