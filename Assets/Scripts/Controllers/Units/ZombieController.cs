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

        protected override void Start()
        {
            // Match difficulty.

        }

        protected override void GetInputs() { }

        /// <summary>
        /// Called each time an Intelligence component thread "thinks".
        /// </summary>
        protected void OnMakeDecision(Intelligence.Mode mode, float distanceToTarget)
        {
            // Assault target.
            if (mode == Intelligence.Mode.Threat)
            {
                var dx = aiComponent.target.position.x - transform.position.x;
                var dy = aiComponent.target.position.y - transform.position.y;
                var dxAbs = Math.Abs(dx);
                var dyAbs = Math.Abs(dy);
                var rng = attackComponent.range;

                // Target is adjacent (non-diagonal) & within range -> Attack.
                if (!(dxAbs != 0 && dyAbs !=0) && (Mathf.Max(dxAbs, dyAbs) <= rng))
                {
                    basicAttack = true;
                    basicAttackDir = new Vector2(dx, dy);
                }
                // Target is out of attack-range -> Pursue.
                else
                {
                    // Move toward Player.
                    horizontalInput = dxAbs > dyAbs ? (int)Math.Sign(dx) : 0;
                    verticalInput   = dxAbs > dyAbs ? 0 : (int)Math.Sign(dy);
                }
            }

            // Wander aimlessley.
            else if (mode == Intelligence.Mode.Wander)
            {
                var direction = Random.Range(0.0f, 1.0f) < 0.5f ? -1 : +1;
                if (Random.Range(0.0f, 1.0f) <= 0.5f)
                {
                    horizontalInput = direction;
                }
                else
                {
                    verticalInput = direction;
                }
            }
        }
    }
}
