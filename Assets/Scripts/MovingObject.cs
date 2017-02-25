using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class MovingObject : MonoBehaviour
    {
        public bool IsMoving { get; private set; }
        private Vector2 _lastMove = Vector2.zero;
        public Vector2 LastMove
        {
            get { return _lastMove; }
            private set
            {
                _lastMove = value;
                if (_lastMove == new Vector2(-1,0))
                {
                    GetComponent<SpriteRenderer>().flipX = true;
                }
                else if (_lastMove == new Vector2(1,0))
                {
                    GetComponent<SpriteRenderer>().flipX = false;
                }
            }
        }

        // Time (seconds) it takes the object to move.
        [Range(0f, 1f)]
        [SerializeField] private float moveTime = 0.1f;
                         private float inverseMoveTime;
        // Time (seconds) to wait before the next movement is allowed.
        [Range(0f, 1f)]
        [SerializeField] private float waitTime = 0.1f;

        // Layer on which collision will be checked.
        [SerializeField] private LayerMask blockingLayer;
        private BoxCollider2D boxCollider;
        private Rigidbody2D rb2D;

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
        protected virtual bool Move(int xDir, int yDir, out RaycastHit2D hit)
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
                LastMove = new Vector2(xDir, yDir);
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

            yield return new WaitForSeconds(waitTime);
            IsMoving = false;
        }

        /// <summary>
        /// Attempts to move the object in the specified direction.
        /// </summary>
        /// <param name="xDir">How far in the x-direction to move.</param>
        /// <param name="yDir">How far in the y-direction to move.</param>
        /// <returns>True if move started successfully, false otherwise.</returns>
        protected virtual bool AttemptMove(int xDir, int yDir)
        {
            if (IsMoving) { return false; }

            RaycastHit2D hit;
            bool canMove = Move(xDir, yDir, out hit);

            if (canMove)
            {
                IsMoving = true;
                return true;
            }

            OnCantMove(hit.transform.gameObject);
            return false;
        }

        /// <summary>
        /// Action taken when blocked.
        /// </summary>
        /// <param name="blockingObject">Object blocking the movement.</param>
        protected abstract void OnCantMove(GameObject blockingObject);
    }
}
