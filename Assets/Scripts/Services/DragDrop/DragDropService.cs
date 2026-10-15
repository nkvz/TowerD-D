using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDropService
{
    private const float GestureThreshold = 12f;
    private const float VerticalDragThreshold = 18f;
    private const float HorizontalScrollThreshold = 35f;

    private ScrollRect _scrollRect;
    private RectTransform _dragLayer;
    private TowerService _tower;
    private GameFactory _factory;
    private Cube _source;
    private Cube _moving;
    private Vector2 _pressPosition;
    private int _pointerId;
    private bool _scrolling;
    private bool _locked;

    public void Initialize(ScrollRect scroll, RectTransform layer, TowerService towerService, GameFactory gameFactory)
    {
        _scrollRect = scroll;
        _dragLayer = layer;
        _tower = towerService;
        _factory = gameFactory;
    }

    public void PointerDown(Cube cube, PointerEventData eventData)
    {
        if (_locked || _source != null || _scrollRect == null)
            return;

        _source = cube;
        _pointerId = eventData.pointerId;
        _pressPosition = eventData.position;
    }

    public void PointerUp(Cube cube, PointerEventData eventData)
    {
        if (cube == _source && eventData.pointerId == _pointerId && _moving == null && !_scrolling)
            Clear();
    }

    public void InitializePotentialDrag(Cube cube, PointerEventData eventData)
    {
        if (cube == _source && eventData.pointerId == _pointerId && cube.IsPaletteCube)
            _scrollRect.OnInitializePotentialDrag(eventData);
    }

    public void BeginDrag(Cube cube, PointerEventData eventData) => DecideGesture(cube, eventData);

    public void Drag(Cube cube, PointerEventData eventData)
    {
        DecideGesture(cube, eventData);
        if (cube != _source || eventData.pointerId != _pointerId)
            return;

        if (_scrolling)
            _scrollRect.OnDrag(eventData);
        else if (_moving != null)
            MoveToPointer(eventData);
    }

    public void EndDrag(Cube cube, PointerEventData eventData)
    {
        if (cube != _source || eventData.pointerId != _pointerId)
            return;

        if (_scrolling)
        {
            _scrollRect.OnEndDrag(eventData);
            Clear();
            return;
        }

        if (_moving == null)
        {
            Clear();
            return;
        }

        Cube dropped = _moving;
        bool fromPalette = _source.IsPaletteCube;
        Clear();
        _locked = true;

        if (_tower.TryThrow(dropped, eventData.position, eventData.pressEventCamera, Unlock))
            return;

        if (fromPalette)
        {
            if (!_tower.TryPlace(dropped, eventData.position, eventData.pressEventCamera, Unlock))
                _tower.Discard(dropped, GameMessage.CubeMissed, Unlock);
        }
        else if (!_tower.TryRepositionTowerCube(dropped, eventData.position, eventData.pressEventCamera, Unlock))
            _tower.Discard(dropped, GameMessage.HoleMissed, Unlock);
    }

    private void DecideGesture(Cube cube, PointerEventData eventData)
    {
        if (cube != _source || eventData.pointerId != _pointerId || _scrolling || _moving != null)
            return;

        Vector2 delta = eventData.position - _pressPosition;
        if (delta.sqrMagnitude < GestureThreshold * GestureThreshold)
            return;

        if (cube.IsPaletteCube && Mathf.Abs(delta.y) < VerticalDragThreshold)
        {
            if (Mathf.Abs(delta.x) >= HorizontalScrollThreshold)
            {
                _scrolling = true;
                _scrollRect.OnBeginDrag(eventData);
            }
            return;
        }

        if (cube.IsPaletteCube)
        {
            _moving = _factory.CreateCube(cube.Config, _dragLayer, false);
        }
        else
        {
            _moving = cube;
            cube.transform.SetParent(_dragLayer, true);
        }

        // The prefab is anchored at the lower left for layout. Free dragging and
        // tower positions use the centre of their parent as the coordinate origin.
        _moving.RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        _moving.RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        _moving.SetRaycastTarget(false);
        _moving.transform.SetAsLastSibling();
        MoveToPointer(eventData);
    }

    private void MoveToPointer(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragLayer, eventData.position, eventData.pressEventCamera, out Vector2 point))
            _moving.RectTransform.anchoredPosition = point;
    }

    private void Clear()
    {
        _source = null;
        _moving = null;
        _scrolling = false;
    }

    private void Unlock() => _locked = false;
}
