using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{
    public class GameManager : MonoBehaviour
    {
        public delegate void PassageTransition(bool startOfTransition);
        public static event PassageTransition OnPassageTransition;

        // Singleton instance.
        private static GameManager _instance = null;
        public static GameManager Instance
        {
            get { return _instance; }
            private set { _instance = value; }
        }

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject overWorldPrefab;
        [SerializeField] private GameObject dungeonPrefab;
        [SerializeField] private Vector2 startSize = new Vector2(5, 5);
        private OverWorldManager overWorld;
        private DungeonManager currentDungeon;
        private GameObject player;
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
            PassageController.OnEnterDungeon += OnEnterDungeon;
            PassageController.OnExitDungeon += OnExitDungeon;
        }

        private void OnDisable()
        {
            PassageController.OnEnterDungeon -= OnEnterDungeon;
            PassageController.OnExitDungeon -= OnExitDungeon;
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
            mainCameraController = mainCamera.GetComponent<CameraController>();
        }

        private void OnEnterDungeon(Vector2 dungeonEntrancePosition)
        {
            StartCoroutine(EnterDungeonSequence(dungeonEntrancePosition));
        }

        /// <summary>
        /// Fade camera out to black. Destroy Dungeon entrance. De-activate OverWorld. Create new
        /// Dungeon. Fade back in.
        /// </summary>
        private IEnumerator EnterDungeonSequence(Vector2 dungeonEntrancePosition)
        {
            // Broadcast start of sequence.
            if (OnPassageTransition != null) { OnPassageTransition(true); }
            mainCameraController.FadeOut();

            yield return new WaitForSeconds(mainCameraController.FadeTime);
            {

                // Destroy Dungeon entrance (in OverWorld).


                // Deactivate OverWorld.
                overWorld.gameObject.SetActive(false);

                // Create new dungeon.
                currentDungeon = (Instantiate(dungeonPrefab, dungeonEntrancePosition,
                    Quaternion.identity) as GameObject).GetComponent<DungeonManager>();
                currentDungeon.SetupBoard(startSize, dungeonEntrancePosition);
            }
            mainCameraController.FadeIn();
            yield return new WaitForSeconds(mainCameraController.FadeTime);
            if (OnPassageTransition != null) { OnPassageTransition(false); }
        }

        /// <summary>
        /// Destroy the dungeon and re-activate the OverWorld. Create a new "starting location" at
        /// the exit position.
        /// </summary>
        private void OnExitDungeon(Vector2 dungeonExitPosition)
        {
            Destroy(currentDungeon.gameObject);
            overWorld.gameObject.SetActive(true);
            overWorld.SetupBoard(startSize, dungeonExitPosition);
        }
    }
}
