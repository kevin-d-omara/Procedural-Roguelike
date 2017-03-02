using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class LightSource : MonoBehaviour
	{
        public delegate void Illuminate(Vector2 location, List<Vector2> offsets);
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

        private void Awake()
        {
            Radius = _radius;
        }

        private void OnEnable()
        {
            GameManager.OnPassageTransition += OnPassageTransition;
        }

        private void OnDisable()
        {
            GameManager.OnPassageTransition -= OnPassageTransition;
        }

        private void RecalculateOffsets()
        {
            Offsets = GridAlgorithms.CircleFill(Radius);
        }

        public void IlluminateDarkness(Vector2 location)
        {
            if (OnIlluminate != null)
            {
                OnIlluminate(location, Offsets);
            }
        }

        private void OnPassageTransition(GameManager.Timing timing, Vector2 passagePosiiton)
        {
            if (timing == GameManager.Timing.Middle)
            {
                IlluminateDarkness(passagePosiiton);
            }
        }
    }
}
