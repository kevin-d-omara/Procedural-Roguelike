using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DynamiteController : MonoBehaviour
    {
        [Range(0f, 3f)] [SerializeField] private float fuseTime = 1f;
        [Range(1, 6)]   [SerializeField] private int blastRadius = 2;
        [Range(1, 10)]  [SerializeField] private int damage;

        // Components
        BoxCollider2D boxCollider;

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }

        public void LightFuse()
        {
            StartCoroutine(CountDownToExplosion(fuseTime));
        }

        private IEnumerator CountDownToExplosion(float fuseTime)
        {
            var timer = fuseTime;
            
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            // Explode!!
            var offsets = GridAlgorithms.GetCircularOffsets(blastRadius);
            var location = BoardManager.Constrain(transform.position);
            var positions = GridAlgorithms.GetPositionsFrom(offsets, location);

            boxCollider.enabled = false;
            foreach (Vector2 position in positions)
            {
                var healthComponents = Utility.FindComponentsAt<Health>(position);
                foreach (Health healthComponent in healthComponents)
                {
                    healthComponent.TakeDamage(damage, true);
                }
            }
            boxCollider.enabled = true;

            // TODO: add flash of light
            // TODO: add (directional) screen shake
            // TODO: add 2 states: lit and un-lit (pick-up-able)

            Destroy(gameObject);
        }
    }
}
