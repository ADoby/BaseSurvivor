using strange.extensions.command.api;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.dispatcher.eventdispatcher.api;
using strange.extensions.dispatcher.eventdispatcher.impl;
using strange.extensions.implicitBind.api;
using strange.extensions.implicitBind.impl;
using strange.extensions.injector.api;
using strange.extensions.injector.impl;
using strange.extensions.mediation.api;
using strange.extensions.mediation.impl;
using strange.extensions.sequencer.api;
using strange.extensions.sequencer.impl;
using strange.extensions.signal.impl;
using strange.framework.api;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleIoC
{
    public class SimpleContext : MVCSContext
    {
        public static MonoBehaviour Behaviour;

        public static SimpleContext Context;

        public SimpleContext(MonoBehaviour view)
        {
            Context = this;
            var flags = ContextStartupFlags.AUTOMATIC;

            //If firstContext was unloaded, the contextView will be null. Assign the new context as firstContext.
            if (firstContext == null || firstContext.GetContextView() == null)
            {
                firstContext = this;
            }
            else
            {
                firstContext.AddContext(this);
            }

            SetContextView(view);
            addCoreComponents();
            this.autoStartup = (flags & ContextStartupFlags.MANUAL_LAUNCH) != ContextStartupFlags.MANUAL_LAUNCH;
            if ((flags & ContextStartupFlags.MANUAL_MAPPING) != ContextStartupFlags.MANUAL_MAPPING)
            {
                Start();
            }
        }

        private void Awake()
        {
            BubbleBind();
        }

        public void BubbleBind()
        {
            var sceenRoots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            SimpleView[] behaviours;
            for (int i = 0; i < sceenRoots.Length; i++)
            {
                if (sceenRoots[i] == null)
                    continue;
                behaviours = sceenRoots[i].GetComponentsInChildren<SimpleView>(true);
                for (int j = 0; j < behaviours.Length; j++)
                {
                    if (behaviours[j] != null)
                    {
                        behaviours[j].Bubble();
                    }
                }
            }
        }

        protected override void addCoreComponents()
        {
            base.addCoreComponents();

            injectionBinder.Unbind<ICommandBinder>();
            injectionBinder.Bind<ICommandBinder>().To<SignalCommandBinder>().ToSingleton();
            injectionBinder.Unbind<IMediationBinder>();
            injectionBinder.Bind<IMediationBinder>().To<SimpleBehaviourBinder>().ToSingleton();
        }

        public virtual void SendSignal<T>() where T : Signal
        {
            var signal = injectionBinder.GetInstance<T>();
            signal.Dispatch();
        }

        /// <summary>
        /// Override "BindContext" instead
        /// </summary>
        protected override void mapBindings()
        {
            base.mapBindings();
            BindContext();
            AfterBinding();
        }

        protected virtual void BindContext()
        {
        }

        protected virtual void AfterBinding()
        {
        }

        //private List<string> registeredTypes = new List<string>();
        public virtual IMediationBinding BindBehaviour<T>()
        {
            return mediationBinder.Bind<T>();
        }

        public virtual IMediationBinding BindBehaviour<T>(T behaviour)
        {
            return BindBehaviour<T>();
        }

        public virtual IMediationBinding BindMediator<View, Mediator>()
        {
            IMediationBinding binding = null;
            try
            {
                binding = mediationBinder.GetBinding<View>() as IMediationBinding;
            }
            catch (InjectionException)
            {
                //Workaround
                //Couldn't find Binding, strange throws error. why? I don't know
                //No "ContainsBinding" method yet
            }

            if (binding != null)
                return binding;

            return mediationBinder.Bind<View>().To<Mediator>();
        }

        /// <summary>
        /// Working multy layer StrangeIOC binding
        /// </summary>
        /// <typeparam name="Signal"></typeparam>
        /// <typeparam name="Command"></typeparam>
        /// <param name="once"></param>
        /// <returns></returns>
        public virtual ICommandBinding BindCommand<Signal, Command>(bool once = false, bool replace = false)
        {
            ICommandBinding binding = null;
            try
            {
                binding = commandBinder.GetBinding<Signal>();
            }
            catch (InjectionException)
            {
                //Workaround
                //Couldn't find Binding, strange throws error. why? I don't know
                //No "ContainsBinding" method yet
            }

            if (binding != null && replace)
            {
                commandBinder.Unbind(injectionBinder.GetInstance<Signal>());
                binding = null;
            }

            if (binding == null)
            {
                binding = commandBinder.Bind<Signal>().To<Command>();
            }
            else
            {
                binding = binding.To<Command>();
            }
            if (once)
                binding = binding.Once();
            return binding;
        }

        public virtual ICommandBinding BindCommand<Command>(object key, bool once = false)
        {
            ICommandBinding binding = commandBinder.Bind(key).To<Command>();
            if (once)
                binding = binding.Once();
            return binding;
        }

        public virtual IInjectionBinding Bind<Type>(bool singleton = false, bool crosscontext = false)
        {
            IInjectionBinding binding = injectionBinder.Bind<Type>();
            if (singleton)
                binding = binding.ToSingleton();
            if (crosscontext)
                binding = binding.CrossContext();
            return binding;
        }
    }
}