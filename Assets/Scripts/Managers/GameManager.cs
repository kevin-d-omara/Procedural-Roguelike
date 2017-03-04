using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    public class GameManager : MonoBehaviour
    {
        public enum Timing { Start, Middle, End }
        public delegate void PassageTransition(Timing timing, Vector2 position);
        public static event PassageTransition OnPassageTransition;

        public delegate void IncreaseDifficulty(int difficulty);
        public static event IncreaseDifficulty OnIncreaseDifficulty;

        /// <summary>
        /// Multiplier applied each time the difficulty increases.
        /// Cumulative (i.e. Lvl 2 => X * mult * mult);
        /// </summary>
        [Range(0f, 100f)]
        public float DifficultyMultiplier = 0.35f;

        public GameObject Player { get; private set; }

        // Singleton instance.
        private static GameManager _instance = null;
        public static GameManager Instance
        {
            get { return _instance; }
            private set { _instance = value; }
        }

        private int _difficulty = 0;
        public int Difficulty
        {
            get { return _difficulty; }
            private set
            {
                _difficulty = value;
                if (OnIncreaseDifficulty != null) { OnIncreaseDifficulty(Difficulty); }
            }
        }

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject overWorldPrefab;
        [SerializeField] private GameObject cavePrefab;
        [SerializeField] private Vector2 startSize = new Vector2(5, 5);
        private OverWorldManager overWorld;
        private CaveManager currentCave;
        private GameObject mainCamera;
        private CameraController mainCameraController;

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

        private void OnEnable()
        {
            PassageController.OnEnterCave += OnEnterCave;
            PassageController.OnExitCave  += OnExitCave;
        }

        private void OnDisable()
        {
            PassageController.OnEnterCave -= OnEnterCave;
            PassageController.OnExitCave  -= OnExitCave;
        }

        /// <summary>
        /// Creates a new OverWorld and places the Player at the center of it.
        /// </summary>
        private void InitializeGame()
        {
            Difficulty = 0;

            // Create OverWorld.
            GameObject instance = Instantiate(overWorldPrefab, new Vector3(0, 0, 0),
                Quaternion.identity);
            overWorld = instance.GetComponent<OverWorldManager>();
            overWorld.SetupEntrance(startSize, new Vector2(0, 0));

            // Create Player and wire it up to the Camera.
            Player = Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            mainCamera = GameObject.FindGameObjectsWithTag("MainCamera")[0];
            mainCamera.GetComponent<CameraFollow>().target = Player.transform;
            mainCameraController = mainCamera.GetComponent<CameraController>();
        }

        private void OnEnterCave(Vector2 caveEntrancePosition)
        {
            StartCoroutine(CaveTransition(caveEntrancePosition, true));
        }

        private void OnExitCave(Vector2 caveExitPosition)
        {
            StartCoroutine(CaveTransition(caveExitPosition, false));
        }

        /// <summary>
        /// Fade camera out to black. Transition to new world (OverWorld <=> Cave). Fade back in.
        /// </summary>
        private IEnumerator CaveTransition(Vector2 passagePosition, bool entering)
        {
            if (OnPassageTransition != null) { OnPassageTransition(Timing.Start, passagePosition); }
            mainCameraController.FadeOut();
            yield return new WaitForSeconds(mainCameraController.FadeTime);

            // Transition between worlds.
            if (entering)
            {
                // Destroy Cave entrance (in OverWorld).
                // TODO: ^ or transition to "collapsed" graphic

                // Deactivate OverWorld.
                overWorld.gameObject.SetActive(false);

                // Create new cave.
                currentCave = (Instantiate(cavePrefab, passagePosition,
                    Quaternion.identity) as GameObject).GetComponent<CaveManager>();
                currentCave.SetupEntrance(startSize, passagePosition);
            }
            else
            {
                // Destroy Cave.
                Destroy(currentCave.gameObject);

                // Reactivate Overworld.
                overWorld.gameObject.SetActive(true);

                // Increase difficulty.
                ++Difficulty;

                // Clear entrance tile on OverWorld (i.e. make sure no Rocks blocking the path up).
                overWorld.SetupEntrance(Vector2.one, passagePosition);
            }

            mainCameraController.FadeIn();
            if (OnPassageTransition != null) { OnPassageTransition(Timing.Middle, passagePosition); }
            yield return new WaitForSeconds(mainCameraController.FadeTime);
            if (OnPassageTransition != null) { OnPassageTransition(Timing.End, passagePosition); }
        }
    }
}
