using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
	public class LightSource : MonoBehaviour
	{
        public delegate void Illuminate(Vector2 location, List<Vector2> brightOffsets,
            List<Vector2> dimOffsetsBand);
        public static event Illuminate OnIlluminate;

        public delegate void LightSourceMoved(
            Vector2 startLocation, List<Vector2> startOffsets, List<Vector2> startDimOffsetsBand,
            Vector2   endLocation, List<Vector2>   endOffsets, List<Vector2>   endDimOffsetsBand);
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

                _brightRadius = value;

                var oldBrightOffsets = BrightOffsets;
                BrightOffsets = GridAlgorithms.GetCircularOffsets(_brightRadius);

                var oldDimOffsetsBand = DimOffsetsBand;
                DimRadius = _brightRadius + DimModifier(_brightRadius);

                if (OnLightSourceMoved != null)
                {
                    OnLightSourceMoved(
                        lastLocationIlluminated, oldBrightOffsets, oldDimOffsetsBand,
                        lastLocationIlluminated,    BrightOffsets,    DimOffsetsBand);
                }
            }
        }
        [Range(0,10)]
        [SerializeField] private int _brightRadius;

        public int DimRadius
        {
            get { return _dimRadius; }
            set
            {
                if (value < 0) { throw new System.ArgumentException("Radius cannot be negative."); }

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
        private int _dimRadius;

        /// <summary>
        /// Returns the number which is added to BrightRadius to get DimRadius.
        /// </summary>
        private DimModifierFunction DimModifier;
        private delegate int DimModifierFunction(int brightRadius);
        
        private enum DimModifierType { Flat, Matched, Fraction }
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
                case DimModifierType.Matched:
                    DimModifier = (int brightRadius) => { return brightRadius; };
                    break;
                case DimModifierType.Fraction:
                    DimModifier = (int brightRadius) => { return Mathf.CeilToInt(1f
                        / (float)dimModifierValue * brightRadius); };
                    break;
                default:
                    throw new System.ArgumentException("Unsupported dim modifier function type.");
            }

            // Prevent "oldOffsets" from being null during first OnLightSourceMoved event.
            BrightOffsets = GridAlgorithms.GetCircularOffsets(_brightRadius);
            DimRadius = _brightRadius + DimModifier(_brightRadius);
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
        /// Illuminates world at end location and de-illuminates world at start location. Use when
        /// Radius has not changed.
        /// </summary>
        public void MoveLightSource(Vector2 start, Vector2 end)
        {
            lastLocationIlluminated = end;
            if (OnLightSourceMoved != null)
            {
                OnLightSourceMoved(start, BrightOffsets, DimOffsetsBand,
                                     end, BrightOffsets, DimOffsetsBand);
            }
        }

        /// <summary>
        /// Illuminate darkness at the light source.
        /// </summary>
        /// <param name="timing">Start, Middle, or End of transition. Only fires on middle.</param>
        private void OnPassageTransition(GameManager.Timing timing, Vector2 passagePosiiton)
        {
            if (timing == GameManager.Timing.Middle)
            {
                IlluminateDarkness();
            }
        }

        /// <summary>
        /// Illuminates darkness at the light source's location.
        /// </summary>
        public void IlluminateDarkness()
        {
            lastLocationIlluminated = BoardManager.Constrain(transform.position);
            if (OnIlluminate != null)
            {
                OnIlluminate(lastLocationIlluminated, BrightOffsets, DimOffsetsBand);
            }
        }

        /// <summary>
        /// Notify OverWorld/Cave to remove this light source's contributions.
        /// </summary>
        private void OnDestroy()
        {
            if (OnLightSourceMoved != null)
            {
                OnLightSourceMoved(lastLocationIlluminated, BrightOffsets, DimOffsetsBand,
                    Vector2.zero, new List<Vector2>(), new List<Vector2>());
            }
        }
    }
}
