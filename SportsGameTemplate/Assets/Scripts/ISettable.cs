using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISettable
{
    public void SetDetails<T>(T item) where T : class;
}
