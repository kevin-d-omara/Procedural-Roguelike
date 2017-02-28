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
        private OverWorldManager overWorld;
        private GameObject player;
        [SerializeField] private GameObject mainCamera;

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
            // Create OverWorld.
            GameObject instance = Instantiate(overWorldPrefab, new Vector3(0, 0, 0),
                Quaternion.identity);
            overWorld = instance.GetComponent<OverWorldManager>();
            overWorld.SetupBoard(startSize, new Vector2(0, 0));

            // Create Player and wire it up to the Camera.
            player = Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            mainCamera = GameObject.FindGameObjectsWithTag("MainCamera")[0];
            mainCamera.GetComponent<CameraFollow>().target = player.transform;
        }
    }
}
