using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    [RequireComponent(typeof(BoxCollider2D))]
	public class Interactible : MonoBehaviour
	{
        /// <summary>
        /// Action to take when this object blocks another's movement.
        /// </summary>
        public virtual void OnCantMove(GameObject blockedObject) { }

        /// <summary>
        /// Action to take when this object is triggered.
        /// </summary>
        protected void OnTriggerEnter2D(Collider2D collision) { }
    }
}
