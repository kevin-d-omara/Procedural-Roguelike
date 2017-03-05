using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Gives the attached GameObject three leveles of visibility: {0, 50, 100}%
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
	public class Visible : MonoBehaviour
	{
        public enum Visibility { Visible, HalfVisible, Invisible }

        public Visibility VisibilityLevel
        {
            get { return _visibilityLevel; }
            set
            {
                _visibilityLevel = value;
                UpdateSprite();
            }
        }
        [SerializeField] private Visibility _visibilityLevel;

        // Components
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            // Get references to all components.
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            UpdateSprite();
        }

        /// <summary>
        /// Updates the alpha value of the attached sprite.
        /// </summary>
        public void UpdateSprite()
        {
            var color = spriteRenderer.color;
            switch(VisibilityLevel)
            {
                case Visibility.Visible:
                    color.a = 1f;
                    break;

                case Visibility.HalfVisible:
                    color.a = 0.5f;
                    break;

                case Visibility.Invisible:
                    color.a = 0f;
                    break;

                default:
                    throw new System.ArgumentException("Unsupported Visibility");
            }
            spriteRenderer.color = color;
        }
    }
}
