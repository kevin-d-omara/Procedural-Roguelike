using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    /// <summary>
    /// Allows the attached GameObject's sprite to be affected by lighting.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Illuminateable : MonoBehaviour
    {
        public delegate void ObjectCreated(Illuminateable illuminateableComponent);
        public static event ObjectCreated OnObjectCreated;

        public enum Type { Terrain, Entity }

        /// <summary>
        /// Type of illuminateable object: Terrain, Entity, etc.
        /// </summary>
        public Type ObjectType
        {
            get { return _objectType; }
        }
        [SerializeField] private Type _objectType = Type.Terrain;

        /// <summary>
        /// Current illumination level of this object: Full, Half, Low, None, etc.
        /// </summary>
        public Illumination Brightness
        {
            get { return _brightness; }
            set
            {
                _brightness = value;
                UpdateSprite();
            }
        }
        private Illumination _brightness = Illumination.Full;

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
            if (OnObjectCreated != null) { OnObjectCreated(this); }
        }

        /// <summary>
        /// Updates the alpha value of the attached sprite.
        /// </summary>
        public void UpdateSprite()
        {
            var color = spriteRenderer.color;
            float intensity;
            float rate;

            switch (Brightness)
            {
                case Illumination.Full:
                    color = Color.white;
                    rate = 3.0f;
                    break;

                case Illumination.Half:
                    intensity = 0.5f;
                    rate = 3.0f;
                    color = new Color(intensity, intensity, intensity, 1f);
                    break;

                case Illumination.Low:
                    intensity = 0.3f;
                    rate = 2.0f;
                    color = new Color(intensity, intensity, intensity, 1f);
                    break;

                case Illumination.None:
                    color = spriteRenderer.color;
                    color.a = 0f;
                    rate = 1.0f;
                    break;

                default:
                    throw new System.ArgumentException("Unsupported illumination.");
            }

            switch (ObjectType)
            {
                case Type.Terrain:
                    spriteRenderer.color = color;
                    break;

                case Type.Entity:
                    StartCoroutine(LerpColor(color, rate, Brightness));
                    break;

                default:
                    throw new System.ArgumentException("Unsupported illuminateable object type.");
            }
        }

        /// <summary>
        /// Changes the current color towards the target color at the specified rate.
        /// </summary>
        /// <param name="speed">Time to go from 1 to 0. (i.e. rate=2.0 -> 0.5 second lerp).</param>
        private IEnumerator LerpColor(Color target, float speed, Illumination targetIllumination)
        {
            var start = spriteRenderer.color;

            bool isMakingBrighter = true;
            if (target.a < start.a || target.r < start.r)
            {
                isMakingBrighter = false;
                speed *= -1f;
            }

            // Calculate bounds for each color channel.
            var minR = isMakingBrighter ? start.r : target.r;
            var maxR = !isMakingBrighter ? start.r : target.r;
            var minG = isMakingBrighter ? start.g : target.g;
            var maxG = !isMakingBrighter ? start.g : target.g;
            var minB = isMakingBrighter ? start.b : target.b;
            var maxB = !isMakingBrighter ? start.b : target.b;
            var minA = isMakingBrighter ? start.a : target.a;
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
                if (Brightness != targetIllumination || current == target) { break; }

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
