using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Enums are ordered by increasing brightness (A > B == A brighter B).
    /// </summary>
    public enum Visibility { None, Half, Full }

    /// <summary>
    /// Container for multiple light source contributions. Maintains the highest visibility.
    /// </summary>
    public class AggregateVisibility
    {
        /// <summary>
        /// Highest visibility from all contributions.
        /// </summary>
        public Visibility VisibilityLevel { get; private set; }

        /// <summary>
        /// Number of contributions to each visibility level.
        /// </summary>
        private int[] contribution;

        public AggregateVisibility()
        {
            // Initialize contributions to zero.
            var numberOfLevels = Enum.GetValues(typeof(Visibility)).Length;
            contribution = new int[numberOfLevels];
            for (int level = 0; level < numberOfLevels; ++level) { contribution[level] = 0; }

            // Start at lowest visibility.
            VisibilityLevel = (Visibility)0;
        }

        public AggregateVisibility(Visibility startLevel) : this()
        {
            AddContribution(startLevel);
        }

        /// <summary>
        /// Add one contribution of the given level. Updates the highest VisibilityLevel.
        /// </summary>
        /// <param name="visibility">Level to add a contribution of.</param>
        public void AddContribution(Visibility visibility)
        {
            ++contribution[(int)visibility];
            if (visibility > VisibilityLevel) { VisibilityLevel = visibility; }
        }

        /// <summary>
        /// Remove one contribution of the given level. Updates the highest VisibilityLevel.
        /// </summary>
        /// <param name="visibility">Level to remove a contribution of.</param>
        public void RemoveContribution(Visibility visibility)
        {
            --contribution[(int)visibility];
            
            if (contribution[(int)visibility] == 0 && visibility == VisibilityLevel)
            {
                UpdateVisibilityLevel();
            }
        }

        /// <summary>
        /// Set VisibilityLevel to the highest level from among all contributions.
        /// </summary>
        private void UpdateVisibilityLevel()
        {
            for (int level = contribution.Length - 1; level >= 0; --level)
            {
                if (contribution[level] > 0)
                {
                    VisibilityLevel = (Visibility)level;
                    return;
                }
            }
            VisibilityLevel = (Visibility)0;
        }
    }
}
