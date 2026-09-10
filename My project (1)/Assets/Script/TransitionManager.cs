using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Cinemachine; // Unity 6 / CM3 Namespace

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    public CinemachineCamera vcamFirstPerson; // CM3 component name
    public RectTransform minigamePanel;
    public TextMeshProUGUI countdownText;

    private void Awake() => Instance = this;

    public void StartChallenge(string instruction) => StartCoroutine(Sequence(instruction));

    private IEnumerator Sequence(string instruction)
    {
        // Swoop camera to First Person
        vcamFirstPerson.Priority = 15;
        yield return new WaitForSeconds(1.2f);

        // Slide WarioWare UI in
        float t = 0;
        while (t < 0.3f)
        {
            minigamePanel.anchoredPosition = Vector2.Lerp(new Vector2(-2500, 0), Vector2.zero, t / 0.3f);
            t += Time.deltaTime;
            yield return null;
        }
        minigamePanel.anchoredPosition = Vector2.zero;

        // Run Countdown
        string[] sequence = { instruction, "3", "2", "1", "GO!" };
        foreach (string text in sequence)
        {
            countdownText.text = text;
            yield return new WaitForSeconds(0.6f);
        }
        countdownText.text = "";

        Debug.Log("Minigame Starts Now!");
    }

    public void EndChallenge()
    {
        vcamFirstPerson.Priority = 5; // Swoop back to ceiling
        minigamePanel.anchoredPosition = new Vector2(-2500, 0);
    }
}