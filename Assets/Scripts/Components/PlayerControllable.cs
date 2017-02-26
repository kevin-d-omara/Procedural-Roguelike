using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    [RequireComponent(typeof(Attack))]
    [RequireComponent(typeof(Health))]
	public class PlayerControllable : MonoBehaviour
	{
        // Input values.
        private int horizontal;
        private int vertical;
        private bool autoAttack;
        private bool specialAttack;

        // Componenets.
        private Moveable moveableComponent;
        private Attack attackComponent;
        private Health healthComponent;
        private LightSource lightSourceComponent;

        private void Awake()
        {
            // Find references to all components.
            moveableComponent = GetComponent<Moveable>();
            attackComponent = GetComponent<Attack>();
            healthComponent = GetComponent<Health>();
            lightSourceComponent = GetComponent<LightSource>();
        }

        private void OnEnable()
        {
            Moveable.OnCanMove += OnCanMove;
        }

        private void OnDisable()
        {
            Moveable.OnCanMove -= OnCanMove;
        }

        private void Update()
        {
            GetInputs();
            HandleMovement();
        }

        private void OnCanMove(Vector2 destination)
        {
            lightSourceComponent.IlluminateDarkness(destination);
        }

        private void HandleMovement()
        {
            // Limit movement to one axis per move.
            if (horizontal != 0) { vertical = 0; }

            if (horizontal != 0 || vertical != 0)
            {
                moveableComponent.AttemptMove(horizontal, vertical);
            }
        }

        /// <summary>
        /// Sets the value of all relevant input channels.
        /// </summary>
        private void GetInputs()
        {
            // Movement input.
            horizontal = (int)Input.GetAxisRaw("Horizontal");
            vertical   = (int)Input.GetAxisRaw("Vertical");

            // Attack input.
            autoAttack    = (int)Input.GetAxisRaw("Auto-Attack")    == 1;
            specialAttack = (int)Input.GetAxisRaw("Special Attack") == 1;
        }
    }
}
