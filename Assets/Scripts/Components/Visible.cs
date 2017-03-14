using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Allows the attached GameObject's sprite to be affected by light levels.
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
                    var rate = 1.5f;
                    switch (VisibilityLevel)
                    {
                        case Visibility.Full:
                            rate = 2.0f;
                            color = new Color(1f, 1f, 1f, 1f);
                            StartCoroutine(LerpColor(color, rate, Visibility.Full));
                            break;

                        case Visibility.Half:
                            rate = 1.5f;
                            var brightness = 0.35f;
                            color = new Color(brightness, brightness, brightness, 1f);
                            StartCoroutine(LerpColor(color, rate, Visibility.Half));
                            break;

                        case Visibility.None:
                            rate = 1.0f;
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
            
            // Calculate bounds for each color channel.
            var minR =  isMakingBrighter ? start.r : target.r;
            var maxR = !isMakingBrighter ? start.r : target.r;
            var minG =  isMakingBrighter ? start.g : target.g;
            var maxG = !isMakingBrighter ? start.g : target.g;
            var minB =  isMakingBrighter ? start.b : target.b;
            var maxB = !isMakingBrighter ? start.b : target.b;
            var minA =  isMakingBrighter ? start.a : target.a;
            var maxA = !isMakingBrighter ? start.a : target.a;

            // Kill speed for color channels which aren't supposed to change, set otherwise.
            var speedR = target.r - start.r == 0 ? 0f : speed * maxR / minR;
            var speedG = target.g - start.g == 0 ? 0f : speed * maxR / minR;
            var speedB = target.b - start.b == 0 ? 0f : speed * maxR / minR;
            var speedA = target.a - start.a == 0 ? 0f : speed * maxR / minR;

            var timeR = isMakingBrighter ? 0f : 1f;
            var timeG = isMakingBrighter ? 0f : 1f;
            var timeB = isMakingBrighter ? 0f : 1f;
            var timeA = isMakingBrighter ? 0f : 1f;

            while (true)
            {
                var current = spriteRenderer.color;

                // Stop when another LerpColor is called or the target color is reached.
                if (VisibilityLevel != targetVisibility || current == target) { break; }

                timeR += Time.deltaTime * speedR;
                timeG += Time.deltaTime * speedG;
                timeB += Time.deltaTime * speedB;
                timeA += Time.deltaTime * speedA;

                var r = Mathf.SmoothStep(minR, maxR, timeR);
                var g = Mathf.SmoothStep(minG, maxG, timeG);
                var b = Mathf.SmoothStep(minB, maxB, timeB);
                var a = Mathf.SmoothStep(minA, maxA, timeA);
                spriteRenderer.color = new Color(r, g, b, a);

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
