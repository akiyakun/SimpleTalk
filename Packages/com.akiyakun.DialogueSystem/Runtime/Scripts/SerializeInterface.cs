using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class SerializeInterface : PropertyAttribute
{
    private bool includeMono;

    public SerializeInterface(bool includeMono = false)
    {
        this.includeMono = includeMono;
    }

    public bool IsIncludeMono()
    {
        return includeMono;
    }
}
