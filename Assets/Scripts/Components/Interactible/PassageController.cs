﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// The passage between two 'worlds' (i.e. OverWorld and Dungeon).
    /// </summary>
	public class PassageController : Interactable
	{
        public delegate void EnterDungeon(Vector2 position);
        public static event EnterDungeon OnEnterDungeon;

        public delegate void ExitDungeon(Vector2 position);
        public static event ExitDungeon OnExitDungeon;

        private enum Type { Entrance, Exit }
        [SerializeField] private Type type;

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (HasBeenUsed) { return; }

            if (collision.attachedRigidbody.tag == "Player")
            {
                HasBeenUsed = true;
                if (type == Type.Entrance)
                {
                    if (OnEnterDungeon != null) { OnEnterDungeon(transform.position); }
                }
                else if (type == Type.Exit)
                {
                    if (OnExitDungeon != null) { OnExitDungeon(transform.position); }
                }
            }
        }
    }
}
