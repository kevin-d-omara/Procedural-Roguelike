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

        public override void RevealDarkness(Vector2 location, List<Vector2> offsets)
        {
            //throw new NotImplementedException();
            // do nothing
        }
    }
}
