using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Enums are ordered by increasing brightness (A > B == A brighter B).
    /// </summary>
    public enum Illumination { None, Low, Half, Full }

    /// <summary>
    /// Container for multiple light source contributions. Maintains the highest light level.
    /// </summary>
    public class AggregateIllumination
    {
        /// <summary>
        /// Highest light level from all contributions.
        /// </summary>
        public Illumination Brightness { get; private set; }

        /// <summary>
        /// Number of contributions to each light level.
        /// </summary>
        private int[] contribution;

        public AggregateIllumination()
        {
            // Initialize contributions to zero.
            var numberOfLevels = Enum.GetValues(typeof(Illumination)).Length;
            contribution = new int[numberOfLevels];
            for (int level = 0; level < numberOfLevels; ++level) { contribution[level] = 0; }

            // Start at lowest light level.
            Brightness = (Illumination)0;
        }

        public AggregateIllumination(Illumination startLevel) : this()
        {
            AddContribution(startLevel);
        }

        /// <summary>
        /// Add one contribution of the given level. Updates Brightness.
        /// </summary>
        /// <param name="luminosity">Level of light to add a contribution of.</param>
        public void AddContribution(Illumination luminosity)
        {
            ++contribution[(int)luminosity];
            if (luminosity > Brightness) { Brightness = luminosity; }
        }

        /// <summary>
        /// Remove one contribution of the given level. Updates Brightness.
        /// </summary>
        /// <param name="luminosity">Level of light to remove a contribution of.</param>
        public void RemoveContribution(Illumination luminosity)
        {
            --contribution[(int)luminosity];

            if (contribution[(int)luminosity] == 0 && luminosity == Brightness)
            {
                UpdateBrightness();
            }
        }

        /// <summary>
        /// Set Brightness to the highest light level from among all contributions.
        /// </summary>
        private void UpdateBrightness()
        {
            for (int level = contribution.Length - 1; level >= 0; --level)
            {
                if (contribution[level] > 0)
                {
                    Brightness = (Illumination)level;
                    return;
                }
            }
            Brightness = (Illumination)0;
        }
    }
}
