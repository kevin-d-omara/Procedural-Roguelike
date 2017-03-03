using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    [RequireComponent(typeof(Health))]
	public class RockController : MonoBehaviour
	{
        // Components
        private Health healthComponent;

        private void Awake()
        {
            // Get references to all components.
            healthComponent = GetComponent<Health>();
        }

        private void OnEnable()
        {
            healthComponent.OnKilled += OnRockDestroyed;
        }

        private void OnDisable()
        {
            healthComponent.OnKilled -= OnRockDestroyed;
        }

        private void OnRockDestroyed()
        {
            Destroy(gameObject);
        }
    }
}
