using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiveMatchCourt : MonoBehaviour
{
    [SerializeField] GameObject _shotPrefab;
    [SerializeField] RectTransform _shotContainer;

    [Header("Court Calibration")]
    [SerializeField] RectTransform _leftBasket;
    [SerializeField] RectTransform _rightBasket;
    [SerializeField] RectTransform _topSideline;
    [SerializeField] RectTransform _bottomSideline;

    [Header("Shot Presentation")]
    [SerializeField] Vector2 _fallbackMarkerSize = new Vector2(18f, 18f);
    [SerializeField] Color _madeShotColor = new Color(0.2f, 0.85f, 0.3f, 1f);
    [SerializeField] Color _missedShotColor = new Color(0.9f, 0.2f, 0.2f, 1f);
    [SerializeField] Color _blockedShotColor = new Color(1f, 0.7f, 0.1f, 1f);
    [SerializeField] Color _trajectoryColor = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField, Min(0f), Tooltip("Time taken to draw the complete curved shot path.")] float _trajectoryDuration = 0.7f;
    [SerializeField, Min(2)] int _trajectorySegments = 18;
    [SerializeField, Min(1f)] float _trajectoryWidth = 5f;
    [SerializeField, Min(0f)] float _trajectoryCurve = 45f;
    [SerializeField, Min(1f)] float _outcomePunchScale = 1.4f;
    [SerializeField, Min(0.05f)] float _outcomePunchDuration = 0.18f;
    [SerializeField, Min(1)] int _maximumVisibleShots = 24;

    const float CourtWidth = 50f;
    const float BasketToBasketDistance = 83.5f;
    const float BasketToHalfCourtDistance = BasketToBasketDistance * 0.5f;
    const float BasketFromBaseline = 5.25f;

    readonly Queue<GameObject> _shotMarkers = new Queue<GameObject>();

    public IEnumerator ShowShot(
        Vector2 courtPosition,
        bool attacksRightBasket,
        bool isMade,
        bool isBlocked,
        float outcomeDuration,
        float playbackSpeed,
        Action revealOutcome)
    {
        RectTransform container = GetShotContainer();
        Vector2 shotPoint = CourtToLocalPoint(courtPosition, attacksRightBasket);
        Vector2 basketPoint = GetBasketPoint(attacksRightBasket);
        GameObject marker = CreateMarker(container);
        RectTransform markerTransform = marker.GetComponent<RectTransform>();
        markerTransform.anchoredPosition = shotPoint;
        Image markerImage = marker.GetComponentInChildren<Image>();
        if (markerImage != null)
            markerImage.color = Color.white;

        _shotMarkers.Enqueue(marker);
        while (_shotMarkers.Count > Mathf.Max(1, _maximumVisibleShots))
            Destroy(_shotMarkers.Dequeue());

        GameObject trajectory = CreateTrajectory(container, shotPoint, basketPoint, attacksRightBasket);
        Image[] segments = trajectory.GetComponentsInChildren<Image>(true);

        float delayPerSegment = _trajectoryDuration / (Mathf.Max(1f, playbackSpeed) * Mathf.Max(1, segments.Length));
        foreach (Image segment in segments)
        {
            segment.gameObject.SetActive(true);
            if (delayPerSegment > 0f)
                yield return new WaitForSeconds(delayPerSegment);
        }

        Color outcomeColor = isBlocked ? _blockedShotColor : isMade ? _madeShotColor : _missedShotColor;
        if (outcomeDuration > 0f)
            yield return new WaitForSeconds(outcomeDuration);

        if (markerImage != null)
            markerImage.color = outcomeColor;

        revealOutcome?.Invoke();
        Destroy(trajectory);

        Vector3 normalScale = markerTransform.localScale;
        float elapsed = 0f;
        while (elapsed < _outcomePunchDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / _outcomePunchDuration);
            float punch = Mathf.Sin(progress * Mathf.PI);
            markerTransform.localScale = normalScale * Mathf.Lerp(1f, _outcomePunchScale, punch);
            yield return null;
        }

        markerTransform.localScale = normalScale;
    }

    public void ClearShots()
    {
        while (_shotMarkers.Count > 0)
        {
            GameObject marker = _shotMarkers.Dequeue();
            if (marker != null)
                Destroy(marker);
        }
    }

    RectTransform GetShotContainer()
    {
        if (_shotContainer != null)
            return _shotContainer;

        _shotContainer = transform as RectTransform;
        return _shotContainer;
    }

    Vector2 CourtToLocalPoint(Vector2 courtPosition, bool attacksRightBasket)
    {
        Vector2 basket = GetBasketPoint(attacksRightBasket);
        Vector2 oppositeBasket = GetBasketPoint(!attacksRightBasket);
        Vector2 towardHalfCourt = (oppositeBasket - basket).normalized;
        float longitudinalScale = Vector2.Distance(basket, oppositeBasket) / BasketToBasketDistance;

        Vector2 topSideline = GetCalibrationPoint(_topSideline, new Vector2(0f, GetShotContainer().rect.height * 0.5f));
        Vector2 bottomSideline = GetCalibrationPoint(_bottomSideline, new Vector2(0f, -GetShotContainer().rect.height * 0.5f));
        Vector2 towardTopSideline = (topSideline - bottomSideline).normalized;
        if (attacksRightBasket)
            towardTopSideline = -towardTopSideline;
        float lateralScale = Vector2.Distance(topSideline, bottomSideline) / CourtWidth;

        float longitudinalFeet = Mathf.Clamp(courtPosition.y, 0f, BasketToHalfCourtDistance);
        float lateralFeet = Mathf.Clamp(courtPosition.x, -CourtWidth * 0.5f, CourtWidth * 0.5f);
        return basket + towardHalfCourt * longitudinalFeet * longitudinalScale + towardTopSideline * lateralFeet * lateralScale;
    }

    Vector2 GetBasketPoint(bool rightBasket)
    {
        RectTransform container = GetShotContainer();
        RectTransform basket = rightBasket ? _rightBasket : _leftBasket;
        if (basket != null)
            return container.InverseTransformPoint(basket.position);

        float courtLength = container.rect.width;
        float basketInset = BasketFromBaseline / (BasketToBasketDistance + BasketFromBaseline * 2f) * courtLength;
        return new Vector2((rightBasket ? 1f : -1f) * (courtLength * 0.5f - basketInset), 0f);
    }

    Vector2 GetCalibrationPoint(RectTransform calibration, Vector2 fallback)
    {
        return calibration != null ? (Vector2)GetShotContainer().InverseTransformPoint(calibration.position) : fallback;
    }

    GameObject CreateTrajectory(RectTransform container, Vector2 start, Vector2 end, bool attacksRightBasket)
    {
        GameObject trajectory = new GameObject("Shot Trajectory", typeof(RectTransform));
        trajectory.layer = gameObject.layer;
        RectTransform trajectoryTransform = trajectory.GetComponent<RectTransform>();
        trajectoryTransform.SetParent(container, false);
        trajectoryTransform.anchorMin = new Vector2(0.5f, 0.5f);
        trajectoryTransform.anchorMax = new Vector2(0.5f, 0.5f);

        Vector2 direction = (end - start).normalized;
        Vector2 curveDirection = new Vector2(-direction.y, direction.x) * (attacksRightBasket ? 1f : -1f);
        Vector2 control = (start + end) * 0.5f + curveDirection * _trajectoryCurve;
        Vector2 previous = start;

        for (int i = 1; i <= Mathf.Max(2, _trajectorySegments); i++)
        {
            float t = i / (float)Mathf.Max(2, _trajectorySegments);
            Vector2 point = QuadraticBezier(start, control, end, t);
            CreateTrajectorySegment(trajectoryTransform, previous, point);
            previous = point;
        }

        return trajectory;
    }

    void CreateTrajectorySegment(RectTransform parent, Vector2 start, Vector2 end)
    {
        GameObject segment = new GameObject("Trajectory Segment", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        segment.layer = gameObject.layer;
        RectTransform segmentTransform = segment.GetComponent<RectTransform>();
        segmentTransform.SetParent(parent, false);
        segmentTransform.anchorMin = new Vector2(0.5f, 0.5f);
        segmentTransform.anchorMax = new Vector2(0.5f, 0.5f);

        Vector2 delta = end - start;
        segmentTransform.sizeDelta = new Vector2(delta.magnitude, _trajectoryWidth);
        segmentTransform.anchoredPosition = (start + end) * 0.5f;
        segmentTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

        Image image = segment.GetComponent<Image>();
        image.color = _trajectoryColor;
        image.raycastTarget = false;
        segment.SetActive(false);
    }

    static Vector2 QuadraticBezier(Vector2 start, Vector2 control, Vector2 end, float t)
    {
        float inverse = 1f - t;
        return inverse * inverse * start + 2f * inverse * t * control + t * t * end;
    }

    GameObject CreateMarker(RectTransform container)
    {
        if (_shotPrefab != null)
            return Instantiate(_shotPrefab, container, false);

        GameObject marker = new GameObject("Shot Marker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        marker.layer = gameObject.layer;
        RectTransform markerTransform = marker.GetComponent<RectTransform>();
        markerTransform.SetParent(container, false);
        markerTransform.anchorMin = new Vector2(0.5f, 0.5f);
        markerTransform.anchorMax = new Vector2(0.5f, 0.5f);
        markerTransform.sizeDelta = _fallbackMarkerSize;
        marker.GetComponent<Image>().raycastTarget = false;
        return marker;
    }

    void OnDisable()
    {
        ClearShots();
    }
}
