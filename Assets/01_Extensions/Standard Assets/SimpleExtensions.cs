using SimpleLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SimpleExtensions
{
    public static void Send(this UnityEngine.Events.UnityEvent function)
    {
        if (function != null)
            function.Invoke();
    }

    public static void Send<T>(this UnityEngine.Events.UnityEvent<T> function, T value)
    {
        if (function != null)
            function.Invoke(value);
    }

    public static void Send<T, TT>(this UnityEngine.Events.UnityEvent<T, TT> function, T value, TT value2)
    {
        if (function != null)
            function.Invoke(value, value2);
    }

    public static void Send<T, TT, TTT>(this UnityEngine.Events.UnityEvent<T, TT, TTT> function, T value, TT value2, TTT value3)
    {
        if (function != null)
            function.Invoke(value, value2, value3);
    }

    public static bool Contains(this Animator _Anim, string _ParamName)
    {
        foreach (AnimatorControllerParameter param in _Anim.parameters)
        {
            if (param.name == _ParamName) return true;
        }
        return false;
    }

    public static string GetName<T>(this object data, System.Linq.Expressions.Expression<System.Func<T>> propertyLambda)
    {
        var me = propertyLambda.Body as System.Linq.Expressions.MemberExpression;

        if (me == null)
        {
            throw new System.ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
        }

        return me.Member.Name;
    }
}