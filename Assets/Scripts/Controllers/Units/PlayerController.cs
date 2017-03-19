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
        private bool useDynamite;

        // Prefabs
        [SerializeField] private GameObject dynamitePrefab;

        // Animations
        protected override string AnimationBasicAttack { get { return "playerChop"; } }
        protected override string AnimationHit         { get { return "playerHit"; } }

        protected override void OnEnable()
        {
            base.OnEnable();
            moveableComponent.OnCantMove += OnCantMove;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
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
            useDynamite = Input.GetButtonDown("Use Dynamite");
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

        protected override void HandleSpecialActions()
        {
            if (useDynamite)
            {
                var position = Utility.Constrain(transform.position);
                var instance = Instantiate(dynamitePrefab, position, Quaternion.identity)
                    as GameObject;
                var dynamite = instance.GetComponent<DynamiteController>();

                dynamite.LightFuse();
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
