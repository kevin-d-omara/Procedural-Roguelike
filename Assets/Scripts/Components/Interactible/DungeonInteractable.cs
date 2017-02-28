using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class DungeonInteractable : Interactable
	{
        public override void OnBlockObject(GameObject blockedObject)
        {
            Debug.Log("DungeonInteractable: OnBlockedObject");
        }
    }
}
