using DG.Tweening;
using TMPro;
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
    [SerializeField] private TMP_Text _statusText;

    private Sequence _statusSequence;

    public ScrollRect CubeScroll => _cubeScroll;
    public RectTransform CubeContainer => _cubeContainer;
    public RectTransform TowerArea => _towerArea;
    public RectTransform Hole => _hole;
    public RectTransform HoleMask => _holeMask;
    public RectTransform DragLayer => _dragLayer;
    public RectTransform PoolRoot => _poolRoot;

    private void Awake() => _statusText.text = string.Empty;
    
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

    public void SetStatus(GameText value)
    {
        _statusSequence?.Kill();

        _statusText.text = value.Text;
        _statusText.color = value.Color;
        _statusText.alpha = 1f;
        _statusText.rectTransform.localScale = Vector3.zero;

        _statusSequence = DOTween.Sequence()
            .SetTarget(_statusText)
            .Append(_statusText.rectTransform.DOScale(1.2f, 0.16f).SetEase(Ease.OutCubic))
            .Append(_statusText.rectTransform.DOScale(1f, 0.1f).SetEase(Ease.InOutQuad))
            .AppendInterval(1f)
            .Append(DOTween.To(() => _statusText.alpha, alpha => _statusText.alpha = alpha, 0f, 0.3f));
    }

    private void OnDestroy() => _statusSequence?.Kill();
}
