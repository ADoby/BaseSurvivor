using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class DOTweenHideable : Hideable
{
    public DOTweenAnimationEx[] ShowAnimations;
    public DOTweenAnimationEx[] HideAnimations;
    private Sequence sequence;

    #region PRIVATE

    private void Awake()
    {
        for (int i = 0; i < ShowAnimations.Length; i++)
        {
            if (ShowAnimations[i] == null)
                continue;
            ShowAnimations[i].Init();
        }
        for (int i = 0; i < HideAnimations.Length; i++)
        {
            if (HideAnimations[i] == null)
                continue;
            HideAnimations[i].Init();
        }
    }

    private void StartSequence(DOTweenAnimationEx[] animations, DG.Tweening.TweenCallback callback, float delay)
    {
        if (sequence != null)
        {
            //Tween running
            sequence.Kill();
        }

        sequence = DOTween.Sequence();
        AddTweens(sequence, animations);
        sequence.OnComplete(callback);
        sequence.SetDelay(delay);
        sequence.OnKill(() => sequence = null);
        sequence.Play();
    }

    private void AddTweens(Sequence sequence, DOTweenAnimationEx[] animations)
    {
        for (int i = 0; i < animations.Length; i++)
        {
            if (animations[i] != null)
            {
                animations[i].gameObject = gameObject;
                animations[i].transform = transform;
                sequence.Insert(0f, animations[i].CreateTween(true));
            }
        }
    }

    #endregion PRIVATE

    #region PUBLIC

    protected override void DoShow(bool instant = false)
    {
        base.DoShow(instant);
        StartSequence(ShowAnimations, ShowFinished, ShowDelay);
    }

    protected override void DoHide(bool instant = false)
    {
        base.DoHide(instant);
        StartSequence(HideAnimations, HideFinished, HideDelay);
    }

    public override float Duration(bool show)
    {
        DOTweenAnimationEx[] array = ShowAnimations;
        if (!show)
            array = HideAnimations;

        float maxTime = 0f;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                if (array[i].delay + array[i].duration > maxTime)
                {
                    maxTime = array[i].duration + array[i].delay;
                }
            }
        }
        return maxTime;
    }

    public override void SetPosition(object sender, float percentage, bool show)
    {
        DOTweenAnimationEx[] array = ShowAnimations;
        if (!show)
            array = HideAnimations;

        if (sender != null)
            SetPosition(null, 1f, !show);

        Tween tween;
        float time = Duration(show) * percentage;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                tween = array[i].CreateTween(true);

                tween.Goto(Mathf.Max(time - array[i].delay, 0));
            }
        }
        CurrentTime = time;

        if (percentage == 1f)
        {
            if (show)
                State = STATES.SHOWN;
            else
                State = STATES.HIDDEN;
        }
    }

    #endregion PUBLIC
}