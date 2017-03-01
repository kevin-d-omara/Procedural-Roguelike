using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    [RequireComponent(typeof(SpriteRenderer))]
	public class ChestController : Interactable
	{
        public override bool HasBeenUsed
        {
            get { return base.HasBeenUsed; }

            protected set
            {
                base.HasBeenUsed = value;
                if (HasBeenUsed)
                {
                    GetComponent<SpriteRenderer>().sprite = chestOpened;
                }
                else
                {
                    GetComponent<SpriteRenderer>().sprite = chestClosed;
                }
            }
        }

        [SerializeField] private Sprite chestClosed;
        [SerializeField] private Sprite chestOpened;

        public override void OnBlockObject(GameObject blockedObject)
        {
            if (!HasBeenUsed && blockedObject.tag == "Player")
            {
                Debug.Log("chest opened");
                HasBeenUsed = true;
            }
        }
    }
}
