using UnityEngine;
using System.Collections;

public class MyAnimtionCurve 
{
    public AnimationCurve animCurve;
    private Keyframe keyframe1;
    private Keyframe keyframe2;
    private Keyframe keyframe0;
    private Keyframe keyframe3;

    public MyAnimtionCurve(animationCurveType type, float middlex = 0.5f, float middley = 1.14f)
    {
        animCurve = new AnimationCurve();
        
        keyframe1 = new Keyframe(middlex,middley);
        if (type == animationCurveType.position)
        {
            keyframe0 = new Keyframe(0, 0);
            keyframe3 = new Keyframe(1, 1);
        }
        else if (type == animationCurveType.scale)
        {
            keyframe0 = new Keyframe(0, 0);
            keyframe3 = new Keyframe(1, 1);
        }
        animCurve.AddKey(keyframe0);
        animCurve.AddKey(keyframe1);
        animCurve.AddKey(keyframe3);
    }
    private animationCurveType _type;
    public animationCurveType showType
    {
        get
        {
            return _type;
        }
        set
        {
            _type = value;
         //   animCurve = GetCurveByType(value);
        }
    }

    #region private function
    //AnimationCurve GetCurveByType(animationCurveType type)
    //{
    //    switch (type)
    //    { 
    //        case animationCurveType.hShowOrDispearFromLeft:
    //            keyframe1.value = 1.14f;
    //            keyframe2.value = 0.95f;
    //            break;
    //        case animationCurveType.hShowOrDispearFromRight:
    //            break;
    //        case animationCurveType.vShowOrDispearFromUp:
    //            break;
    //        case animationCurveType.vShowOrDispearFromDown:
    //            break;
    //        default:
    //            break;
    //    }
    //    return null;  
    //}

    
    #endregion



    public enum animationCurveType
    { 
        position,
        scale,
        rotation,
    }
}
