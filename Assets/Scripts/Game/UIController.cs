using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public ScrollRect CubeScroll => _cubeScroll;
    public RectTransform CubeContainer => _cubeContainer;
    public RectTransform TowerArea => _towerArea;
    public RectTransform Hole => _hole;
    public RectTransform HoleMask => _holeMask;
    public RectTransform DragLayer => _dragLayer;
    public RectTransform PoolRoot => _poolRoot;
    


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

    [Header("Controll")]
    [SerializeField] private Button _reloadButton;
    [SerializeField] private Button _clearButton;
    [SerializeField] private Image Transition;
    
    
    private Sequence _statusSequence;
    private Tween _transitionTween;
    private Action _reloadAction;
    private Action _clearAction;
    private bool _transitionInProgress;
    
    private void Awake()
    {
        _statusText.text = string.Empty;
        Transition.transform.SetAsLastSibling();
        SetTransitionAlpha(1f);
        Transition.raycastTarget = true;
        _transitionInProgress = true;
    }

    private void Start()
    {
        OpenTransition();
    }

    public void InitializeControls(Action reloadAction, Action clearAction)
    {
        _reloadAction = reloadAction;
        _clearAction = clearAction;
        _reloadButton.onClick.AddListener(OnReloadClicked);
        _clearButton.onClick.AddListener(OnClearClicked);
    }

    public void OpenTransition()
    {
        _transitionTween = Transition.DOFade(0f, 2).SetUpdate(true).OnComplete(() =>
        {
            Transition.raycastTarget = false;
            _transitionInProgress = false;
        });
    }
    
    public void CloseTransition(Action completed)
    {
        if (_transitionInProgress)
            return;

        _transitionInProgress = true;
        Transition.raycastTarget = true;
        _transitionTween?.Kill();
        _transitionTween = Transition.DOFade(1f, 1).SetUpdate(true).OnComplete(() => completed?.Invoke());
    }

    private void OnReloadClicked()
    {
        if (!_transitionInProgress)
            _reloadAction?.Invoke();
    }

    private void OnClearClicked()
    {
        if (!_transitionInProgress)
            _clearAction?.Invoke();
    }

    private void SetTransitionAlpha(float alpha)
    {
        Color color = Transition.color;
        color.a = alpha;
        Transition.color = color;
    }
    
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

    private void OnDestroy()
    {
        _reloadButton.onClick.RemoveListener(OnReloadClicked);
        _clearButton.onClick.RemoveListener(OnClearClicked);
        _statusSequence?.Kill();
        _transitionTween?.Kill();
    }
}
