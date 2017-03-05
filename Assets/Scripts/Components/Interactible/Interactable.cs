using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class Interactable : MonoBehaviour
	{
        public bool _hasBeenUsed = false;
        public virtual bool HasBeenUsed
        {
            get { return _hasBeenUsed; }
            set { _hasBeenUsed = value; }
        }

        // Components
        protected BoxCollider2D boxCollider;

        protected virtual void Awake()
        {
            // Get references to all components.
            boxCollider = GetComponent<BoxCollider2D>();
        }

        /// <summary>
        /// Action to take when this object blocks another's movement.
        /// </summary>
        public virtual void OnBlockObject(GameObject blockedObject) { }

        /// <summary>
        /// Action to take when this object is triggered.
        /// </summary>
        protected virtual void OnTriggerEnter2D(Collider2D collision) { }
    }
}
