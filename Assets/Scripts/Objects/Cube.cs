using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class Cube : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");

    private DragDropService _dragDrop;
    private Image _image;
    private RectTransform _rectTransform;
    private Material _baseMaterial;
    private Material _dissolveMaterial;
    private float _dissolveValue;

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
        _baseMaterial = _image.material;
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
        ResetDissolve();
        gameObject.name = $"Cube {config.Type}";
    }

    public Tween DODissolve(float duration)
    {
        if (_dissolveMaterial == null)
        {
            if (_baseMaterial == null || !_baseMaterial.HasProperty(DissolveAmount))
                return DOVirtual.Float(0f, 1f, duration, _ => { });

            _dissolveMaterial = new Material(_baseMaterial);
        }

        _image.material = _dissolveMaterial;
        SetDissolve(0f);
        return DOTween.To(() => _dissolveValue, SetDissolve, 1f, duration);
    }

    private void SetDissolve(float value)
    {
        _dissolveValue = value;
        _dissolveMaterial.SetFloat(DissolveAmount, value);

        // A UI Mask renders with a stencil copy of the material.
        Material renderingMaterial = _image.materialForRendering;
        if (renderingMaterial != _dissolveMaterial && renderingMaterial.HasProperty(DissolveAmount))
            renderingMaterial.SetFloat(DissolveAmount, value);
    }

    private void ResetDissolve()
    {
        _dissolveValue = 0f;
        if (_dissolveMaterial == null)
            return;

        _dissolveMaterial.SetFloat(DissolveAmount, 0f);
        _image.material = _baseMaterial;
    }

    public void ResetForPool()
    {
        _rectTransform.DOKill();
        _image.DOKill();
        _rectTransform.localScale = Vector3.one;
        _rectTransform.localRotation = Quaternion.identity;
        _image.color = Color.white;
        _image.raycastTarget = true;
        ResetDissolve();
        Config = null;
        IsPaletteCube = false;
    }

    private void OnDestroy()
    {
        if (_dissolveMaterial != null)
            Destroy(_dissolveMaterial);
    }

    public void SetRaycastTarget(bool value) => _image.raycastTarget = value;

    public void OnPointerDown(PointerEventData eventData) => _dragDrop.PointerDown(this, eventData);
    public void OnPointerUp(PointerEventData eventData) => _dragDrop.PointerUp(this, eventData);
    public void OnInitializePotentialDrag(PointerEventData eventData) => _dragDrop.InitializePotentialDrag(this, eventData);
    public void OnBeginDrag(PointerEventData eventData) => _dragDrop.BeginDrag(this, eventData);
    public void OnDrag(PointerEventData eventData) => _dragDrop.Drag(this, eventData);
    public void OnEndDrag(PointerEventData eventData) => _dragDrop.EndDrag(this, eventData);
}
