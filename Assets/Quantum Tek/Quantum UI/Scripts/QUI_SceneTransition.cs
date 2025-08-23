using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuantumTek.QuantumUI
{
    /// <summary>
    /// QUI_LoadType determines how a scene should be loaded.
    /// </summary>
    [System.Serializable]
    public enum QUI_LoadType
    {
        /// <summary> The scene will load instantly, with transition effects. </summary>
        Instant,
        /// <summary> The scene will load asynchronously (in the background), while showing some loading UI after the start of the scene transition effect. </summary>
        LoadingUI,
        /// <summary> The scene will load asynchronously (in the background), while showing a loading scene. </summary>
        LoadingScene
    }

    /// <summary>
    /// QUI_SceneTransition handles smooth transition effects between scenes, along with loading screen effects.
    /// </summary>
    [AddComponentMenu("Quantum Tek/Quantum UI/Scene Transition")]
    [DisallowMultipleComponent]
    public class QUI_SceneTransition : MonoBehaviour
    {
        [Tooltip("The duration of each loading phase, in seconds. The first phase is the longest, and the last phase is the shortest.")]
        [SerializeField] float[] loadPhasesDuration = { 0.7f, 0.5f, 0.4f};
        [Header("Scene Transition Object References")]
        [Tooltip("The animator used in running the scene transition.")]
        public QUI_ElementAnimator animator;
        [Tooltip("The loading bar to update, if there is one.")]
        public QUI_Bar loadingBar;
        [Tooltip("The UI element to show when loading, if there is one.")]
        public RectTransform loadingUI;

        [Header("Scene Transition Variables")]
        [Tooltip("How to load the scene.")]
        public QUI_LoadType loadType;
        public static string sceneToLoad = "";
        [Tooltip("The name of the loading scene, if the type is LoadingScene.")]
        public string loadingSceneName = "";
        [Tooltip("The name of the transition animation to play when entering a scene.")]
        public string enterSceneAnimation;
        [Tooltip("The name of the transition animation to play when exiting a scene.")]
        public string exitSceneAnimation;

        private void Awake()
        {
            if (sceneToLoad.Length > 0)
            {
                if (SceneManager.GetActiveScene().name == sceneToLoad)
                {
                    sceneToLoad = "";

                    if (animator)
                        animator.PlayAnimation(enterSceneAnimation);
                }

                if (loadType == QUI_LoadType.LoadingScene && SceneManager.GetActiveScene().name == loadingSceneName)
                    StartLoad();
            }
        }

        /// <summary>
        /// Starts to load the necessary scene, used at the end of scene transition animations.
        /// </summary>
        public void StartLoad()
        {
            if (sceneToLoad.Length == 0)
                return;
            if (loadType == QUI_LoadType.LoadingUI || (loadType == QUI_LoadType.LoadingScene && SceneManager.GetActiveScene().name == loadingSceneName))
                StartCoroutine(LoadSceneAsync(sceneToLoad));
            else if (loadType == QUI_LoadType.LoadingScene && SceneManager.GetActiveScene().name != sceneToLoad)
                SceneManager.LoadScene(loadingSceneName);
        }

        /// <summary>
        /// Starts loading a scene, with behaviour based on the loading type.
        /// </summary>
        /// <param name="sceneName"></param>
        public void LoadScene(string sceneName)
        {
            Time.timeScale = 1f; // Ensure time scale is normal

            if (loadType == QUI_LoadType.Instant)
                SceneManager.LoadScene(sceneName);
            else if (loadType == QUI_LoadType.LoadingUI)
            {
                sceneToLoad = sceneName;
                if (loadingUI)
                    loadingUI.gameObject.SetActive(true);

                if (animator)
                    animator.PlayAnimation(exitSceneAnimation);

            }
            else if (loadType == QUI_LoadType.LoadingScene)
            {
                sceneToLoad = sceneName;

                if (animator)
                    animator.PlayAnimation(exitSceneAnimation);
            }
        }

        protected IEnumerator LoadSceneAsync(string sceneName)
        {
            Debug.Log("Starting LoadSceneAsync coroutine.");

            yield return new WaitForSecondsRealtime(0.5f);

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
            loadOperation.allowSceneActivation = false;

            float loadProgress = 0f;

            // Fase 1: in 0.5 secondi
            float phase1Duration = loadPhasesDuration[0];
            float elapsed = 0f;
            float range = Random.Range(0f, 0.5f);

            Debug.Log($"Entering Phase 1 loop. Elapsed: {elapsed}, Target Duration: {phase1Duration}");

            while (elapsed < phase1Duration)
            {
                elapsed += Time.unscaledDeltaTime;
                loadProgress = Mathf.Lerp(0f, range, elapsed / phase1Duration);
                if (loadingBar) loadingBar.SetFill(loadProgress);
                yield return null;
            }
            Debug.Log($"Finished Phase 1. Final Elapsed: {elapsed}");


            yield return new WaitForSecondsRealtime(0.2f);


            // Fase 2: in 0.6 secondi
            float phase2Duration = loadPhasesDuration[1];
            elapsed = 0f;

            float start = loadProgress;
            float target = Random.Range(start + 0.1f, start + (1f - start - 0.2f)); // lascia margine per fase 3

            while (elapsed < phase2Duration)
            {
                elapsed += Time.unscaledDeltaTime;
                loadProgress = Mathf.Lerp(start, target, elapsed / phase2Duration);
                if (loadingBar) loadingBar.SetFill(loadProgress);
                yield return null;
            }


            yield return new WaitForSecondsRealtime(0.5f);

            // Fase 3: 0.2 secondi
            float phase3Duration = loadPhasesDuration[2];
            elapsed = 0f;

            start = loadProgress;
            target = 1; // punta verso il 100%

            while (elapsed < phase3Duration)
            {
                elapsed += Time.unscaledDeltaTime;
                loadProgress = Mathf.Lerp(start, target, elapsed / phase3Duration);
                if (loadingBar) loadingBar.SetFill(loadProgress);
                yield return null;
            }


            // Attivazione della scena
            loadOperation.allowSceneActivation = true;
        }


        public float Remap(float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            float fromAbs = from - fromMin;
            float fromMaxAbs = fromMax - fromMin;

            float normal = fromAbs / fromMaxAbs;

            float toMaxAbs = toMax - toMin;
            float toAbs = toMaxAbs * normal;

            float to = toAbs + toMin;

            return to;
        }
    }

}