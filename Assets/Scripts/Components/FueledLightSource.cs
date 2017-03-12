using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    public class FueledLightSource : LightSource
    {
        // Wrap events raised from fuel.
        public event LimitedQuantityFloat.LostQuantity OnLostFuel
        {
            add { fuel.OnLostQuantity += value; }
            remove { fuel.OnLostQuantity -= value; }
        }

        public event LimitedQuantityFloat.GainedQuantity OnGainedFuel
        {
            add { fuel.OnGainedQuantity += value; }
            remove { fuel.OnGainedQuantity -= value; }
        }

        public event LimitedQuantityFloat.Depleted OnExtinguished
        {
            add { fuel.OnDepleted += value; }
            remove { fuel.OnDepleted += value; }
        }

        public delegate int ChangeIntensity(int radius);
        public static event ChangeIntensity OnChangeIntensity;

        /// <summary>
        /// Fuel (quantity and maximum allowed).
        /// </summary>
        public LimitedQuantityFloat fuel;

        [Range(0.0f, 5.0f)]
        /// <summary>
        /// Fuel per second consumed.
        /// </summary>
        public float burnRate = 1.0f;

        [Range(-2.0f, 2.0f)]
        /// <summary>
        /// Value added to BurnRate.
        /// </summary>
        public float burnRateModifier = 0.0f;

        /// <summary>
        /// Returns the radius of light as a function of fuel quantity.
        /// </summary>
        public delegate int IntensityFunction(float fuel);
        private IntensityFunction intensity;

        // TODO: create custom inspector for function parameter choice (i.e. y = m*x + b: m, b)
        public enum IntensityFunctionType { Linear, Logarithmic }
        [SerializeField] private IntensityFunctionType intensityFunctionType;

        [Range(0, 3)]
        /// <summary>
        /// Value added to Intensity.
        /// </summary>
        public int intensityModifier = 0;

        protected override void Awake()
        {
            base.Awake();
            fuel.Quantity = fuel.MaxQuantity;

            switch(intensityFunctionType)
            {
                case IntensityFunctionType.Linear:
                    intensity = LinearFunctionFactory(5f / 100f, 0f);
                    break;
                case IntensityFunctionType.Logarithmic:
                    throw new System.ArgumentException("Not yet implemented!");
                    //break;
                default:
                    throw new System.ArgumentException("Unsupported intensity function type.");
            }
        }

        private void Update()
        {
            if (fuel.Quantity > 0)
            {
                // Burn fuel.
                fuel.Quantity -= Time.deltaTime * (burnRate + burnRateModifier);
                fuel.Quantity = Mathf.Max(fuel.Quantity, 0f);

                // Update intensity.
                var newIntensity = Mathf.Max(intensity(fuel.Quantity), 0);
                if (newIntensity != Radius)
                {
                    Radius = newIntensity;
                    if (OnChangeIntensity!= null) { OnChangeIntensity(Radius); }
                }

            }
        }

        /// <summary>
        /// Returns a linear function of form: y = m*x + b;
        /// </summary>
        private IntensityFunction LinearFunctionFactory(float m, float b)
        {
            return (float x) => { return Mathf.CeilToInt(m * x + b); };
        }
    }
}
