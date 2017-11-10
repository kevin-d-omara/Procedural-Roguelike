using UnityEngine;

namespace TabletopCardCompanion
{
    public class ExitGame : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }
}
