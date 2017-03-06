using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    public enum Visibility { Full, Half, None }

    /// <summary>
    /// Gives the attached GameObject three leveles of visibility: {0, 50, 100}%
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
	public class Visible : MonoBehaviour
	{
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
                case Visibility.Full:
                    color = Color.white;
                    break;

                case Visibility.Half:
                    color = Color.gray;
                    break;

                case Visibility.None:
                    color.a = 0f;
                    break;

                default:
                    throw new System.ArgumentException("Unsupported Visibility");
            }
            spriteRenderer.color = color;
        }
    }
}
