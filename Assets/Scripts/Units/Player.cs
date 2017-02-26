using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class Player : MovingObject
	{
        public delegate void SuccessfulMove (Vector2 location, List<Vector2> offsets);
        public static event SuccessfulMove OnSuccessfulMove;

        [Range(0,10)]
        [SerializeField] private int sightDistance = 3;
        private LineOfSight lineOfSight;

        private void Awake()
        {
            lineOfSight = new LineOfSight(sightDistance);
        }

        protected override bool Move(int xDir, int yDir, out RaycastHit2D hit)
        {
            if (base.Move(xDir, yDir, out hit))
            {
                var movedTo = new Vector2(transform.position.x + xDir, transform.position.y + yDir);
                if (OnSuccessfulMove != null)
                {
                    OnSuccessfulMove(movedTo, lineOfSight.Offsets);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

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
