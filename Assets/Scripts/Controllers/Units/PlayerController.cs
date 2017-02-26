﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Moveable))]
    [RequireComponent(typeof(Attack))]
    [RequireComponent(typeof(Health))]
    public class PlayerController : MonoBehaviour
    {
        // Input values.
        private int horizontal;
        private int vertical;
        private bool autoAttack;
        private bool specialAttack;

        // Componenets.
        private Animator animator;
        private Moveable moveableComponent;
        private Attack attackComponent;
        private Health healthComponent;
        private LightSource lightSourceComponent;

        private void Awake()
        {
            // Get references to all components.
            animator = GetComponent<Animator>();
            moveableComponent = GetComponent<Moveable>();
            attackComponent = GetComponent<Attack>();
            healthComponent = GetComponent<Health>();
            lightSourceComponent = GetComponent<LightSource>();
        }

        private void OnEnable()
        {
            moveableComponent.OnCanMove += OnCanMove;
            moveableComponent.OnCantMove += OnCantMove;
            healthComponent.OnLostHitPoints += OnTakeDamage;
            healthComponent.OnKilled += OnKilled;
        }

        private void OnDisable()
        {
            moveableComponent.OnCanMove -= OnCanMove;
            moveableComponent.OnCantMove -= OnCantMove;
            healthComponent.OnLostHitPoints -= OnTakeDamage;
            healthComponent.OnKilled += OnKilled;
        }

        private void Update()
        {
            GetInputs();
            HandleMovement();
            HandleChop();
        }

        /// <summary>
        /// Records the values of each relevant input channel.
        /// </summary>
        private void GetInputs()
        {
            // Movement input.
            horizontal = (int)Input.GetAxisRaw("Horizontal");
            vertical = (int)Input.GetAxisRaw("Vertical");

            // Attack input.
            autoAttack = (int)Input.GetAxisRaw("Chop Attack") == 1;
            specialAttack = (int)Input.GetAxisRaw("Throw Dynamite") == 1;
        }

        /// <summary>
        /// Passes input along to the Moveable componenet and other interested componenets.
        /// </summary>
        private void HandleMovement()
        {
            // Limit movement to one axis per move.
            if (horizontal != 0) { vertical = 0; }

            if (!moveableComponent.IsMoving && (horizontal != 0 || vertical != 0))
            {
                moveableComponent.AttemptMove(new Vector2(horizontal, vertical));
            }
        }

        private void HandleChop()
        {
            if (autoAttack)
            {
                if (attackComponent.DoAttack(moveableComponent.Facing))
                {
                    animator.SetTrigger("playerChop");
                }
            }
        }

        private void OnCanMove(Vector2 destination)
        {
            lightSourceComponent.IlluminateDarkness(destination);
        }

        private void OnTakeDamage(int damage)
        {
            if (damage > 0)
            {
                animator.SetTrigger("playerHit");
            }
        }

        private void OnKilled()
        {
            Debug.Log("Player killed!");
        }

        /// <summary>
        /// Action the Player takes when movement is blocked. For example, opening a chest, entering
        /// a dungeon, etc.
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