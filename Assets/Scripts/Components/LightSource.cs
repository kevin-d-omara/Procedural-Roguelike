﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class LightSource : MonoBehaviour
	{
        public delegate void Illuminate(List<Vector2> offsets);
        public static event Illuminate OnIlluminate;

        [SerializeField] private int _radius;
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
        public List<Vector2> Offsets { get; private set; }

        private void RecalculateOffsets()
        {
            Offsets = GridAlgorithms.CircleFill(Radius);
        }

        public void IlluminateDarkness()
        {
            if (OnIlluminate != null)
            {
                OnIlluminate(Offsets);
            }
        }
    }
}
