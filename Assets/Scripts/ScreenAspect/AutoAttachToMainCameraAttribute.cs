//------------------------------------------------------------
// Shrink Framework
// Author Eicy.
// Homepage: https://github.com/cneicy/ShrinkFramework
// Feedback: mailto:im@crash.work
//------------------------------------------------------------
using System;

namespace ScreenAspect
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AutoAttachToMainCameraAttribute : Attribute 
    {
        public bool CreateIfMissing { get; set; } = true;
    }
}