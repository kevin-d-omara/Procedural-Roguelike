using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        protected void OnMakeDecision()
        {
            if (aiComponent.target.transform.position == Vector3.zero)
            {
                basicAttack = true;
            }
        }
    }
}
