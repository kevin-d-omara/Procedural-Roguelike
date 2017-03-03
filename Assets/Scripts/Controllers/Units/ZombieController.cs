using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Intelligence))]
    [RequireComponent(typeof(Moveable))]
    [RequireComponent(typeof(Attack))]
    [RequireComponent(typeof(Health))]
    public class ZombieController : UnitController
    {
        // Componenets.
        protected Intelligence aiComponent;

        protected override string AnimationBasicAttack { get { return "zombieAttack"; } }

        protected override void Awake()
        {
            base.Awake();
            aiComponent = GetComponent<Intelligence>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            aiComponent.OnMakeDecision += OnMakeDecision;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            aiComponent.OnMakeDecision -= OnMakeDecision;
        }

        protected override void GetInputs() { }

        /// <summary>
        /// Called each time the Intelligence component "thinks".
        /// </summary>
        protected void OnMakeDecision(Intelligence.Mode mode, float distanceToTarget)
        {
            switch (mode)
            {
                // Pursue player.
                case Intelligence.Mode.Threat:
                    basicAttack = true;
                    break;

                // Wander aimlessly.
                case Intelligence.Mode.Wander:
                    var direction = Random.Range(0.0f, 1.0f) < 0.5f ? -1 : +1;
                    if (Random.Range(0.0f, 1.0f) <= 0.5f)
                    {
                        horizontalInput = direction;
                    }
                    else
                    {
                        verticalInput = direction;
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
