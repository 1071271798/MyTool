using Game.Platform;
using System;
using Game;
using System.Collections.Generic;
using UnityEngine;

namespace MyTest
{
    public class PlatformTestManager : SingletonObject<PlatformTestManager>
    {
#if UNITY_EDITOR
        private long mStartScanIndex = 0;
        private List<string> mScanList = null;
        private int mScanListIndex = 0;

        public PlatformTestManager()
        {
            PlayerPrefs.SetString(PublicFunction.Hardware_Version_Name + TopologyPartType.MainBoard.ToString(), "Jimu_p1.66");
            PlayerPrefs.SetString(PublicFunction.Hardware_Version_Path + TopologyPartType.MainBoard.ToString(), "E:/主板舵机文件/Jimu2primary_p1.66.bin");
            PlayerPrefs.SetString(PublicFunction.Hardware_Version_Name + TopologyPartType.MainBoard_new_low.ToString(), "Jimu_b0.15");
            PlayerPrefs.SetString(PublicFunction.Hardware_Version_Path + TopologyPartType.MainBoard_new_low.ToString(), "E:/主板舵机文件/jimu_app_b0.15.bin");
            PlayerPrefs.SetString(PublicFunction.Hardware_Version_Name + TopologyPartType.Servo.ToString(), "41165101");
            PlayerPrefs.SetString(PublicFunction.Hardware_Version_Path + TopologyPartType.Servo.ToString(), "E:/主板舵机文件/jimu2_app_41165101.bin");
            PlayerPrefs.SetString(PublicFunction.Hardware_Version_Name + TopologyPartType.Infrared.ToString(), "14161215");
            PlayerPrefs.SetString(PublicFunction.Hardware_Version_Path + TopologyPartType.Infrared.ToString(), "E:/主板舵机文件/Jimu2_infrared_sensor_14161215.bin");
        }

        public void StartScan()
        {
            StopScan();
            mScanListIndex = 0;
            int waitTime = UnityEngine.Random.Range(0, 5);
            mStartScanIndex = Timer.Add(waitTime, 1, 6, StartScanTest);
        }

        public void StopScan()
        {
            if (-1 != mStartScanIndex)
            {
                Timer.Cancel(mStartScanIndex);
                mStartScanIndex = -1;
            }
        }

        public void ConnenctBluetooth(string mac)
        {
            if (string.Equals(mac, "test"))
            {
                NetWaitMsg.ShowWait(5);
                PlatformMgr.Instance.ConnenctCallBack(mac);
            } else
            {
                Timer.Add(6, 1, 1, delegate ()
                {
                    int result = UnityEngine.Random.Range(1, 10);
                    if (result > 7)
                    {
                        PlatformMgr.Instance.ConnenctCallBack(string.Empty);
                    }
                    else
                    {
                        PlatformMgr.Instance.ConnenctCallBack(mac);
                    }
                });
            }
        }

        public void DisConnenctBuletooth()
        {

        }

        public void CallPlatformFunc(string funcName, string arg)
        {

        }

        public string GetPlatformData(string dataType)
        {
            return string.Empty;
        }

        public string GetData(string dataType, string jsonString)
        {
            return string.Empty;
        }


        //////////////////////////////////////////////////////////////////////////

        void StartScanTest()
        {
            string info = null;
            if (null != mScanList && mScanList.Count >= 6 && mScanListIndex < mScanList.Count)
            {
                info = mScanList[mScanListIndex];
                ++mScanListIndex;
            }
            else
            {
                int rssi = UnityEngine.Random.Range(0, 128);
                if (rssi % 3 == 0)
                {
                    info = "JIMU\n" + Guid.NewGuid() + "\n" + (-rssi);
                }
                else if (rssi % 3 == 1)
                {
                    string name = "My_Jimu_" + UnityEngine.Random.Range(1000, 9999);
                    info = name + "\n" + Guid.NewGuid() + "\n" + (-rssi);
                }
                else
                {
                    string name = "Jimu_" + UnityEngine.Random.Range(1000, 9999);
                    info = name + "\n" + Guid.NewGuid() + "\n" + (-rssi);
                }
                if (null == mScanList)
                {
                    mScanList = new List<string>();
                }
                mScanList.Add(info);
            }
            PlatformMgr.Instance.OnFoundDevice(info);
        }

#endif
    }
}

