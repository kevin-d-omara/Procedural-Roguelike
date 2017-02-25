using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class Zombie : MovingObject
	{
        // Time (seconds) between attacks.
        [Range(0f,2f)]
        public float attackSpeed = 0.5f;

        [Range(1,5)]
        public int attackDamage = 1;

        protected override void OnCantMove(GameObject blockingObject)
        {
            // TODO: do something
        }
    }
}
