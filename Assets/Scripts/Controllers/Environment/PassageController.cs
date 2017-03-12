using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// The passage between two 'worlds' (i.e. OverWorld and Cave).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(TwoSidedTile))]
    public class PassageController : Interactable
	{
        public delegate void EnterPassage(Vector2 position, PassageController passageController);
        public static event EnterPassage OnEnterPassage;

        public override bool HasBeenUsed
        {
            get { return base.HasBeenUsed; }

            set
            {
                base.HasBeenUsed = value;
                UpdateSprite();
            }
        }

        // Componenets
        private TwoSidedTile twoSidedTileComponent;

        protected override void Awake()
        {
            base.Awake();

            // Get references to all components.
            twoSidedTileComponent = GetComponent<TwoSidedTile>();
        }

        private void Start()
        {
            UpdateSprite();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (HasBeenUsed) { return; }

            if (collision.attachedRigidbody.tag == "Player")
            {
                HasBeenUsed = true;
                if (OnEnterPassage != null) { OnEnterPassage(transform.position, this); }
            }
        }

        /// <summary>
        /// Change the passage sprite to match the HasBeenUsed state.
        /// </summary>
        public void UpdateSprite()
        {
            StartCoroutine(UpdateSpriteDelayed(0.25f));
        }

        private IEnumerator UpdateSpriteDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (twoSidedTileComponent == null) twoSidedTileComponent = GetComponent<TwoSidedTile>();

            if (HasBeenUsed) { twoSidedTileComponent.SetSpriteToBack(); }
            else { twoSidedTileComponent.SetSpriteToFront(); }
        }
    }
}
