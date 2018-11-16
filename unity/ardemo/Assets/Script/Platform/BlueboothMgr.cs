using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Event;

namespace Game.Platform
{
    public class DeviceInfo
    {
        private string m_name = string.Empty;
        public string Name { get { return this.m_name; } set { this.m_name = value; } }

        private string m_mac = string.Empty;
        public string Mac { get { return this.m_mac; } }

        public int RSSI = 0;//信号
        public DeviceInfo(string str)
        {
            Parse(str);
        }

        //解析蓝牙设备信息，后17位为mac地址，前面的都是名称
        void Parse(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            string[] ary = str.Split('\n');
            if (ary.Length > 0)
            {
                m_name = ary[0];
            }
            if (ary.Length > 1)
            {
                m_mac = ary[1];
            }
            if (ary.Length > 2)
            {
                if (PublicFunction.IsInteger(ary[2]))
                {
                    RSSI = int.Parse(ary[2]);
                }
                else
                {
                    RSSI = -127;
                }
                
            }
            /*if(str.Length>17)
            {
                m_name = str.Substring(0, str.Length - 17);
                m_mac = str.Substring(str.Length - 17, 17);
            }*/
        }
    }

    class BluetoothMgr
    {
        private List<DeviceInfo> m_newDevice = new List<DeviceInfo>();                                //新搜寻到的蓝牙设备
        public List<DeviceInfo> NewDevice { get { return m_newDevice; } }


        public BluetoothMgr()
        {
            
        }

        //当发现新设备
        public void NewFound(string name)
        {
            DeviceInfo device = new DeviceInfo(name);
            if (Check(device))
            {
                m_newDevice.Add(device);
                if (SingletonObject<ConnectCtrl>.GetInst().IsReconnectFlag)
                {
                    SingletonObject<ConnectCtrl>.GetInst().FoundDevice(device);
                } else
                {
                    EventMgr.Inst.Fire(EventID.BLUETOOTH_ON_DEVICE_FOUND, new EventArg(device));
                }
            }
        }


        //剔除掉无效的蓝牙名称
        public bool Check(DeviceInfo device)
        {
            if (device.Name.Equals("null"))
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "发现的设备 name=" + device.Name + " mac=" + device.Mac);
                return false;
            }
            for (int i = 0; i < m_newDevice.Count; ++i)
            {
                if (m_newDevice[i].Mac == device.Mac || device.Name.StartsWith("My_Jimu_") && m_newDevice[i].Name == device.Name)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 通过mac地址获取设备名字
        /// </summary>
        /// <param name="mac"></param>
        /// <returns></returns>
        public string GetNameForMac(string mac)
        {
            for (int i = 0, icount = m_newDevice.Count; i < icount; ++i)
            {
                if (m_newDevice[i].Mac.Equals(mac))
                {
                    return m_newDevice[i].Name;
                }
            }
            return string.Empty;
        }

        public void BlueRename(string name, string mac)
        {
            for (int i = 0, icount = m_newDevice.Count; i < icount; ++i)
            {
                if (m_newDevice[i].Mac.Equals(mac))
                {
                    m_newDevice[i].Name = name;
                }
            }
        }

        public void ClearDevice()
        {
            m_newDevice.Clear();
        }
    }
}
