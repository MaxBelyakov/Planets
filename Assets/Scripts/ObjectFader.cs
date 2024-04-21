using System.Collections;
using UnityEngine;

// Put it on objects that should fade in/out between switching scenes

public class ObjectFader : MonoBehaviour
{
    private float _fadeDuration = 1f;

    private Material _material;
    private Color _startColor;

    private LineRenderer _lineRenderer;
    private Color _lineStartColor;

    // Use it to wait to switch scene while fading is in progress
    public bool IsFading { get; private set; } = false;

    // Use it to skip hiding and destroying selected object
    public bool IgnoreFader { get; set; } = false;

    void Start()
    {
        _material = GetComponent<Renderer>().material;
        _startColor = _material.color;
        _startColor.a = 0f;
        _material.color = _startColor;

        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer != null)
        {
            _lineStartColor = _lineRenderer.material.color;
            _lineStartColor.a = 0f;
            _lineRenderer.material.color = _lineStartColor;
        }
    }

    public void FadeIn()
    {
        StartCoroutine(FadeObject(_material.color.a, 1f, _fadeDuration));
        if (_lineRenderer != null)
            StartCoroutine(FadeLineRenderer(_lineRenderer.material.color.a, 1f, _fadeDuration));
    }

    public void FadeOut()
    {
        StartCoroutine(FadeObject(_material.color.a, 0f, _fadeDuration));
        if (_lineRenderer != null)
            StartCoroutine(FadeLineRenderer(_lineRenderer.material.color.a, 0f, _fadeDuration));
    }

    private IEnumerator FadeObject(float startAlpha, float endAlpha, float duration)
    {
        IsFading = true;
        float startTime = Time.time;
        Color color = _material.color;
        while (Time.time < startTime + duration)
        {
            color.a = Mathf.Lerp(startAlpha, endAlpha, (Time.time - startTime) / duration);
            _material.color = color;
            yield return null;
        }
        color.a = endAlpha;
        _material.color = color;
        IsFading = false;
    }

    private IEnumerator FadeLineRenderer(float startAlpha, float endAlpha, float duration)
    {
        IsFading = true;
        float startTime = Time.time;
        Color color = _lineRenderer.material.color;
        while (Time.time < startTime + duration)
        {
            color.a = Mathf.Lerp(startAlpha, endAlpha, (Time.time - startTime) / duration);
            _lineRenderer.material.color = color;
            yield return null;
        }
        color.a = endAlpha;
        _lineRenderer.material.color = color;
        IsFading = false;
    }
}
