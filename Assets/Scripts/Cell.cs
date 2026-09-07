using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerClickHandler
{
    public Minesweeper board;
    public int x;
    public int y;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) board.Reveal(x, y); //LMB click
        else if (eventData.button == PointerEventData.InputButton.Right) board.Flag(x, y); //RMB click
    }
}
