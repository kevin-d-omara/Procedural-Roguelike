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
    public class ZombieController : MonoBehaviour
	{
        // Componenets.
        private Animator animator;
        private Moveable moveableComponent;
        private Attack attackComponent;
        private Health healthComponent;

        private void Awake()
        {
            // Get references to all components.
            animator = GetComponent<Animator>();
            moveableComponent = GetComponent<Moveable>();
            attackComponent = GetComponent<Attack>();
            healthComponent = GetComponent<Health>();
        }
    }
}
