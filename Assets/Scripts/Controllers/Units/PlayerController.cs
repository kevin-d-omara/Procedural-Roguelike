using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Moveable))]
    [RequireComponent(typeof(Attack))]
    [RequireComponent(typeof(Health))]
    public class PlayerController : UnitController
    {
        // Input values.
        private bool specialAttack;

        // Animations
        protected override string AnimationBasicAttack { get { return "playerChop"; } }
        protected override string AnimationHit         { get { return "playerHit"; } }

        protected override void OnEnable()
        {
            base.OnEnable();
            moveableComponent.OnReachedMiddleOfMove += OnReachedMiddleOfMove;
            moveableComponent.OnCantMove += OnCantMove;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            moveableComponent.OnReachedMiddleOfMove -= OnReachedMiddleOfMove;
            moveableComponent.OnCantMove -= OnCantMove;
        }

        /// <summary>
        /// Records the values of each relevant input channel.
        /// </summary>
        protected override void GetInputs()
        {
            // Movement input.
            horizontalInput = (int)Input.GetAxisRaw("Horizontal");
            verticalInput = (int)Input.GetAxisRaw("Vertical");

            // Attack input.
            basicAttack = (int)Input.GetAxisRaw("Chop Attack") == 1;
            specialAttack = (int)Input.GetAxisRaw("Throw Dynamite") == 1;
        }

        protected override void ResetInputs()
        {
            base.ResetInputs();
            specialAttack = false;
        }

        protected override void HandleMovement()
        {
            base.HandleMovement();

            // Allow Player to quickly change facing to make attacking easier.
            if (horizontalInput != 0 || verticalInput != 0)
            {
                moveableComponent.Facing = new Vector2(horizontalInput, verticalInput);
                basicAttackDir = moveableComponent.Facing;
            }
        }

        protected override void OnKilled()
        {
            Debug.Log("Player killed!");
        }

        /// <summary>
        /// Action the Player takes when movement is blocked. For example, opening a chest, entering
        /// a cave, etc.
        /// </summary>
        private void OnCantMove(GameObject blockingObject)
        {
            switch (blockingObject.tag)
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
