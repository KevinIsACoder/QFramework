
using UnityEditor;
using UnityEngine;
using DG.DOTweenEditor;
using Spark;

namespace SparkEditor
{
    [CustomEditor(typeof(UIViewOpenAnimation))]
    public class UIViewOpenAnimationInspector: DOTweenAnimationInspector
    {
    }
    [CustomEditor(typeof(UIViewCloseAnimation))]
    public class UIViewCloseAnimationInspector: DOTweenAnimationInspector
    {
    }
    [CustomEditor(typeof(SparkTweenAnimation))]
    public class SparkTweenAnimationInspector: DOTweenAnimationInspector
    {
    }
}
