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
        private Attack attackComponent;
        private Health healthComponent;

        private void Awake()
        {
            // Find references to all components.
            attackComponent = GetComponent<Attack>();
            healthComponent = GetComponent<Health>();
        }

        private void Update()
        {
            GetInputs();

            if (autoAttack)
            {
                attackComponent.DoAttack(new Vector2(1,0));
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
