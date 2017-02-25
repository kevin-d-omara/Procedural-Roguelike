using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class Attacker : MonoBehaviour
	{
        // Time that must be waited between making attacks.
        [Range(0f, 2f)]
        public float speed;

        // Time after beginning an attack before the attack happens. (i.e. the wind-up)
        [Range(0f, 0.5f)]
        public float delay;

        // How much damage is dealt.
        [Range(1, 5)]
        public int damage;

        // How far the linecast is sent to determine targets.
        [Range(0.75f, 2f)]
        public float range;
	}
}
