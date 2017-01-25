using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class HideableList : Hideable
{
    public float DelayBetween = 0.25f;
    public IHideable[] List;

    public override float Duration(bool state)
    {
        float maxtime = 0f;
        int foundChilds = 0;
        for (int i = 0; i < List.Length; i++)
        {
            if (List[i] != null)
            {
                foundChilds++;
                if (foundChilds >= 2)
                    maxtime += DelayBetween;
                maxtime += List[i].Duration(state);
            }
        }
        return maxtime;
    }

    #region PRIVATE

    private void Set(bool show)
    {
        int foundChilds = 0;
        int lastIndexFound = 0;
        for (int i = 0; i < List.Length; i++)
        {
            if (List[i] == null)
                continue;
            foundChilds++;
            List[i].SetFinishedListener(null);
            if (!show && foundChilds == 1)
            {
                List[i].SetFinishedListener(HideFinished);
            }
            lastIndexFound = i;
        }
        if (show)
        {
            List[lastIndexFound].SetFinishedListener(ShowFinished);
        }

        int currentChild = 0;
        float delay = 0f;
        for (int i = 0; i < List.Length; i++)
        {
            if (List[i] == null)
                continue;
            delay = DelayBetween * currentChild;
            currentChild++;
            if (!ReverseDelayWhenHiding && !show)
            {
                delay = DelayBetween * (foundChilds - currentChild);
            }

            List[i].SetDelay(show, delay);
            List[i].SetVisibility(show);
        }
    }

    #endregion PRIVATE

    #region PUBLIC

    public override void SetPosition(object sender, float percentage, bool show)
    {
        float maxtime = 0f;
        int foundChilds = 0;
        for (int i = 0; i < List.Length; i++)
        {
            if (List[i] != null)
            {
                foundChilds++;
                if (foundChilds >= 2)
                    maxtime += DelayBetween;
                maxtime += List[i].Duration(show);
            }
        }

        float timer = maxtime * percentage;

        float time = 0f;
        float perc = 0f;
        for (int i = 0; i < List.Length; i++)
        {
            if (List[i] != null)
            {
                time += List[i].Duration(show);
                perc = Mathf.Clamp01(timer / time);
                List[i].SetPosition(this, perc, show);
                time += DelayBetween;
            }
        }
    }

    #endregion PUBLIC
}