using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class Player : MovingObject
	{
        /// <summary>
        /// Action the Player takes when movement is blocked. For example, opening a chest, entering
        /// a dungeon via an exit tile, etc.
        /// </summary>
        /// <param name="hitTransform">Transform of the object blocking movement.</param>
        protected override void OnCantMove(GameObject blockingObject)
        {
            switch(blockingObject.tag)
            {
                case "Exit":
                    break;
                case "Chest":
                    break;
                default:
                    break;
            }
        }
    }
}
