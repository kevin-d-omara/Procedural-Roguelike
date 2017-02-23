using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class Player : MovingObject
	{
        public delegate void SuccessfulMove (Vector2 location, List<Vector2> offsets);
        public static event SuccessfulMove OnSuccessfulMove;

        #if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
            private Vector2 touchOrigin = -Vector2.one;
        #endif
        [Range(0,10)]
        [SerializeField] private int sightDistance = 3;
        private LineOfSight lineOfSight;

        private void Awake()
        {
            lineOfSight = new LineOfSight(sightDistance);
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            if (IsMoving) { return; }

            int xDir, yDir;
            GetInputAsInt(out xDir, out yDir);
            // Limit movement to one axis per move.
            if (xDir != 0) { yDir = 0; }

            if (xDir != 0 || yDir != 0)
            {
                AttemptMove(xDir, yDir);
            }
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
        /// Checks input device (keyboard, touchscreen, etc.) for horizontal and vertical input.
        /// Snaps the values to -1, 0, or 1;
        /// </summary>
        /// <param name="horizontal">Variable to store horizontal input value in.</param>
        /// <param name="vertical">Variable to store vertical input value in.</param>
        private void GetInputAsInt(out int horizontal, out int vertical)
        {
            horizontal = 0;
            vertical   = 0;

            #if UNITY_STANDALONE || UNITY_WEBPLAYER

                horizontal = (int)(Input.GetAxisRaw("Horizontal"));
                vertical   = (int)(Input.GetAxisRaw("Vertical"));

            #elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
			
			    if (Input.touchCount > 0)
			    {
				    var touch = Input.touches[0];
                    touchOrigin = touch.position;
				    if (touch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
				    {
					    var touchEnd = touch.position;
					    var dx = touchEnd.x - touchOrigin.x;
					    var dy = touchEnd.y - touchOrigin.y;

                        // Prevent this touch event from being re-used.
					    touchOrigin.x = -1;
					
                        // Snap movement to int value -1, 0, or 1;
                        horizontal = dx > 0 ? 1 : -1;
                        vertical   = dy > 0 ? 1 : -1;
				    }
			    }
			
            #endif
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
