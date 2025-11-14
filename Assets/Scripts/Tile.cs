using UnityEngine;

public enum TileColor {
    None,
    Blue,
    Red,
    Yellow,
    Green,
    Black
}

public class Tile : MonoBehaviour
{
    public bool isLocked = false;
    public TileColor color;

    public SpriteRenderer fillRenderer;
    public GridManager gridManager;

    public void SetColor(Color newVisualColor)
    {
        if (fillRenderer != null)
        {
            fillRenderer.color = newVisualColor;
        }
    }

    void OnMouseDown()
    {
        if (isLocked) return;
        gridManager.OnTileClicked(this);
    }
}