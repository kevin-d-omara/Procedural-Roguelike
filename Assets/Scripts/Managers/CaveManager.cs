using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralRoguelike
{
    public class CaveManager : BoardManager
    {
        [Header("Path parameters:")]
        [SerializeField] private List<PathParameters> essentialPathParameterSet;
        [SerializeField] private List<PathParameters> majorPathParameterSet;
        [SerializeField] private List<PathParameters> minorPathParameterSet;
        [SerializeField] [Range(0f, 1f)] private float majorLevelChance = 1f;
        [SerializeField] [Range(0f, 1f)] private float minorLevelChance = 1f;

        private List<PathParameters> pathParameters = new List<PathParameters>();
        private Path caveSystem;

        protected override void Awake()
        {
            base.Awake();

            // For each set of path parameters, pick a single one for this CaveManager instance.
            pathParameters.Add(essentialPathParameterSet[Random.Range(0, 
                essentialPathParameterSet.Count)]);
            if (Random.value <= majorLevelChance)
            {
                pathParameters.Add(majorPathParameterSet[Random.Range(0,
                    majorPathParameterSet.Count)]);
            }
            if (Random.value <= minorLevelChance)
            {
                pathParameters.Add(minorPathParameterSet[Random.Range(0,
                    minorPathParameterSet.Count)]);
            }

            // Wire up holder GameObjects for organizational parenting.
            holders.Add("CaveExit", transform.Find("CaveExit"));

            caveSystem = new Path(pathParameters);
        }

        public override void SetupEntrance(Vector2 size, Vector2 position)
        {
            base.SetupEntrance(size, position);

            // Temporary exit spawning to test transition between Cave => OverWorld
            var positionV3 = new Vector3(position.x + 2, position.y + 2, 0);
            GameObject instance = Instantiate(passagePrefab, positionV3, Quaternion.identity)
                as GameObject;
            instance.transform.SetParent(holders["CaveExit"]);
        }

        /// <summary>
        /// Check the tiles surrounding 'location' and creates tile which are defined the the cave
        /// system, or Rocks otherwise.
        /// </summary>
        /// <param name="location">Location in the world to center the offsets.</param>
        /// <param name="offsets">List of offsets specifying pattern to reveal.</param>
        public override void RevealDarkness(Vector2 location, List<Vector2> offsets)
        {
            // TODO: reveal only rocks
        }

        /// <summary>
        /// Creates all the GameObjects defined by the cave system (i.e. Floor, Obstacles, Enemies,
        /// Chests, etc.).
        /// </summary>
        /// <param name="location">Location in the world to center the entrance passage.</param>
        public void SetupCave(Vector2 position)
        {
            // Fill in floor tiles for each path and chamber.
            //      Recursively fill in each path/fork
            //      Mark bottleneck regions
            //      Fill in chambers (except where overlap with bottleneck regions)
            //      Expand essential paths via Choke & Jitter

            // Fill the dungeon with loot and denizens.
            //      (optional) randomly pick entrance and exit locations (@ ForkPts and/or PathEnds)
            //      Place entrance and exit tiles
            //      Spawn gems
            //      Spawn chests
            //      Spawn enemies
            //      Spawn obstacles
            //          do not place non-bramble on choke 0 tiles

        }
    }
}
