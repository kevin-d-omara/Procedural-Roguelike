using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class LightSource : MonoBehaviour
	{
        public delegate void Illuminate(Vector2 location, List<Vector2> offsets);
        public static event Illuminate OnIlluminate;

        public delegate void LightSourceMoved(Vector2 startLocation, List<Vector2> startOffsets,
                                              Vector2 endLocation,   List<Vector2> endOffsets);
        public static event LightSourceMoved OnLightSourceMoved;

        /// <summary>
        /// Radius of the light source (meters).
        /// </summary>
        public int Radius
        {
            get { return _radius; }
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentException("Radius cannot be negative.");
                }

                if (value != _radius)
                {
                    _radius = value;

                    var oldOffsets = Offsets;
                    Offsets = GridAlgorithms.GetCircularOffsets(Radius);

                    if (OnLightSourceMoved != null)
                    {
                        OnLightSourceMoved(lastLocationIlluminated, oldOffsets,
                                           lastLocationIlluminated, Offsets);
                    }
                }
            }
        }
        [SerializeField] private int _radius;

        private Vector2 lastLocationIlluminated;

        /// <summary>
        /// Describes the pattern of illumination.
        /// </summary>
        public List<Vector2> Offsets { get; private set; }

        protected virtual void Awake()
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

        /// <summary>
        /// Illuminates darkness at the specified location.
        /// </summary>
        public void IlluminateDarkness(Vector2 location)
        {
            lastLocationIlluminated = location;
            if (OnIlluminate != null) { OnIlluminate(location, Offsets); }
        }

        /// <summary>
        /// Illuminates world at end location and de-illuminates world at start location. Use when
        /// Radius has not changed.
        /// </summary>
        public void MoveLightSource(Vector2 start, Vector2 end)
        {
            lastLocationIlluminated = end;
            if (OnLightSourceMoved != null) { OnLightSourceMoved(start, Offsets, end, Offsets); }
        }

        private void OnPassageTransition(GameManager.Timing timing, Vector2 passagePosiiton)
        {
            lastLocationIlluminated = passagePosiiton;
            if (timing == GameManager.Timing.Middle) { IlluminateDarkness(passagePosiiton); }
        }
    }
}
