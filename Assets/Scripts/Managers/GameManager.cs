using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    public class GameManager : MonoBehaviour
    {
        // Singleton instance.
        private static GameManager _instance = null;
        public static GameManager Instance
        {
            get { return _instance; }
            private set { _instance = value; }
        }

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject overWorldPrefab;
        [SerializeField] private Vector2 startSize = new Vector2(5, 5);
        private OverWorld overWorld;

        private void Awake()
        {
            // Enforce singleton pattern.
            if (Instance == null)
            {
                Instance = this;
                InitializeGame();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Creates a new OverWorld and places the Player at the center of it.
        /// </summary>
        private void InitializeGame()
        {
            GameObject instance = Instantiate(overWorldPrefab, new Vector3(0, 0, 0),
                Quaternion.identity);
            overWorld = instance.GetComponent<OverWorld>();
            overWorld.SetupOverWorld(startSize, new Vector2(0, 0));

            Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }
}
