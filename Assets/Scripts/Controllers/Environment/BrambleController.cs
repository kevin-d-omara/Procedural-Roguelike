using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(TwoSidedTile))]
    [RequireComponent(typeof(Health))]
	public class BrambleController : MonoBehaviour
	{
        // Components
        private Health healthComponent;
        private TwoSidedTile twoSidedTileComponent;
        private Sprite sprite;

        private void Awake()
        {
            // Get references to all components

            healthComponent = GetComponent<Health>();
            twoSidedTileComponent = GetComponent<TwoSidedTile>();
        }

        private void OnEnable()
        {
            healthComponent.OnLostHitPoints += OnTakeDamage;
            healthComponent.OnKilled += OnBrambleDestroyed;
        }

        private void OnDisable()
        {
            healthComponent.OnLostHitPoints -= OnTakeDamage;
            healthComponent.OnKilled -= OnBrambleDestroyed;
        }

        private void OnTakeDamage(int damage)
        {
            if (healthComponent.HitPoints <= healthComponent.MaxHitPoints / 2)
            {
                // Set tile to damaged sprite.
                twoSidedTileComponent.SetSpriteToBack();
            }
        }

        private void OnBrambleDestroyed()
        {
            Destroy(gameObject);
        }
    }
}
