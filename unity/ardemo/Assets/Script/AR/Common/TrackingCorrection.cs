using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 追踪位置修正
/// </summary>
public class TrackingCorrection
{
    public delegate void ChangeTransformDataCallback(Vector3 pos, Vector3 angle);

    ChangeTransformDataCallback onChangeTransformDataCallback;
    ModelTransformData mTargetTransformData;
    ModelTransformData[] mTransformDataArray;
    CorrectionData[] mCorrectionArray;
    int mDataArrayIndex;
    readonly int Data_Array_Length = 10;

    public TrackingCorrection(ChangeTransformDataCallback callback)
    {
        onChangeTransformDataCallback = callback;
        mTransformDataArray = new ModelTransformData[Data_Array_Length];
        mCorrectionArray = new CorrectionData[Data_Array_Length];
        mDataArrayIndex = 0;
    }

    public void AddTargetTransformData(Vector3 pos, Vector3 angle)
    {
        if (float.IsNaN(pos.x) || float.IsNaN(angle.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z) || float.IsNaN(angle.y) || float.IsNaN(angle.z))
        {
            return;
        }
        if (mDataArrayIndex < Data_Array_Length)
        {
            if (null == mTransformDataArray[mDataArrayIndex])
            {
                mTransformDataArray[mDataArrayIndex] = new ModelTransformData(pos, angle);
            } else
            {
                mTransformDataArray[mDataArrayIndex].SetData(pos, angle);
            }
            ++mDataArrayIndex;
            if (mDataArrayIndex == Data_Array_Length)
            {
                OnCorrection();
            }
        }
    }

    void OnCorrection()
    {
        if (Data_Array_Length > 1)
        {
            float disSum = 0;
            float angleSum = 0;
            //算出每个点与其他点的距离之和
            for (int i = 0; i < Data_Array_Length; ++i)
            {
                //MyLog.Log("transform index = " + i + " pos = " + mTransformDataArray[i].localPosition + " angle = " + mTransformDataArray[i].localEulerAngles);
                for (int compareIndex = i + 1; compareIndex < Data_Array_Length; ++compareIndex)
                {
                    if (null == mCorrectionArray[i])
                    {
                        mCorrectionArray[i] = new CorrectionData();
                    }
                    if (null == mCorrectionArray[compareIndex])
                    {
                        mCorrectionArray[compareIndex] = new CorrectionData();
                    }
                    float dis = Vector3.Distance(mTransformDataArray[i].localPosition, mTransformDataArray[compareIndex].localPosition);
                    float angle = Vector3.Distance(mTransformDataArray[i].localEulerAngles, mTransformDataArray[compareIndex].localEulerAngles);
                    mCorrectionArray[i].Add(dis, angle);
                    mCorrectionArray[compareIndex].Add(dis, angle);
                    disSum += dis;
                    angleSum += angle;
                }
            }
            disSum *= 2;
            angleSum *= 2;
            float disAve = disSum / Data_Array_Length;
            float angleAve = angleSum / Data_Array_Length;
            int count = 0;
            Vector3 localPosition = Vector3.zero;
            Vector3 eulerAnglesAve = Vector3.zero;
            for (int i = 0; i < Data_Array_Length; ++i)
            {
                if (mCorrectionArray[i].distance <= disAve/* && mCorrectionArray[i].eulerAngles <= angleAve*/)
                {//只选择位置集中的点
                    ++count;
                    localPosition += mTransformDataArray[i].localPosition;
                    if (count > 1)
                    {
                        eulerAnglesAve = new Vector3(CommonUtils.AngleAve(eulerAnglesAve.x, mTransformDataArray[i].localEulerAngles.x), CommonUtils.AngleAve(eulerAnglesAve.y, mTransformDataArray[i].localEulerAngles.y), CommonUtils.AngleAve(eulerAnglesAve.z, mTransformDataArray[i].localEulerAngles.z));
                    }
                    else
                    {
                        eulerAnglesAve = mTransformDataArray[i].localEulerAngles;
                    }
                    //MyLog.Log("select transform index = " + i + " pos = " + mTransformDataArray[i].localPosition + " angle = " + mTransformDataArray[i].localEulerAngles);
                }
            }
            if (count > 0)
            {
                if (null == mTargetTransformData)
                {
                    mTargetTransformData = new ModelTransformData(localPosition / count, eulerAnglesAve);
                }
                else
                {
                    mTargetTransformData.SetData(localPosition / count, eulerAnglesAve);
                }
                if (null != onChangeTransformDataCallback)
                {
                    onChangeTransformDataCallback(mTargetTransformData.localPosition, mTargetTransformData.localEulerAngles);
                }
            }
        } else
        {
            if (null == mTargetTransformData)
            {
                mTargetTransformData = new ModelTransformData(mTransformDataArray[0].localPosition, mTransformDataArray[0].localEulerAngles);
            }
            else
            {
                mTargetTransformData.SetData(mTransformDataArray[0].localPosition, mTransformDataArray[0].localEulerAngles);
            }
            if (null != onChangeTransformDataCallback)
            {
                onChangeTransformDataCallback(mTargetTransformData.localPosition, mTargetTransformData.localEulerAngles);
            }
        }
        mDataArrayIndex = 0;
        CleanArrayData();
    }

    void CleanArrayData()
    {
        for (int i = 0, imax = Data_Array_Length; i < imax; ++i)
        {
            if (null != mTransformDataArray[i])
            {
                mTransformDataArray[i].CleanUp();
            }
            if (null != mCorrectionArray[i])
            {
                mCorrectionArray[i].CleanUp();
            }
        }
    }



    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// 位置与角度数据
    /// </summary>
    public class ModelTransformData
    {
        public Vector3 localPosition;
        public Vector3 localEulerAngles;

        public ModelTransformData(Vector3 pos, Vector3 angles)
        {
            SetData(pos, angles);
        }

        public void SetData(Vector3 pos, Vector3 angles)
        {
            this.localPosition = new Vector3(pos.x, pos.y, pos.z);
            this.localEulerAngles = new Vector3(CommonUtils.KeepDecimal(angles.x, 2), CommonUtils.KeepDecimal(angles.y, 2), CommonUtils.KeepDecimal(angles.z, 2));
        }

        public void CleanUp()
        {
            localPosition = Vector3.zero;
            localEulerAngles = Vector3.zero;
        }
    }
    
    public class CorrectionData
    {
        public float distance = 0;
        public float eulerAngles = 0;

        public CorrectionData()
        {

        }

        public void Add(float dis, float angle)
        {
            distance += dis;
            eulerAngles += angle;
        }
        public void CleanUp()
        {
            distance = 0;
            eulerAngles = 0;
        }
    }

}

