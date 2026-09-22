using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinCon : MonoBehaviour
{
    [Header("WIN BANNER")]
    [SerializeField] private GameObject winBanner;
    [SerializeField] private RectTransform imageToEnlarge;
    [SerializeField]
    private Vector3 enlargedScale =
        new Vector3(1.5f, 1.5f, 1.5f);

    [Header("WIN TIMING")]
    [SerializeField] private float waitBeforeEnlarge = 0.5f;
    [SerializeField] private float enlargeDuration = 0.8f;

    [Tooltip("How long to wait after the banner finishes enlarging before loading the cutscene.")]
    [SerializeField] private float waitBeforeSceneChange = 3f;

    [Header("CUTSCENE")]
    [SerializeField] private string nextSceneName;

    private bool winTriggered;

    private void Start()
    {
        // Make sure the win banner is hidden when the game begins.
        if (winBanner != null)
            winBanner.SetActive(false);
    }

    private void Update()
    {
        if (winTriggered)
            return;

        if (GameManager.Instance == null)
            return;

        // Player has no cards left = WIN.
        if (GameManager.Instance.playerHand.Count <= 0)
        {
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        if (winTriggered)
            return;

        winTriggered = true;

        Debug.Log("PLAYER WON THE GAME!");

        // Stop normal gameplay.
        GameManager.Instance.state = GameState.GameOver;

        StartCoroutine(
            WinSequenceRoutine()
        );
    }

    private IEnumerator WinSequenceRoutine()
    {
        // Show WIN banner.
        if (winBanner != null)
        {
            winBanner.SetActive(true);
        }

        // Wait before growing it.
        yield return new WaitForSeconds(
            waitBeforeEnlarge
        );

        // Enlarge the banner.
        if (imageToEnlarge != null)
        {
            yield return StartCoroutine(
                EnlargeOverTime(
                    enlargeDuration
                )
            );
        }

        // Leave the WIN banner on screen.
        yield return new WaitForSeconds(
            waitBeforeSceneChange
        );

        // Load cutscene.
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(
                nextSceneName
            );
        }
        else
        {
            Debug.LogWarning(
                "CanvasSequence: Next Scene Name is empty. " +
                "The WIN sequence finished, but no cutscene scene was loaded."
            );
        }
    }

    private IEnumerator EnlargeOverTime(
        float duration)
    {
        if (imageToEnlarge == null)
            yield break;

        Vector3 startScale =
            imageToEnlarge.localScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );

            imageToEnlarge.localScale =
                Vector3.Lerp(
                    startScale,
                    enlargedScale,
                    t
                );

            yield return null;
        }

        imageToEnlarge.localScale =
            enlargedScale;
    }
}