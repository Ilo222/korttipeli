using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ButtonMashing : MonoBehaviour
{
    public float mashDelay = 0.2f;
    public float surviveTime = 10f;
    public float difficultyMultiplier = 1.5f;
    public TMP_Text text; // single TMP text object used for everything

    private float mash;
    private float elapsed;
    private bool started;
    private bool ended;

    void Start()
    {
        mash = mashDelay;
        text.gameObject.SetActive(true);
        text.text = "Press Space to start mashing!";
    }

    void Update()
    {
        bool jumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (!started && jumpPressed)
        {
            started = true;
            text.text = "Go!";
        }

        if (started && !ended)
        {
            mash -= Time.deltaTime;
            elapsed += Time.deltaTime;

            if (jumpPressed)
            {
                mash = mashDelay;
            }

            if (mash <= 0f)
            {
                ended = true;
                text.text = "You failed!";
            }
            else if (elapsed >= surviveTime)
            {
                ended = true;
                text.text = "You win!";
                surviveTime *= difficultyMultiplier;
            }
        }
    }

    public void ResetRound()
    {
        mash = mashDelay;
        elapsed = 0f;
        started = false;
        ended = false;
        text.text = "Press Space to start mashing!";
    }
}