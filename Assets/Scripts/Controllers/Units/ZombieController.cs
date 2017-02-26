using System;
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
    public class ZombieController : UnitController
    {
        protected override string AnimationBasicAttack { get { return "zombieAttack"; } }

        protected override void GetInputs()
        {
            basicAttack = true;
        }
    }
}
