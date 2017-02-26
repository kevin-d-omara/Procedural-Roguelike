using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class ZombieController : MonoBehaviour
	{
        // Componenets.
        private Moveable moveableComponent;
        private Attack attackComponent;
        private Health healthComponent;

        private void Awake()
        {
            // Find references to all components.
            moveableComponent = GetComponent<Moveable>();
            attackComponent = GetComponent<Attack>();
            healthComponent = GetComponent<Health>();
        }
    }
}
