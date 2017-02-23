using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class LineOfSight
	{
        private int _radius;
        public int Radius
        {
            get { return _radius; }
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentException("Radius cannot be negative.");
                }
                _radius = value;
                RecalculateOffsets();
            }
        }
        public List<Vector2> offsets { get; private set; }

        public LineOfSight(int radius)
        {
            Radius = radius;
        }

        private void RecalculateOffsets()
        {

        }
	}
}
