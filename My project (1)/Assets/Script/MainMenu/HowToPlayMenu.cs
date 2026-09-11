using UnityEngine;
using UnityEngine.UI;

public class HowToPlayMenu : MonoBehaviour
{
    public GameObject howToPlayPanel; // Vedä tähän koko Scroll View -paneeli
    public Button howToPlayButton;    // Vedä tähän "How to Play" -nappi

    private bool isOpen = false;

    void Start()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false); // Piilotetaan säännöt alussa
        }

        if (howToPlayButton != null)
        {
            howToPlayButton.onClick.AddListener(TogglePanel);
        }
    }

    void TogglePanel()
    {
        if (howToPlayPanel != null)
        {
            isOpen = !isOpen;
            howToPlayPanel.SetActive(isOpen);
        }
    }
}