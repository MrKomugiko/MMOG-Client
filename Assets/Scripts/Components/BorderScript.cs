using UnityEngine;

public class BorderScript : MonoBehaviour
{
    Transform _transform;    
    Vector3Int borderPosition;
    SpriteRenderer SRenderer;

    public Vector3Int BorderPosition 
    {
        get => borderPosition; 
        set 
        {
            borderPosition = value; 
        }
    }

    private void Awake() {
        SRenderer= GetComponent<SpriteRenderer>();
        _transform = GetComponent<Transform>();    
    }
    private void Start() 
    {
        ShowBorder();
    }
    public void ShowBorder()
    {
        SRenderer.enabled = true;
    }
    public void HideBorder()
    {
        SRenderer.enabled = false;
    }
    
    public void ChangeBorderSortingOrder(int currentPlayerFloor)
    {
        SRenderer.sortingOrder = 1+ 2*(currentPlayerFloor); 
    }
}
