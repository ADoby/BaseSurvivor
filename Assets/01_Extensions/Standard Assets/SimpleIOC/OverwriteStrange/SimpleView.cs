using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.mediation.api;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using System;
using System.Collections.Generic;
using UnityEngine;

using SimpleIoC;

public class SimpleView : MonoBehaviour, IView, IMediator
{
    #region Mediator

    [Inject(ContextKeys.CONTEXT_VIEW)]
    public GameObject contextView { get; set; }

    [SerializeField]
    [ReadOnly]
    protected bool HasRegistered = false;

    public SimpleView()
    {
    }

    private SimpleContext mainContext;

    public virtual void BindToContext(SimpleContext context)
    {
        mainContext = context;
        Bind(this);
    }

    protected void Bind<T>(T view)
    {
        if (mainContext != null)
            mainContext.BindBehaviour(view);
    }

    /**
     * Fires directly after creation and before injection
     */

    virtual public void PreRegister()
    {
    }

    /**
     * Fires after all injections satisifed.
     *
     * Override and place your initialization code here.
     */

    virtual public void OnRegister()
    {
        HasRegistered = true;
    }

    /**
     * Fires on removal of view.
     *
     * Override and place your cleanup code here
     */

    virtual public void OnRemove()
    {
        HasRegistered = false;
    }

    #endregion Mediator

    #region View

    /// Leave this value true most of the time. If for some reason you want
    /// a view to exist outside a context you can set it to false. The only
    /// difference is whether an error gets generated.
    private bool _requiresContext = true;

    public bool requiresContext
    {
        get
        {
            return _requiresContext;
        }
        set
        {
            _requiresContext = value;
        }
    }

    /// A flag for allowing the View to register with the Context
    /// In general you can ignore this. But some developers have asked for a way of disabling
    ///  View registration with a checkbox from Unity, so here it is.
    /// If you want to expose this capability either
    /// (1) uncomment the commented-out line immediately below, or
    /// (2) subclass View and override the autoRegisterWithContext method using your own custom (public) field.
    //[SerializeField]
    protected bool registerWithContext = true;

    virtual public bool autoRegisterWithContext
    {
        get { return registerWithContext; }
        set { registerWithContext = value; }
    }

    [SerializeField]
    [ReadOnly]
    protected bool _registeredWithContext;

    public bool registeredWithContext
    {
        get
        {
            return _registeredWithContext;
        }
        set
        {
            _registeredWithContext = value;
        }
    }

    /// A MonoBehaviour Awake handler.
    /// The View will attempt to connect to the Context at this moment.
    protected virtual void Awake()
    {
        if (autoRegisterWithContext)
            bubbleToContext(this, true, false);
    }

    /// A MonoBehaviour Start handler
    /// If the View is not yet registered with the Context, it will
    /// attempt to connect again at this moment.
    protected virtual void Start()
    {
        if (autoRegisterWithContext)
            bubbleToContext(this, true, true);
    }

    /// A MonoBehaviour OnDestroy handler
    /// The View will inform the Context that it is about to be
    /// destroyed.
    protected virtual void OnDestroy()
    {
        if (mainContext != null)
        {
            mainContext.RemoveView(this);
        }
        else
        {
            bubbleToContext(this, false, false);
        }
    }

    public void DestroyGameObject()
    {
        OnDestroy();
        Destroy(gameObject);
    }

    public void Bubble()
    {
        bubbleToContext(this, true, true);
    }

    public void BubbleFast(SimpleContext context = null)
    {
        if (context == null)
            context = SimpleContext.Context;
        if (SimpleContext.Context == null)
        {
            Debug.LogWarning("No Context", this);
            return;
        }

        BindToContext(SimpleContext.Context);
        SimpleContext.Context.AddView(this);
        //registeredWithContext = true;
    }

    /// Recurses through Transform.parent to find the GameObject to which ContextView is attached
    /// Has a loop limit of 100 levels.
    /// By default, raises an Exception if no Context is found.
    virtual protected void bubbleToContext(MonoBehaviour view, bool toAdd, bool finalTry)
    {
        if (toAdd && registeredWithContext)
            return;

        const int LOOP_MAX = 100;
        int loopLimiter = 0;
        Transform trans = view.gameObject.transform;
        while (trans.parent != null && loopLimiter < LOOP_MAX)
        {
            loopLimiter++;
            trans = trans.parent;
            if (trans.gameObject.GetComponent<ContextView>() != null)
            {
                ContextView contextView = trans.gameObject.GetComponent<ContextView>() as ContextView;
                if (contextView.context != null)
                {
                    BindToContext((SimpleContext)contextView.context);

                    IContext context = contextView.context;
                    if (toAdd)
                    {
                        context.AddView(view);
                        //registeredWithContext = true;
                    }
                    else
                    {
                        context.RemoveView(view);
                    }
                    return;
                }
            }
        }
        if (requiresContext && finalTry)
        {
            //last ditch. If there's a Context anywhere, we'll use it!
            if (Context.firstContext != null)
            {
                BindToContext(SimpleContext.Context);
                Context.firstContext.AddView(view);
                //registeredWithContext = true;
                return;
            }

            string msg = (loopLimiter == LOOP_MAX) ?
                msg = "A view couldn't find a context. Loop limit reached." :
                    msg = "A view was added with no context. Views must be added into the hierarchy of their ContextView lest all hell break loose.";
            msg += "\nView: " + view.ToString();
            throw new MediationException(msg,
                MediationExceptionType.NO_CONTEXT);
        }
    }

    //TODO IMediator.OnEnabled interface vollständig implementieren
    public void OnEnabled()
    {
        // throw new NotImplementedException();
    }

    //TODO IMediator.OnEnabled interface vollständig implementieren
    public void OnDisabled()
    {
        //throw new NotImplementedException();
    }

    #endregion View
}