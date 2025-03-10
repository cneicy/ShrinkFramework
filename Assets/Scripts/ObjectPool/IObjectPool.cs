//------------------------------------------------------------
// Shrink Framework
// Author Eicy.
// Homepage: https://github.com/cneicy/ShrinkFramework
// Feedback: mailto:im@crash.work
//------------------------------------------------------------
using System;
using UnityEngine;

namespace ObjectPool
{
    public interface IObjectPool
    {
        Type ObjectType { get; }
        MonoBehaviour Get(Transform parent);
        void Release(MonoBehaviour obj);
    }
}