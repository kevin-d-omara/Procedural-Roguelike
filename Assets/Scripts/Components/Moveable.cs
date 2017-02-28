using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Moveable : MonoBehaviour
    {
        public delegate void FlippedDirectionX(bool flipX);
        public event FlippedDirectionX OnFlippedDirectionX;

        public delegate void CantMove(GameObject blockingObject);
        public event CantMove OnCantMove;

        public delegate void CanMove(Vector2 destination);
        public event CanMove OnCanMove;

        public delegate void EndedSuccessfulMove(Vector2 destination);
        public event EndedSuccessfulMove OnEndedSuccessfulMove;

        public bool IsMoving { get; private set; }
        private Vector2 _facing = Vector2.right;
        public Vector2 Facing
        {
            get { return _facing; }
            private set
            {
                _facing = value;
                if (_facing == new Vector2(-1, 0))
                {
                    if (OnFlippedDirectionX != null) { OnFlippedDirectionX(true); }
                }
                else if (_facing == new Vector2(1, 0))
                {
                    if (OnFlippedDirectionX != null) { OnFlippedDirectionX(false); }
                }
            }
        }

        /// <summary>
        /// Speed (units/sec) of movement (i.e. 10 units/sec = 0.1 sec to move 1 units).
        /// </summary>
        [Range(1, 100)]
        [SerializeField] private int speed = 10;

        /// <summary>
        /// Time that must be waited between movements.
        /// </summary>
        [Range(0f, 2f)]
        [SerializeField] private float cooldown = 0.1f;

        /// <summary>
        /// Distance moved during each movement.
        /// </summary>
        [Range(1, 3)]
        [SerializeField] private int distance = 1;

        /// <summary>
        /// Layer on which collision will be checked.
        /// </summary>
        [SerializeField] private LayerMask blockingLayer;

        private BoxCollider2D boxCollider;
        private Rigidbody2D rb2D;

        protected virtual void Start()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            rb2D = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Moves the object if the path is not blocked. Does nothing otherwise.
        /// </summary>
        /// <param name="xDir">How far in the x-direction to move.</param>
        /// <param name="yDir">How far in the y-direction to move.</param>
        /// <param name="hit">Object blocking movement or null if none.</param>
        /// <returns>True if movemet was successful. False otherwise.</returns>
        protected virtual bool Move(Vector2 direction, out RaycastHit2D hit)
        {
            Vector2 start = transform.position;
            var end = start + direction * distance;

            if (boxCollider != null) { boxCollider.enabled = false; }
            hit = Physics2D.Linecast(start, end, blockingLayer);
            if (boxCollider != null) { boxCollider.enabled = true; }

            if (hit.transform == null)
            {
                StartCoroutine(SmoothMovement(end));
                Facing = direction;
                if (OnCanMove != null)
                {
                    OnCanMove(end);
                }
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
                    speed * Time.deltaTime);
                rb2D.MovePosition(newPostion);
                sqrRemainingDistance = (transform.position - end).sqrMagnitude;
                yield return null;
            }

            yield return new WaitForSeconds(cooldown);
            IsMoving = false;
            if (OnEndedSuccessfulMove != null)
            {
                OnEndedSuccessfulMove(end);
            }
        }

        /// <summary>
        /// Attempts to move the object in the specified direction.
        /// </summary>
        /// <param name="xDir">How far in the x-direction to move.</param>
        /// <param name="yDir">How far in the y-direction to move.</param>
        /// <returns>True if move started successfully, false otherwise.</returns>
        public virtual bool AttemptMove(Vector2 direction)
        {
            if (IsMoving) { return false; }

            RaycastHit2D hit;
            if (Move(direction, out hit))
            {
                IsMoving = true;
                return true;
            }
            else
            {
                Facing = direction;

                // Raise collision event.
                var interactibleComponent = hit.transform.GetComponent<Interactible>();
                if (interactibleComponent != null)
                {
                    // Notify blocking object of collision.
                    interactibleComponent.OnCantMove(gameObject);
                }
                // Notify this object of collision.
                if (OnCantMove != null) { OnCantMove(hit.transform.gameObject); }

                return false;
            }
        }
    }
}
