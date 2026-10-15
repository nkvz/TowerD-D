using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TowerService
{
    private const float PlacementJitterFraction = 0.1f;

    private readonly List<Cube> _cubes = new List<Cube>();
    private readonly IPlacementRule _placementRule;
    private RectTransform _area;
    private RectTransform _hole;
    private RectTransform _holeMask;
    private RectTransform _cubeBar;
    private GameFactory _factory;
    private Action<GameMessage> _report;
    private bool _heightLimitReached;

    public TowerService(IPlacementRule placementRule)
    {
        _placementRule = placementRule;
    }

    public void Initialize(RectTransform towerArea, RectTransform holeRect, RectTransform holeMask, RectTransform cubeBar,
        GameFactory gameFactory, Action<GameMessage> reportAction)
    {
        _area = towerArea;
        _hole = holeRect;
        _holeMask = holeMask;
        _cubeBar = cubeBar;
        _factory = gameFactory;
        _report = reportAction;
    }

    public bool TryPlace(Cube cube, Vector2 screenPoint, Camera camera, Action completed)
    {
        _heightLimitReached = false;
        Cube top = _cubes.Count > 0 ? _cubes[_cubes.Count - 1] : null;
        if (!TryGetDestination(cube, screenPoint, camera, top, 0f, false, out Vector2 destination))
            return false;

        Place(cube, destination, top != null, completed);
        return true;
    }

    private bool TryGetDestination(Cube cube, Vector2 screenPoint, Camera camera, Cube top, float pendingFallDistance, bool allowDropOverTop, out Vector2 destination)
    {
        destination = default;
        if (_area == null || !RectTransformUtility.RectangleContainsScreenPoint(_area, screenPoint, camera))
            return false;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_area, screenPoint, camera, out Vector2 dropPoint))
            return false;

        float floorY = GetFloorY(_area);
        if (dropPoint.y < floorY)
            return false;

        Vector2 size = cube.RectTransform.rect.size;
        if (top == null)
        {
            destination.x = Mathf.Clamp(dropPoint.x, _area.rect.xMin + size.x * 0.5f, _area.rect.xMax - size.x * 0.5f);
            destination.y = floorY + size.y * 0.5f;
        }
        else
        {
            Vector2 topPosition = top.RectTransform.anchoredPosition;
            topPosition.y -= pendingFallDistance;
            float topEdge = topPosition.y + top.RectTransform.rect.height * 0.5f;
            float landingY = topEdge + size.y * 0.5f;
            float minimumDropY = allowDropOverTop ? topPosition.y - top.RectTransform.rect.height * 0.5f : topEdge;

            // The drop must align with the future top of the tower.
            if (dropPoint.y < minimumDropY || Mathf.Abs(dropPoint.x - topPosition.x) > top.RectTransform.rect.width * 0.5f || !_placementRule.CanPlace(cube, top))
                return false;

            if (landingY + size.y * 0.5f > _area.rect.yMax)
            {
                _heightLimitReached = true;
                return false;
            }

            if (!allowDropOverTop && dropPoint.y < landingY)
                return false;

            float maxOffset = Mathf.Min(size.x, top.RectTransform.rect.width) * 0.5f;
            float requestedOffset = Mathf.Clamp(dropPoint.x - topPosition.x, -maxOffset, maxOffset);
            float jitter = UnityEngine.Random.Range(-maxOffset * PlacementJitterFraction, maxOffset * PlacementJitterFraction);
            float offset = Mathf.Clamp(requestedOffset + jitter, -maxOffset, maxOffset);

            // Randomness must not reverse the side chosen by the player.
            if (requestedOffset > 0f) offset = Mathf.Max(0f, offset);
            else if (requestedOffset < 0f) offset = Mathf.Min(0f, offset);

            destination = topPosition;
            destination.x += offset;
            destination.x = Mathf.Clamp(destination.x, _area.rect.xMin + size.x * 0.5f, _area.rect.xMax - size.x * 0.5f);
            destination.y = landingY;
        }

        return true;
    }

    private float GetFloorY(RectTransform area)
    {
        // The areas fill the screen, while the cube bar covers their lower part.
        Vector3 barTop = _cubeBar.TransformPoint(new Vector3(0f, _cubeBar.rect.yMax, 0f));
        return area.InverseTransformPoint(barTop).y;
    }

    private void Place(Cube cube, Vector2 destination, bool hasCubeBelow, Action completed)
    {
        cube.transform.SetParent(_area, true);
        cube.SetRaycastTarget(false);
        _cubes.Add(cube);
        _report?.Invoke(GameMessage.CubePlaced);

        RectTransform cubeRect = cube.RectTransform;
        cubeRect.anchoredPosition = new Vector2(destination.x, Mathf.Max(cubeRect.anchoredPosition.y, destination.y));
        cubeRect.localRotation = Quaternion.identity;

        Sequence placement = DOTween.Sequence().SetTarget(cubeRect);
        if (hasCubeBelow)
        {
            float startY = cubeRect.anchoredPosition.y;
            float jumpHeight = Mathf.Min(cubeRect.rect.height * 0.22f, 32f);
            float highestVisibleY = _area.rect.yMax - cubeRect.rect.height * 0.5f;
            float jumpY = Mathf.Max(startY, Mathf.Min(startY + jumpHeight, highestVisibleY));

            placement.Append(cubeRect.DOAnchorPosY(jumpY, 0.2f).SetEase(Ease.OutQuad));
            placement.Join(cubeRect.DOLocalRotate(new Vector3(0f, 0f, 360f), 0.2f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic));
            placement.Append(cubeRect.DOAnchorPosY(destination.y, 0.35f).SetEase(Ease.InQuad));
        }
        else
        {
            placement.Append(cubeRect.DOAnchorPosY(destination.y, 0.42f).SetEase(Ease.InQuad));
            placement.Join(cubeRect.DOLocalRotate(new Vector3(0f, 0f, 360f), 0.24f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic));
        }

        placement.OnComplete(() =>
        {
            cubeRect.localRotation = Quaternion.identity;
            cube.SetRaycastTarget(true);
            completed?.Invoke();
        });
    }

    public bool TryRepositionTowerCube(Cube cube, Vector2 screenPoint, Camera camera, Action completed)
    {
        int index = _cubes.IndexOf(cube);
        if (index < 0 ||
            _area == null || !RectTransformUtility.RectangleContainsScreenPoint(_area, screenPoint, camera))
            return false;

        _heightLimitReached = false;
        GetTowerAfterRemoval(index, out Cube top, out float pendingFallDistance);
        if (!TryGetDestination(cube, screenPoint, camera, top, pendingFallDistance, true, out Vector2 destination))
        {
            Discard(cube, GameMessage.CubeMissed, completed);
            return true;
        }

        RemoveFromTower(cube);
        Place(cube, destination, top != null, completed);
        return true;
    }

    private void GetTowerAfterRemoval(int index, out Cube top, out float fallDistance)
    {
        fallDistance = 0f;
        if (_cubes.Count == 1)
        {
            top = null;
            return;
        }

        if (index == _cubes.Count - 1)
        {
            top = _cubes[index - 1];
            return;
        }

        if (index == 0)
        {
            RectTransform newBase = _cubes[1].RectTransform;
            float landingY = GetFloorY(_area) + newBase.rect.height * 0.5f;
            fallDistance = Mathf.Max(0f, newBase.anchoredPosition.y - landingY);
            top = _cubes[_cubes.Count - 1];
            return;
        }

        RectTransform lower = _cubes[index - 1].RectTransform;
        RectTransform upper = _cubes[index + 1].RectTransform;
        if (!HasSupport(_cubes[index + 1], _cubes[index - 1]))
        {
            top = _cubes[index - 1];
            return;
        }

        float landing = lower.anchoredPosition.y + (lower.rect.height + upper.rect.height) * 0.5f;
        fallDistance = Mathf.Max(0f, upper.anchoredPosition.y - landing);
        top = _cubes[_cubes.Count - 1];
    }

    public bool TryThrow(Cube cube, Vector2 screenPoint, Camera camera, Action completed)
    {
        if (!TryGetHoleImpact(screenPoint, camera, out Vector3 impact) || !(_hole.parent is RectTransform holeArea))
            return false;

        Vector3 localImpact = holeArea.InverseTransformPoint(impact);
        float floorY = GetFloorY(holeArea) + cube.RectTransform.rect.height * 0.5f;
        Vector3 floor = holeArea.TransformPoint(new Vector3(localImpact.x, floorY, localImpact.z));

        cube.transform.SetParent(_holeMask, true);
        cube.transform.SetAsLastSibling();
        RemoveFromTower(cube);
        _report?.Invoke(GameMessage.CubeRemoved);

        cube.SetRaycastTarget(false);
        DOTween.Sequence()
            .Append(cube.transform.DOMove(impact, 0.4f).SetEase(Ease.InQuad))
            .Append(cube.transform.DOMove(floor, 0.42f).SetEase(Ease.InQuad))
            .Append(cube.RectTransform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack))
            .OnComplete(() => Finish(cube, completed));
        return true;
    }

    public void Discard(Cube cube, GameMessage message, Action completed)
    {
        RemoveFromTower(cube);
        cube.SetRaycastTarget(false);
        _report?.Invoke(_heightLimitReached ? GameMessage.HeightLimit : message);
        _heightLimitReached = false;

        float fallDistance = Mathf.Max(cube.RectTransform.rect.height * 1.2f, 60f);
        DOTween.Sequence()
            .Append(cube.RectTransform.DOAnchorPosY(cube.RectTransform.anchoredPosition.y - fallDistance, 0.25f).SetEase(Ease.InQuad))
            .Append(cube.RectTransform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack))
            .OnComplete(() => Finish(cube, completed));
    }

    private float RemoveFromTower(Cube cube)
    {
        int index = _cubes.IndexOf(cube);
        if (index < 0)
            return 0f;

        _cubes.RemoveAt(index);
        if (index >= _cubes.Count)
            return 0f;

        if (index == 0)
        {
            RectTransform newBase = _cubes[0].RectTransform;
            return MoveStackDown(0, GetFloorY(_area) + newBase.rect.height * 0.5f);
        }

        // For a gap inside the tower, the upper section needs a cube below it.
        if (!HasSupport(_cubes[index], _cubes[index - 1]))
        {
            DropUnsupportedCubes(index);
            return 0f;
        }

        RectTransform lower = _cubes[index - 1].RectTransform;
        RectTransform upper = _cubes[index].RectTransform;
        float landingY = lower.anchoredPosition.y + (lower.rect.height + upper.rect.height) * 0.5f;
        return MoveStackDown(index, landingY);
    }

    private float MoveStackDown(int firstIndex, float landingY)
    {
        RectTransform first = _cubes[firstIndex].RectTransform;
        float fallDistance = Mathf.Max(0f, first.anchoredPosition.y - landingY);

        for (int i = firstIndex; i < _cubes.Count; i++)
        {
            RectTransform item = _cubes[i].RectTransform;
            item.DOAnchorPosY(item.anchoredPosition.y - fallDistance, 0.35f).SetEase(Ease.InQuad);
        }

        return fallDistance;
    }

    private static bool HasSupport(Cube upperCube, Cube lowerCube)
    {
        RectTransform upper = upperCube.RectTransform;
        RectTransform lower = lowerCube.RectTransform;
        float upperLeft = upper.anchoredPosition.x - upper.rect.width * 0.5f;
        float upperRight = upper.anchoredPosition.x + upper.rect.width * 0.5f;
        float lowerLeft = lower.anchoredPosition.x - lower.rect.width * 0.5f;
        float lowerRight = lower.anchoredPosition.x + lower.rect.width * 0.5f;

        return Mathf.Min(upperRight, lowerRight) > Mathf.Max(upperLeft, lowerLeft);
    }

    private void DropUnsupportedCubes(int firstIndex)
    {
        for (int i = _cubes.Count - 1; i >= firstIndex; i--)
        {
            Cube fallingCube = _cubes[i];
            _cubes.RemoveAt(i);
            fallingCube.SetRaycastTarget(false);

            RectTransform rect = fallingCube.RectTransform;
            float belowArea = _area.rect.yMin - rect.rect.height * 0.5f;
            DOTween.Sequence()
                .SetTarget(rect)
                .Append(rect.DOAnchorPosY(belowArea, 0.38f).SetEase(Ease.InQuad))
                .Append(rect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack))
                .OnComplete(() => _factory.ReleaseCube(fallingCube));
        }
    }

    private bool TryGetHoleImpact(Vector2 screenPoint, Camera camera, out Vector3 impact)
    {
        impact = Vector3.zero;
        if (_hole == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(_hole, screenPoint, camera, out Vector2 local))
            return false;

        Rect rect = _hole.rect;
        if (rect.width <= 0f || rect.height <= 0f)
            return false;

        float x = (local.x - rect.center.x) / (rect.width * 0.5f);
        if (Mathf.Abs(x) > 1f)
            return false;

        float lowerEdge = rect.center.y - rect.height * 0.5f * Mathf.Sqrt(1f - x * x);
        if (local.y < lowerEdge)
            return false;

        impact = _hole.TransformPoint(new Vector3(local.x, Mathf.Min(local.y, rect.center.y), 0f));
        return true;
    }

    private void Finish(Cube cube, Action completed)
    {
        _factory.ReleaseCube(cube);
        completed?.Invoke();
    }
}
