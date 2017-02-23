using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class Player : MovingObject
	{
        #if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
            private Vector2 touchOrigin = -Vector2.one;
        #endif

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            var horizontal = 0;
            var vertical = 0;

            #if UNITY_STANDALONE || UNITY_WEBPLAYER

                horizontal = (int)(Input.GetAxisRaw("Horizontal"));
                vertical   = (int)(Input.GetAxisRaw("Vertical"));

                // Limit movement to one axis per move.
                if (horizontal != 0) { vertical = 0; }

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
					
                        // Limit movement to one axis per move by favoring the dominant axis.
                        // Also, snap movement value to int values 1 or 0;
                        if (Mathf.Abs(dx) > Mathf.Abs(dy))
                        {
                            horizontal = dx > 0 ? 1 : -1;
                        }
                        else
                        {
                            vertical = dy > 0 ? 1 : -1;
                        }
				    }
			    }
			
            #endif

            if (horizontal != 0 || vertical != 0)
            {
                AttemptMove(horizontal, vertical);
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
