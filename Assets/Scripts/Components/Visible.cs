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

        /// <summary>
        /// Type of visible object: Ambient or Entity.
        /// </summary>
        public Type ObjectType { get { return _objectType; } }
        [SerializeField] private Type _objectType = Type.Ambient;
        public enum Type { Ambient, Entity }

        // Components
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            // Get references to all components.
            spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.color = new Color(1f, 1f, 1f, 0f);
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
            var rate = 2f;
            Color color = spriteRenderer.color;
            switch(ObjectType)
            {
                case Type.Ambient:
                    switch (VisibilityLevel)
                    {
                        case Visibility.Full:
                            color = Color.white;
                            break;

                        case Visibility.Half:
                            var brightness = 0.35f;
                            color = new Color(brightness, brightness, brightness, 1f);
                            break;

                        case Visibility.None:
                            color = spriteRenderer.color;
                            color.a = 0f;
                            break;

                        default:
                            throw new System.ArgumentException("Unsupported Visibility");
                    }
                    spriteRenderer.color = color;
                    break;

                case Type.Entity:
                    switch (VisibilityLevel)
                    {
                        case Visibility.Full:
                            color = new Color(1f, 1f, 1f, 1f);
                            StartCoroutine(LerpColor(color, rate, Visibility.Full));
                            break;

                        case Visibility.Half:
                            var brightness = 0.35f;
                            color = new Color(brightness, brightness, brightness, 1f);
                            StartCoroutine(LerpColor(color, rate, Visibility.Half));
                            break;

                        case Visibility.None:
                            color.a = 0f;
                            StartCoroutine(LerpColor(color, rate, Visibility.None));
                            break;

                        default:
                            throw new System.ArgumentException("Unsupported Visibility");
                    }
                    break;

                default:
                    throw new System.ArgumentException("Unsupported Visibility");
            }
        }

        /// <summary>
        /// Changes the current color towards the target color at the specified rate.
        /// </summary>
        /// <param name="speed">Time to go from 1 to 0. (i.e. rate=2.0 -> 0.5 second lerp).</param>
        private IEnumerator LerpColor(Color target, float speed, Visibility targetVisibility)
        {
            var start = spriteRenderer.color;

            bool isMakingBrighter = true;
            if (target.a < start.a || target.r < start.r)
            {
                isMakingBrighter = false;
                speed *= -1f;
            }
            
            // Kill speed for color channels which aren't supposed to change.
            var speedR = target.r - start.r == 0 ? 0f : speed;
            var speedG = target.g - start.g == 0 ? 0f : speed;
            var speedB = target.b - start.b == 0 ? 0f : speed;
            var speedA = target.a - start.a == 0 ? 0f : speed;

            // Calculate bounds for each color channel.
            var minR =  isMakingBrighter ? start.r : target.r;
            var maxR = !isMakingBrighter ? start.r : target.r;
            var minG =  isMakingBrighter ? start.g : target.g;
            var maxG = !isMakingBrighter ? start.g : target.g;
            var minB =  isMakingBrighter ? start.b : target.b;
            var maxB = !isMakingBrighter ? start.b : target.b;
            var minA =  isMakingBrighter ? start.a : target.a;
            var maxA = !isMakingBrighter ? start.a : target.a;

            while (true)
            {
                var current = spriteRenderer.color;

                // Stop when another LerpColor is called or the target color is reached.
                if (VisibilityLevel != targetVisibility || current == target) { break; }

                var r = Mathf.Clamp(current.r + Time.deltaTime * speedR, minR, maxR);
                var g = Mathf.Clamp(current.g + Time.deltaTime * speedG, minG, maxG);
                var b = Mathf.Clamp(current.b + Time.deltaTime * speedB, minB, maxB);
                var a = Mathf.Clamp(current.a + Time.deltaTime * speedA, minA, maxA);
                spriteRenderer.color = new Color(r, g, b, a);

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
