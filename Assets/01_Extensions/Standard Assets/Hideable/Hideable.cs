using UnityEngine;
using UnityEngine.Events;

public class Hideable : MonoBehaviour, IHideable
{
    public enum STATES
    {
        NONE,
        SHOWING,
        SHOWN,
        HIDING,
        HIDDEN
    }

    [ReadOnly]
    public STATES State;

    public STATES StartState;
    public float CurrentTime = 0f;
    public bool ReverseDelayWhenHiding;

    public float ShowDelay;
    public float HideDelay;

    public bool ActivateAndDeactivate;

    public bool HasShowingEvent;
    public bool HasShowEvent;
    public bool HasShowInstantEvent;
    public bool HasShownEvent;
    public bool HasHideEvent;
    public bool HasHideInstantEvent;
    public bool HasHiddenEvent;
    public bool HasFinishedEvent;
    public UnityEvent OnShow;
    public UnityEvent OnShowInstant;
    public UnityEvent OnShown;
    public UnityEvent OnHide;
    public UnityEvent OnHideInstant;
    public UnityEvent OnHidden;

    public UnityEvent OnFinished;

    protected UnityAction OnFinishedCallback;

    #region INTERFACE

    public bool isVisible
    {
        get
        {
            return State == STATES.SHOWING || State == STATES.SHOWN;
        }
    }

    public void SetVisibility(bool state, bool instant = false)
    {
        if (state)
        {
            if (instant)
                ShowInstant();
            else
                Show();
        }
        else
        {
            if (instant)
                HideInstant();
            else
                Hide();
        }
    }

    public void SetFinishedListener(UnityAction callback)
    {
        OnFinishedCallback = callback;
    }

    public virtual float Duration(bool state)
    {
        return 0f;
    }

    public void SetDelay(bool state, float time)
    {
        if (state)
            ShowDelay = time;
        else
            HideDelay = time;
    }

    public virtual void SetPosition(object sender, float percentage, bool state)
    {
        if (state && percentage == 1f)
            ShowInstant();
        else if (!state && percentage == 1f)
            HideInstant();
        else if (state && percentage == 0f)
            HideInstant();
        else if (!state && percentage == 0f)
            ShowInstant();
    }

    #endregion INTERFACE

    #region PRIVATE

    private void Start()
    {
        switch (StartState)
        {
            case STATES.NONE:
                break;

            case STATES.SHOWING:
                HideInstant();
                Show();
                break;

            case STATES.SHOWN:
                ShowInstant();
                break;

            case STATES.HIDING:
                ShowInstant();
                Hide();
                break;

            case STATES.HIDDEN:
                HideInstant();
                break;

            default:
                break;
        }
    }

    protected bool IsSameState(bool state)
    {
        if (isVisible && state)
            return true;
        if (!isVisible && State != STATES.NONE && !state)
            return true;
        return false;
    }

    protected void HideFinished()
    {
        State = STATES.HIDDEN;
        if (HasHiddenEvent)
            OnShown.Send();
        if (HasFinishedEvent)
            OnFinished.Send();
    }

    protected void ShowFinished()
    {
        State = STATES.SHOWN;

        if (HasShownEvent)
            OnShown.Send();
        if (HasFinishedEvent)
            OnFinished.Send();
    }

    protected virtual void DoShow(bool instant = false)
    {
    }

    protected virtual void DoHide(bool instant = false)
    {
    }

    #endregion PRIVATE

    #region PUBLIC

    public void Show()
    {
        if (isVisible)
            return;
        State = STATES.SHOWING;
        OnShow.Send();
        DoShow();
    }

    public void ShowInstant()
    {
        if (isVisible)
            return;
        State = STATES.SHOWING;
        OnShow.Send();
        DoShow(true);
    }

    public void Hide()
    {
        if (!isVisible && State != STATES.NONE)
            return;
        State = STATES.HIDING;
        OnHide.Send();
        DoHide();
    }

    public void HideInstant()
    {
        if (!isVisible && State != STATES.NONE)
            return;
        State = STATES.HIDING;
        OnHide.Send();
        DoHide(true);
    }

    #endregion PUBLIC
}