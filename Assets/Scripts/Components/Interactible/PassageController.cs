using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// The passage between two 'worlds' (i.e. OverWorld and Cave).
    /// </summary>
	public class PassageController : Interactable
	{
        public delegate void EnterCave(Vector2 position);
        public static event EnterCave OnEnterCave;

        public delegate void ExitCave(Vector2 position);
        public static event ExitCave OnExitCave;

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
                    if (OnEnterCave != null) { OnEnterCave(transform.position); }
                }
                else if (type == Type.Exit)
                {
                    if (OnExitCave != null) { OnExitCave(transform.position); }
                }
            }
        }
    }
}
