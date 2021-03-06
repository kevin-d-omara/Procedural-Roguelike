﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Moveable : MonoBehaviour
    {
        public delegate void CanMove(Vector2 destination);
        public event CanMove OnCanMove;

        public delegate void CantMove(GameObject blockingObject);
        public event CantMove OnCantMove;

        public delegate void StartMove(GameObject movingObject, Vector2 destination);
        public static event StartMove OnStartMove;

        public delegate void EndedSuccessfulMove(Vector2 destination);
        public event EndedSuccessfulMove OnEndedSuccessfulMove;

        public delegate void ReachedMiddleOfMove(Vector2 source, Vector2 destination);
        public event ReachedMiddleOfMove OnReachedMiddleOfMove;

        public delegate void FlippedDirectionX(bool flipX);
        public event FlippedDirectionX OnFlippedDirectionX;

        public delegate void TileNotFound(Vector2 position);
        public static event TileNotFound OnTileNotFound;

        public bool IsMoving { get; private set; }
        private Vector2 _facing = Vector2.right;
        public Vector2 Facing
        {
            get { return _facing; }
            set
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
        [Range(0.01f, 15f)]
        public float speed = 2;

        /// <summary>
        /// Time that must be waited between movements.
        /// </summary>
        [Range(0f, 2f)]
        public float cooldown = 0.1f;

        /// <summary>
        /// Distance moved during each movement.
        /// </summary>
        [Range(1, 3)]
        public int distance = 1;

        /// <summary>
        /// Layer on which collision will be checked.
        /// </summary>
        [SerializeField] private LayerMask blockingLayer;

        /// <summary>
        /// Layer containing floor tiles. Use to determine the bounds of the known map.
        /// </summary>
        [SerializeField] private LayerMask floorLayer;

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
        /// <param name="direction">X & Y directions to move. Constrained to a unit value.</param>
        /// <param name="hit">Object blocking movement or null if none.</param>
        /// <returns>True if movemet was successful. False otherwise.</returns>
        protected virtual bool Move(Vector2 direction, out RaycastHit2D hit)
        {
            Vector2 start = transform.position;
            var end = start + direction * distance;

            // Check if target position exists (i.e. has been populated by the OverWorld/Cave).
            var gameObjects = Utility.FindObjectsAt(start);
            Utility.SetActiveBoxColliders(gameObjects, false);
            var floorHit = Physics2D.Linecast(start, end);
            if (floorHit.transform == null)
            {
                if (OnTileNotFound != null) { OnTileNotFound(end); }
            }
            Utility.SetActiveBoxColliders(gameObjects, true);

            if (boxCollider != null ) { boxCollider.enabled = false; }
            hit = Physics2D.Linecast(start, end, blockingLayer);
            if (boxCollider != null ) { boxCollider.enabled = true; }

            if (hit.transform == null)
            {
                StartCoroutine(SmoothMovement(end));
                Facing = direction;
                if (OnCanMove != null) { OnCanMove(end); }
                if (OnStartMove != null)
                {
                    OnStartMove(gameObject, end);
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
            var start = transform.position;
            var sqrRemainingDistance = (start - end).sqrMagnitude;

            var hasRaisedMiddleOfMove = false;
            var midpoint = sqrRemainingDistance / 2f;

            while (sqrRemainingDistance > float.Epsilon)
            {
                if (!hasRaisedMiddleOfMove && sqrRemainingDistance < midpoint)
                {
                    hasRaisedMiddleOfMove = true;
                    if (OnReachedMiddleOfMove != null) { OnReachedMiddleOfMove(start, end); }
                }

                var newPostion = Vector3.MoveTowards(rb2D.position, end,
                    speed * Time.deltaTime);
                rb2D.MovePosition(newPostion);
                sqrRemainingDistance = (transform.position - end).sqrMagnitude;
                yield return null;
            }
            // TODO: snap to integer position

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
        /// <param name="direction">X & Y directions to move. Constrained to a unit value.</param>
        /// <returns>True if move started successfully, false otherwise.</returns>
        public virtual bool AttemptMove(Vector2 direction)
        {
            if (IsMoving) { return false; }

            // Constrain direction to a unit value (-1, 0, or 1).
            direction = Utility.MakeUnitLength(direction);

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
                var interactableComponent = hit.transform.GetComponent<Interactable>();
                if (interactableComponent != null)
                {
                    // Notify blocking object of collision.
                    interactableComponent.OnBlockObject(gameObject);
                }
                // Notify this object of collision.
                if (OnCantMove != null) { OnCantMove(hit.transform.gameObject); }

                return false;
            }
        }
    }
}
