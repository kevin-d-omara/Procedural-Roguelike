﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Provides the basic functionality of all units.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Moveable))]
    [RequireComponent(typeof(Attack))]
    [RequireComponent(typeof(Health))]
    public abstract class UnitController : MonoBehaviour
	{
        // Input values.
        protected int horizontalInput;
        protected int verticalInput;
        protected bool basicAttack;
        protected Vector2 basicAttackDir;

        // Componenets.
        protected SpriteRenderer spriteRenderer;
        protected Animator animator;
        protected Moveable moveableComponent;
        protected Attack attackComponent;
        protected Health healthComponent;

        // Optional Components
        protected LightSource lightSourceComponent;

        // Animations
        // Names for the trigger associated with each basic animation.
        protected virtual string AnimationBasicAttack { get { return null; } }
        protected virtual string AnimationHit { get { return null; } }

        // State
        protected virtual bool IsPaused { get; set; }

        protected virtual void Awake()
        {
            // Get references to all components.
            spriteRenderer       = GetComponent<SpriteRenderer>();
            animator             = GetComponent<Animator>();
            moveableComponent    = GetComponent<Moveable>();
            attackComponent      = GetComponent<Attack>();
            healthComponent      = GetComponent<Health>();
            lightSourceComponent = GetComponent<LightSource>();

            // Set state.
            IsPaused = false;
        }

        protected virtual void Start() { }

        protected virtual void OnEnable()
        {
            moveableComponent.OnFlippedDirectionX += OnFlippedDirection;
            healthComponent.OnLostHitPoints += OnTakeDamage;
            healthComponent.OnKilled += OnKilled;
            GameManager.OnPassageTransition += OnPassageTransition;

            if (lightSourceComponent != null)
            {
                moveableComponent.OnReachedMiddleOfMove += OnReachedMiddleOfMove;
            }
        }

        protected virtual void OnDisable()
        {
            moveableComponent.OnFlippedDirectionX -= OnFlippedDirection;
            healthComponent.OnLostHitPoints -= OnTakeDamage;
            healthComponent.OnKilled += OnKilled;
            GameManager.OnPassageTransition -= OnPassageTransition;

            if (lightSourceComponent != null)
            {
                moveableComponent.OnReachedMiddleOfMove -= OnReachedMiddleOfMove;
            }
        }


        protected virtual void Update()
        {
            if (IsPaused) { return; }

            GetInputs();
            HandleMovement();
            HandleBasicAttack();
            HandleSpecialActions();
            ResetInputs();
        }

        /// <summary>
        /// Records the values of each relevant input channel.
        /// </summary>
        protected abstract void GetInputs();

        /// <summary>
        /// Passes input along to the Moveable componenet and other interested componenets.
        /// </summary>
        protected virtual void HandleMovement()
        {
            // Limit movement to one axis per move.
            if (horizontalInput != 0) { verticalInput = 0; }

            if (!moveableComponent.IsMoving && (horizontalInput != 0 || verticalInput != 0))
            {
                moveableComponent.AttemptMove(new Vector2(horizontalInput, verticalInput));
            }
        }

        protected virtual void HandleBasicAttack()
        {
            if (basicAttack)
            {
                if (attackComponent.DoAttack(basicAttackDir))
                {
                    if (AnimationBasicAttack != null) { animator.SetTrigger(AnimationBasicAttack); }
                }
            }
        }

        /// <summary>
        /// Do nothing. Must override in child class to gain functionality. Called before
        /// ResetInputs().
        /// </summary>
        protected virtual void HandleSpecialActions() { }

        /// <summary>
        /// Sets all input values to 0. Prevents glitches where a unit continually moves after a
        /// single input.
        /// </summary>
        protected virtual void ResetInputs()
        {
            horizontalInput = 0;
            verticalInput = 0;
            basicAttack = false;
        }

        protected virtual void OnReachedMiddleOfMove(Vector2 start, Vector2 destination)
        {
            lightSourceComponent.MoveLightSource(start, destination);
        }

        protected virtual void OnFlippedDirection(bool flipX)
        {
            spriteRenderer.flipX = flipX;
        }

        protected virtual void OnTakeDamage(int damage)
        {
            if (damage > 0)
            {
                if (AnimationHit != null) { animator.SetTrigger(AnimationHit); }
            }
        }

        protected virtual void OnKilled()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Disable/enable Unit actions. Stops combat from occuring during transitions.
        /// </summary>
        private void OnPassageTransition(GameManager.Timing timing, Vector2 _)
        {
            if (timing == GameManager.Timing.Start)
            {
                IsPaused = true;
            }
            else if (timing == GameManager.Timing.End)
            {
                IsPaused = false;
            }
        }
    }
}
