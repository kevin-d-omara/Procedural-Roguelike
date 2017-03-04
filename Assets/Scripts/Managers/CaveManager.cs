using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    public class CaveManager : BoardManager
    {
        [SerializeField] private GameObject exitPrefab;

        protected override void Awake()
        {
            base.Awake();

            // Wire up holder GameObjects for organizational parenting.
            holders.Add("CaveExit", transform.Find("CaveExit"));
        }

        public override void SetupEntrance(Vector2 size, Vector2 position)
        {
            base.SetupEntrance(size, position);

            // Temporary exit spawning to test transition between Cave => OverWorld
            var positionV3 = new Vector3(position.x + 2, position.y + 2, 0);
            GameObject instance = Instantiate(exitPrefab, positionV3, Quaternion.identity)
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
