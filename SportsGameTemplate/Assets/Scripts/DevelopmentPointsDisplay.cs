using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class DevelopmentPointsDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject _visibilityRoot;
    [SerializeField] TextMeshProUGUI _balanceText;
    [SerializeField] TextMeshProUGUI _transactionAmountText;

    [Header("Balance Animation")]
    [SerializeField, Min(0f)] float _numberAnimationDuration = 0.35f;
    [SerializeField, Min(1f)] float _punchScale = 1.2f;
    [SerializeField, Min(0f)] float _punchDuration = 0.3f;
    [SerializeField] AnimationCurve _numberAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] Color _gainColor = new Color(1f, 0.6f, 0f, 1f);
    [SerializeField] Color _spendColor = new Color(0.9f, 0.15f, 0.15f, 1f);

    [Header("Amount Animation")]
    [SerializeField, Min(0f)] float _amountAnimationDuration = 0.7f;
    [SerializeField, Min(0f)] float _amountStartScale = 0.8f;
    [SerializeField, Min(0f)] float _amountPeakScale = 1.2f;
    [SerializeField] Vector2 _amountMovement = new Vector2(0f, 45f);
    [SerializeField] Color _amountGainColor = new Color(0.2f, 0.85f, 0.3f, 1f);
    [SerializeField] Color _amountSpendColor = new Color(0.9f, 0.15f, 0.15f, 1f);
    [SerializeField] AnimationCurve _amountMovementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] AnimationCurve _amountFadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Formatting")]
    [SerializeField] bool _appendDpSuffix = true;
    [SerializeField] bool _abbreviateLargeBalances;

    readonly Queue<DevelopmentPointsChange> _animationQueue = new Queue<DevelopmentPointsChange>();

    Coroutine _animationCoroutine;
    Color _balanceBaseColor;
    Color _amountBaseColor;
    Vector3 _balanceBaseScale;
    Vector3 _amountBaseScale;
    Vector2 _amountBasePosition;
    int _displayedBalance;
    int _lastRenderedBalance = int.MinValue;
    bool _isLiveSimulation;

    void Awake()
    {
        if (_balanceText == null)
            _balanceText = GetComponent<TextMeshProUGUI>();

        if (_balanceText != null)
        {
            _balanceBaseColor = _balanceText.color;
            _balanceBaseScale = _balanceText.rectTransform.localScale;
        }

        if (_transactionAmountText != null)
        {
            _amountBaseColor = _transactionAmountText.color;
            _amountBaseScale = _transactionAmountText.rectTransform.localScale;
            _amountBasePosition = _transactionAmountText.rectTransform.anchoredPosition;
            _transactionAmountText.gameObject.SetActive(false);
        }

        DevelopmentPointsManager.OnDevelopmentPointsChanged += QueueChange;
        LocalSaveManager.OnCareerAvailable += ShowAndRefresh;
        Navigation.OnScreenOpened += HandleScreenOpened;
    }

    void OnEnable()
    {
        _animationQueue.Clear();
        _isLiveSimulation = Navigation.Instance != null && Navigation.Instance.CurrentScreen == CanvasKey.LiveMatch;
        UpdateVisibility();
        if (LocalSaveManager.IsCareerActive && !_isLiveSimulation)
            RefreshDisplay();
    }

    void OnDisable()
    {
        _animationQueue.Clear();
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
        RestoreAppearance();
    }

    void OnDestroy()
    {
        DevelopmentPointsManager.OnDevelopmentPointsChanged -= QueueChange;
        LocalSaveManager.OnCareerAvailable -= ShowAndRefresh;
        Navigation.OnScreenOpened -= HandleScreenOpened;
    }

    public void RefreshDisplay()
    {
        if (DevelopmentPointsManager.Instance == null || _balanceText == null)
            return;

        _displayedBalance = DevelopmentPointsManager.Instance.CurrentBalance;
        _lastRenderedBalance = int.MinValue;
        SetBalanceText(_displayedBalance);
    }

    void ShowAndRefresh()
    {
        UpdateVisibility();
        if (!_isLiveSimulation)
            RefreshDisplay();
    }

    void HandleScreenOpened(CanvasKey screen)
    {
        _isLiveSimulation = screen == CanvasKey.LiveMatch;
        UpdateVisibility();
        if (!_isLiveSimulation && LocalSaveManager.IsCareerActive)
            RefreshDisplay();
    }

    void UpdateVisibility()
    {
        bool visible = LocalSaveManager.IsCareerActive && !_isLiveSimulation;
        if (_visibilityRoot != null)
            _visibilityRoot.SetActive(visible);
        else if (_balanceText != null)
            _balanceText.enabled = visible;

        if (!visible && _transactionAmountText != null)
            _transactionAmountText.gameObject.SetActive(false);
    }

    public string PreviewCost(int cost)
    {
        return "-" + FormatBalance(Mathf.Max(0, cost), _appendDpSuffix);
    }

    public string PreviewReward(int reward)
    {
        return "+" + FormatBalance(Mathf.Max(0, reward), _appendDpSuffix);
    }

    public static string FormatBalance(int balance, bool appendDpSuffix = true)
    {
        string formatted = Mathf.Max(0, balance).ToString("N0", CultureInfo.InvariantCulture);
        return appendDpSuffix ? formatted + " DP" : formatted;
    }

    void QueueChange(DevelopmentPointsChange change)
    {
        if (!LocalSaveManager.IsCareerActive || !isActiveAndEnabled || _isLiveSimulation)
            return;

        _animationQueue.Enqueue(change);
        if (_animationCoroutine == null && isActiveAndEnabled)
            _animationCoroutine = StartCoroutine(PlayQueuedAnimations());
    }

    IEnumerator PlayQueuedAnimations()
    {
        while (_animationQueue.Count > 0)
            yield return AnimateChange(_animationQueue.Dequeue());

        _animationCoroutine = null;
    }

    IEnumerator AnimateChange(DevelopmentPointsChange change)
    {
        int startBalance = _displayedBalance;
        int targetBalance = change.NewBalance;
        float totalDuration = Mathf.Max(_numberAnimationDuration, _punchDuration, _transactionAmountText != null ? _amountAnimationDuration : 0f);
        Color highlightColor = change.AmountChanged >= 0 ? _gainColor : _spendColor;

        PrepareAmountText(change.AmountChanged);

        if (totalDuration <= 0f)
        {
            _displayedBalance = targetBalance;
            SetBalanceText(targetBalance);
            RestoreAppearance();
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < totalDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            AnimateBalance(startBalance, targetBalance, highlightColor, elapsed);
            AnimateAmount(change.AmountChanged, elapsed);
            yield return null;
        }

        _displayedBalance = targetBalance;
        SetBalanceText(targetBalance);
        RestoreAppearance();
    }

    void AnimateBalance(int startBalance, int targetBalance, Color highlightColor, float elapsed)
    {
        float numberProgress = _numberAnimationDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / _numberAnimationDuration);
        float curvedProgress = _numberAnimationCurve.Evaluate(numberProgress);
        _displayedBalance = Mathf.RoundToInt(Mathf.Lerp(startBalance, targetBalance, curvedProgress));
        SetBalanceText(_displayedBalance);

        float punchProgress = _punchDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / _punchDuration);
        float punchWeight = 1f - Mathf.Abs((punchProgress * 2f) - 1f);
        _balanceText.rectTransform.localScale = _balanceBaseScale * Mathf.Lerp(1f, _punchScale, punchWeight);
        _balanceText.color = Color.Lerp(highlightColor, _balanceBaseColor, numberProgress);
    }

    void PrepareAmountText(int amount)
    {
        if (_transactionAmountText == null)
            return;

        _transactionAmountText.gameObject.SetActive(true);
        _transactionAmountText.SetText(amount >= 0 ? "+{0}" : "{0}", amount);
        _transactionAmountText.color = amount >= 0 ? _amountGainColor : _amountSpendColor;
        _transactionAmountText.rectTransform.anchoredPosition = _amountBasePosition;
        _transactionAmountText.rectTransform.localScale = _amountBaseScale * _amountStartScale;
    }

    void AnimateAmount(int amount, float elapsed)
    {
        if (_transactionAmountText == null)
            return;

        float progress = _amountAnimationDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / _amountAnimationDuration);
        float movementProgress = _amountMovementCurve.Evaluate(progress);
        _transactionAmountText.rectTransform.anchoredPosition = _amountBasePosition + (_amountMovement * movementProgress);

        float scaleWeight = 1f - Mathf.Abs((progress * 2f) - 1f);
        float scale = Mathf.Lerp(_amountStartScale, _amountPeakScale, scaleWeight);
        _transactionAmountText.rectTransform.localScale = _amountBaseScale * scale;

        Color color = amount >= 0 ? _amountGainColor : _amountSpendColor;
        color.a *= Mathf.Clamp01(_amountFadeCurve.Evaluate(progress));
        _transactionAmountText.color = color;
    }

    void SetBalanceText(int balance)
    {
        if (_balanceText == null || balance == _lastRenderedBalance)
            return;

        _lastRenderedBalance = balance;
        if (_abbreviateLargeBalances && balance >= 1000)
        {
            if (balance >= 1000000000)
                SetAbbreviatedBalance(balance / 1000000000f, "B");
            else if (balance >= 1000000)
                SetAbbreviatedBalance(balance / 1000000f, "M");
            else
                SetAbbreviatedBalance(balance / 1000f, "K");
            return;
        }

        _balanceText.text = FormatBalance(balance, _appendDpSuffix);
    }

    void SetAbbreviatedBalance(float balance, string suffix)
    {
        string formatted = balance.ToString("0.#", CultureInfo.InvariantCulture) + suffix;
        _balanceText.text = _appendDpSuffix ? formatted + " DP" : formatted;
    }

    void RestoreAppearance()
    {
        if (_balanceText != null)
        {
            _balanceText.color = _balanceBaseColor;
            _balanceText.rectTransform.localScale = _balanceBaseScale;
        }

        if (_transactionAmountText != null)
        {
            _transactionAmountText.color = _amountBaseColor;
            _transactionAmountText.rectTransform.localScale = _amountBaseScale;
            _transactionAmountText.rectTransform.anchoredPosition = _amountBasePosition;
            _transactionAmountText.gameObject.SetActive(false);
        }
    }
}
