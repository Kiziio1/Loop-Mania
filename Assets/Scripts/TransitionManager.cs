using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    public float fadeDuration = 0.5f; 
    private CanvasGroup fadeCanvasGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            fadeCanvasGroup = GetComponent<CanvasGroup>();
            if (fadeCanvasGroup == null)
            {
                Debug.LogError("¡TransitionManager necesita un componente CanvasGroup!");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        yield return StartCoroutine(Fade(1f)); 

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
        {
            yield return null;
        }
        
        yield return StartCoroutine(Fade(0f)); 
    }

    private IEnumerator Fade(float targetAlpha)
    {
        fadeCanvasGroup.blocksRaycasts = true;

        float time = 0;
        float startAlpha = fadeCanvasGroup.alpha;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;

        if (targetAlpha == 0f)
        {
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }
    
    public void QuitGame()
    {
        StartCoroutine(FadeOutAndQuit());
    }

    private IEnumerator FadeOutAndQuit()
    {
        yield return StartCoroutine(Fade(1f)); 
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}