using System.Linq;
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

        public List<Vector2> BrightOffsets { get; private set; }
        public List<Vector2> DimOffsets { get; private set; }
        public List<Vector2> DimOffsetsBand { get; private set; }

        public int BrightRadius
        {
            get { return _brightRadius; }
            set
            {
                if (value < 0) { throw new System.ArgumentException("Radius cannot be negative."); }

                if (value != _brightRadius)
                {
                    _brightRadius = value;

                    var oldBrightOffsets = BrightOffsets;
                    BrightOffsets = GridAlgorithms.GetCircularOffsets(_brightRadius);

                    DimRadius = _brightRadius + DimModifier(_brightRadius);

                    if (OnLightSourceMoved != null)
                    {
                        OnLightSourceMoved(lastLocationIlluminated, oldBrightOffsets,
                                           lastLocationIlluminated, BrightOffsets);
                    }
                }
            }
        }
        [SerializeField] private int _brightRadius;

        public int DimRadius
        {
            get { return _dimRadius; }
            set
            {
                if (value < 0) { throw new System.ArgumentException("Radius cannot be negative."); }

                if (value != _dimRadius)
                {
                    _dimRadius = value;
                    DimOffsets = GridAlgorithms.GetCircularOffsets(_dimRadius);

                    // Calculate DimOffsetsBand
                    var dimOffsetsBand = new HashSet<Vector2>();
                    foreach (Vector2 dimOffset in DimOffsets)
                    {
                        dimOffsetsBand.Add(dimOffset);
                    }
                    foreach (Vector2 brightOffset in BrightOffsets)
                    {
                        dimOffsetsBand.Remove(brightOffset);
                    }
                    DimOffsetsBand = dimOffsetsBand.ToList();
                }
            }
        }
        private int _dimRadius;

        /// <summary>
        /// Returns the number which is added to BrightRadius to get DimRadius.
        /// </summary>
        private DimModifierFunction DimModifier;
        private delegate int DimModifierFunction(int brightRadius);
        
        private enum DimModifierType { Flat, Fraction }
        [SerializeField] private DimModifierType dimModifierType = DimModifierType.Flat;

        [Range(0, 4)]
        [SerializeField] private int dimModifierValue = 1;

        private Vector2 lastLocationIlluminated;

        protected virtual void Awake()
        {
            switch(dimModifierType)
            {
                case DimModifierType.Flat:
                    DimModifier = (int brightRadius) => { return dimModifierValue; };
                    break;
                case DimModifierType.Fraction:
                    DimModifier = (int brightRadius) => { return Mathf.CeilToInt(1f
                        / (float)dimModifierValue); };
                    break;
                default:
                    throw new System.ArgumentException("Unsupported dim modifier function type.");
            }

            BrightRadius = _brightRadius;
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
            if (OnIlluminate != null) { OnIlluminate(location, BrightOffsets); }
        }

        /// <summary>
        /// Illuminates world at end location and de-illuminates world at start location. Use when
        /// Radius has not changed.
        /// </summary>
        public void MoveLightSource(Vector2 start, Vector2 end)
        {
            lastLocationIlluminated = end;
            if (OnLightSourceMoved != null) { OnLightSourceMoved(start, BrightOffsets, end, BrightOffsets); }
        }

        private void OnPassageTransition(GameManager.Timing timing, Vector2 passagePosiiton)
        {
            lastLocationIlluminated = passagePosiiton;
            if (timing == GameManager.Timing.Middle) { IlluminateDarkness(passagePosiiton); }
        }
    }
}
