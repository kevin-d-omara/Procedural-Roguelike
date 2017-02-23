﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    public abstract class MovingObject : MonoBehaviour
    {
        public bool IsMoving { get; private set; }

        // Time (seconds) it takes the object to move.
        [SerializeField] private float moveTime = 0.1f;
                         private float inverseMoveTime;
        // Time (seconds) to wait before the next movement is allowed.
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
                // Successfully start moving.
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
            var totalMoveTime = 0f;

            var sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            while (sqrRemainingDistance > float.Epsilon)
            {
                var newPostion = Vector3.MoveTowards(rb2D.position, end,
                    inverseMoveTime * Time.deltaTime);
                rb2D.MovePosition(newPostion);
                sqrRemainingDistance = (transform.position - end).sqrMagnitude;
                totalMoveTime += Time.deltaTime;
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
        protected virtual void AttemptMove(int xDir, int yDir)
        {
            if (IsMoving) { return; }

            RaycastHit2D hit;
            bool canMove = Move(xDir, yDir, out hit);

            if (canMove)
            {
                IsMoving = true;
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
