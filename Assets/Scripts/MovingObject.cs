using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    public abstract class MovingObject : MonoBehaviour
    {
        public float moveTime = 0.1f;           //Time it will take object to move, in seconds.
        public bool isMoving = false;
        public LayerMask blockingLayer;         //Layer on which collision will be checked.

        private BoxCollider2D boxCollider;      //The BoxCollider2D component attached to this object.
        private Rigidbody2D rb2D;               //The Rigidbody2D component attached to this object.
        private float inverseMoveTime;          //Used to make movement more efficient.

        protected virtual void Start()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            rb2D = GetComponent<Rigidbody2D>();

            inverseMoveTime = 1f / moveTime;
        }

        /// <summary>
        /// Moves the object if the path is not blocked. Does nothing otherwise.
        /// </summary>
        /// <param name="xDir">How far in the x-direction to move.</param>
        /// <param name="yDir">How far in the y-direction to move.</param>
        /// <param name="hit">Object blocking movement or null if none.</param>
        /// <returns>True if movemet was successful. False otherwise.</returns>
        protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
        {
            Vector2 start = transform.position;
            var end = start + new Vector2(xDir, yDir);

            // Linecast to check if movement is blocked.
            boxCollider.enabled = false;
            hit = Physics2D.Linecast(start, end, blockingLayer);
            boxCollider.enabled = true;

            if (hit.transform == null)
            {
                StartCoroutine(SmoothMovement(end));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Co-routine for moving objects smoothly.
        /// </summary>
        /// <param name="end">Position to move to.</param>
        protected IEnumerator SmoothMovement(Vector3 end)
        {
            var sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            while (sqrRemainingDistance > float.Epsilon)
            {
                var newPostion = Vector3.MoveTowards(rb2D.position, end,
                    inverseMoveTime * Time.deltaTime);
                rb2D.MovePosition(newPostion);
                sqrRemainingDistance = (transform.position - end).sqrMagnitude;
                yield return null;
            }

            isMoving = false;
        }

        /// <summary>
        /// Attempts to move the object in the specified direction.
        /// </summary>
        /// <param name="xDir">How far in the x-direction to move.</param>
        /// <param name="yDir">How far in the y-direction to move.</param>
        protected virtual void AttemptMove(int xDir, int yDir)
        {
            if (isMoving) { return; }

            RaycastHit2D hit;
            bool canMove = Move(xDir, yDir, out hit);

            if (canMove)
            {
                isMoving = true;
                return;
            }

            OnCantMove(hit.transform.gameObject);
        }

        /// <summary>
        /// Action taken when blocked.
        /// </summary>
        /// <param name="blockingObject">Object blocking the movement.</param>
        protected abstract void OnCantMove(GameObject blockingObject);
    }
}
