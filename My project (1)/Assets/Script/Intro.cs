using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasSequence : MonoBehaviour
{
    public RectTransform imageToEnlarge;
    public Vector3 enlargedScale = new Vector3(1.5f, 1.5f, 1.5f);

    public float waitBeforeEnlarge = 2f;
    public float enlargeDuration = 2f;      // how long the growth animation takes
    public float waitBeforeSceneChange = 2f;
    public string nextSceneName;

    void Start()
    {
        StartCoroutine(SequenceRoutine());
    }

    IEnumerator SequenceRoutine()
    {
        yield return new WaitForSeconds(waitBeforeEnlarge);

        yield return StartCoroutine(EnlargeOverTime(enlargeDuration));

        yield return new WaitForSeconds(waitBeforeSceneChange);

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator EnlargeOverTime(float duration)
    {
        Vector3 startScale = imageToEnlarge.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            imageToEnlarge.localScale = Vector3.Lerp(startScale, enlargedScale, t);
            yield return null;
        }

        imageToEnlarge.localScale = enlargedScale; // snap to exact final value
    }
}