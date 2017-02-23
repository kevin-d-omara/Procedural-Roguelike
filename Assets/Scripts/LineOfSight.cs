using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class LineOfSight
	{
        public int Radius { get; set; }
        public List<Vector2> offsets { get; private set; }
	}
}
