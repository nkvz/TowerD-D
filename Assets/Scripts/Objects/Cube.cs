using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class Cube : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private DragDropService _dragDrop;
    private Image _image;
    private RectTransform _rectTransform;

    public CubeConfig Config { get; private set; }
    public bool IsPaletteCube { get; private set; }
    public RectTransform RectTransform => _rectTransform;

    [Inject]
    private void Construct(DragDropService service)
    {
        _dragDrop = service;
    }

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
        _image = GetComponent<Image>();
    }

    public void Configure(CubeConfig config, bool isPaletteCube)
    {
        Config = config;
        IsPaletteCube = isPaletteCube;
        _image.sprite = config.Sprite;
        _image.color = config.Sprite != null ? Color.white : Color.magenta;
        _image.raycastTarget = true;
        _rectTransform.localScale = Vector3.one;
        _rectTransform.localRotation = Quaternion.identity;
        gameObject.name = $"Cube {config.Type}";
    }

    public void ResetForPool()
    {
        _rectTransform.DOKill();
        _image.DOKill();
        _rectTransform.localScale = Vector3.one;
        _rectTransform.localRotation = Quaternion.identity;
        _image.color = Color.white;
        _image.raycastTarget = true;
        Config = null;
        IsPaletteCube = false;
    }

    public void SetRaycastTarget(bool value) => _image.raycastTarget = value;

    public void OnPointerDown(PointerEventData eventData) => _dragDrop.PointerDown(this, eventData);
    public void OnPointerUp(PointerEventData eventData) => _dragDrop.PointerUp(this, eventData);
    public void OnInitializePotentialDrag(PointerEventData eventData) => _dragDrop.InitializePotentialDrag(this, eventData);
    public void OnBeginDrag(PointerEventData eventData) => _dragDrop.BeginDrag(this, eventData);
    public void OnDrag(PointerEventData eventData) => _dragDrop.Drag(this, eventData);
    public void OnEndDrag(PointerEventData eventData) => _dragDrop.EndDrag(this, eventData);
}
