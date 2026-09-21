using UnityEngine;
using UnityEngine.EventSystems;

public class LuckHat : MonoBehaviour, IPointerClickHandler
{
    private LuckMinigame owner;
    private int hatIndex;

    public void Configure(
        LuckMinigame minigame,
        int index)
    {
        owner = minigame;
        hatIndex = index;
    }

    public void OnPointerClick(
        PointerEventData eventData)
    {
        if (owner == null)
            return;

        owner.TryPickHat(hatIndex);
    }
}