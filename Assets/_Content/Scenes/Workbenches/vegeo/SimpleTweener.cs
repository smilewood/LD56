using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SimpleTweener : MonoBehaviour
{
    public Transform TweenTarget;
    public bool TweenOnEnable = true;
    public bool TweenPosition = true;
    public bool TweenRotation = false;
    public bool TweenScale = false;
    public float TweenDuration = 0.4f;
    public Ease TweenEase = Ease.OutSine;
    public float TweenDelay = 0f;

    private Tween _positionTween;
    private Tween _rotationTween;
    private Tween _scaleTween;

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        if (!TweenOnEnable) return;

        if (TweenPosition)
            transform.DOLocalMove(TweenTarget.localPosition, TweenDuration).SetEase(TweenEase).SetDelay(TweenDelay).From();
        if (TweenRotation)
            transform.DOLocalRotate(TweenTarget.localRotation.eulerAngles, TweenDuration).SetEase(TweenEase).SetDelay(TweenDelay).From();
        if (TweenScale)
            transform.DOScale(TweenTarget.localScale, TweenDuration).SetEase(TweenEase).SetDelay(TweenDelay).From();
    }

    public void OnDisable()
    {
        transform.DOComplete();
    }
}
