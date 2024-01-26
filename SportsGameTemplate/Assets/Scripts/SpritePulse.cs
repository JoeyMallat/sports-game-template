using UnityEngine;

public class SpritePulse : MonoBehaviour
{
    SpriteRenderer _spriteRenderer;
    [SerializeField] bool _isPulsing;
    [SerializeField] AnimationCurve _pulsingCurve;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TogglePulse()
    {
        _isPulsing = !_isPulsing;
        Color color = _spriteRenderer.color;

        if (_isPulsing)
        {
            LeanTween.value(0f, 0.5f, .6f).setEase(_pulsingCurve).setLoopPingPong().setOnUpdate((x) =>
            {
                _spriteRenderer.color = new Color(color.r, color.g, color.b, x);
            });
        }
        else
        {
            LeanTween.cancel(gameObject);
            _spriteRenderer.color = new Color(color.r, color.g, color.b, 0);
        }
    }
}
