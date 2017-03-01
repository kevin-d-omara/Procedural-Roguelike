using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class DungeonEntrance : Interactable
	{
        public delegate void EnterDungeon(Vector2 position);
        public static event EnterDungeon OnEnterDungeon;

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.attachedRigidbody.tag == "Player")
            {
                // "Seal the entrance" so this Dungeon cannot be visited again.
                Destroy(gameObject);

                if (OnEnterDungeon != null) { OnEnterDungeon(transform.position); }
            }
        }
    }
}
