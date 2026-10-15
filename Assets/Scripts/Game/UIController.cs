using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Cube strip")]
    [SerializeField] private ScrollRect _cubeScroll;
    [SerializeField] private RectTransform _cubeContainer;

    [Header("Play area")]
    [SerializeField] private RectTransform _towerArea;
    [SerializeField] private RectTransform _hole;
    [SerializeField] private RectTransform _holeMask;
    [SerializeField] private RectTransform _dragLayer;
    [SerializeField] private RectTransform _poolRoot;

    [Header("Feedback")]
    [SerializeField] private Text _statusText;

    public ScrollRect CubeScroll => _cubeScroll;
    public RectTransform CubeContainer => _cubeContainer;
    public RectTransform TowerArea => _towerArea;
    public RectTransform Hole => _hole;
    public RectTransform HoleMask => _holeMask;
    public RectTransform DragLayer => _dragLayer;
    public RectTransform PoolRoot => _poolRoot;
    
    public void ClearExistingCubes()
    {
        for (int i = _cubeContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _cubeContainer.GetChild(i);
            if (!child.TryGetComponent(out Cube _))
                continue;

            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    public void SetStatus(string value)
    {
        _statusText.text = value;
    }
}
