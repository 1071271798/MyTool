using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using Game;
using Game.Platform;

namespace MyTest
{
    public class MsgTestManager : SingletonObject<MsgTestManager>
    {
#if UNITY_EDITOR
        public delegate byte[] CreateReceiveByte(SendMsgData sendMsg);

        private Dictionary<CMDCode, CreateReceiveByte> mMsgDict;

        private int mReadBackCount = 0;
        private bool robotNeedUpdate = false;
        private bool servoNeedUpdate = false;
        private bool sensorNeedUpdate = false;
        public MsgTestManager()
        {
            mMsgDict = new Dictionary<CMDCode, CreateReceiveByte>();
            mMsgDict[CMDCode.Send_RgbLight_Data] = GetSendRgbLight;
            mMsgDict[CMDCode.Read_Sensor_Data] = GetSensorData;
            mMsgDict[CMDCode.Read_Device_Type] = GetReadDevice;
            mMsgDict[CMDCode.Read_Motherboard_Data] = GetMotherboardData;
            mMsgDict[CMDCode.Self_Check] = GetSelfCheck;
            mMsgDict[CMDCode.Ctrl_Action] = GetCtrlAction;
            mMsgDict[CMDCode.Read_Back] = GetReadBack;
            mMsgDict[CMDCode.Robot_Update_Start] = GetCommonMsgAck;
            mMsgDict[CMDCode.Robot_Update_Write] = GetCommonMsgAck;
            mMsgDict[CMDCode.Robot_Update_Finish] = GetCommonMsgAck;
            mMsgDict[CMDCode.Robot_Update_Stop] = GetCommonMsgAck;
            mMsgDict[CMDCode.Servo_Update_Start] = GetCommonMsgAck;
            mMsgDict[CMDCode.Servo_Update_Write] = GetCommonMsgAck;
            mMsgDict[CMDCode.Servo_Update_Finish] = GetCommonMsgAck;
            mMsgDict[CMDCode.Servo_Update_Stop] = GetCommonMsgAck;
            mMsgDict[CMDCode.Sensor_Update_Start] = GetSensorUpdate;
            mMsgDict[CMDCode.Sensor_Update_Write] = GetSensorUpdate;
            mMsgDict[CMDCode.Sensor_Update_Finish] = GetSensorUpdate;
            mMsgDict[CMDCode.Read_Sensor_Data_Other] = GetQueryAllSensorData;
            mMsgDict[CMDCode.Set_Sensor_IO_State] = GetSensorInitAck;
            mMsgDict[CMDCode.Change_Sensor_ID] = GetChangeSensorAck;
            mMsgDict[CMDCode.Change_ID] = GetCommonMsgAck;
            mMsgDict[CMDCode.Read_System_Power] = GetPowerMsgAck;
            mMsgDict[CMDCode.Ctrl_Motor] = GetCommonMsgAck;
            mMsgDict[CMDCode.Stop_Motor] = GetCommonMsgAck;
        }

        public void ReceiveMsg(SendMsgData sendMsg)
        {
            if (mMsgDict.ContainsKey(sendMsg.cmd))
            {
                byte[] bytes = mMsgDict[sendMsg.cmd](sendMsg);
                ReceiveMsg(sendMsg.cmd, sendMsg.mac, bytes);
            }
        }

        private void ReceiveMsg(CMDCode cmd, string mac, byte[] bytes)
        {
            MemoryStream readStream = new MemoryStream(bytes);
            BinaryReader br = new BinaryReader(readStream, Encoding.ASCII);
            int len = br.ReadByte();
            byte[] param = new byte[bytes.Length - 1];
            Array.Copy(bytes, 1, param, 0, param.Length);
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "mac=" + mac + ";len=" + len + ";cmd=" + cmd.ToString() + " param = " + PublicFunction.BytesToHexString(param));
            NetWork.GetInst().ReceiveMsg(cmd, len, mac, br);
            readStream.Flush();
            readStream.Close();
            br.Close();
        }

        private void NotifyMsg(CMDCode cmd, string mac, byte[] param)
        {
            int len = param.Length;
            byte[] bytes = new byte[len + 1];
            bytes[0] = (byte)len;
            Array.Copy(param, 0, bytes, 1, param.Length);
            ReceiveMsg(cmd, mac, bytes);
        }


        byte[] GetSpeakerData(SendMsgData sendMsg)
        {
            byte[] mac = PublicFunction.HexStringToBytes("FFFFFF");
            byte[] name = Encoding.ASCII.GetBytes("Jimuspk_FFFF");
            return ConvertBytes(sendMsg.cmd, TopologyPartData.ConvertToSensorType(TopologyPartType.Speaker), mac, name);
        }

        byte[] GetSensorData(SendMsgData sendMsg)
        {
            ReadSensorDataMsg msg = (ReadSensorDataMsg)sendMsg.msg;
            if (msg.sensorData.sensorType == TopologyPartData.ConvertToSensorType(TopologyPartType.Speaker))
            {
                return GetSpeakerData(sendMsg);
            }
            return null;
        }
        /// <summary>
        /// rgb灯结果返回
        /// </summary>
        /// <param name="sendMsg"></param>
        /// <returns></returns>
        byte[] GetSendRgbLight(SendMsgData sendMsg)
        {
            byte[] bytes = new byte[3];
            SendRgbLightDataMsg msg = (SendRgbLightDataMsg)sendMsg.msg;
            bytes[0] = PublicFunction.ListToByte(msg.sensorData.ids);
            bytes[1] = (byte)msg.controlType;
            bytes[2] = 0;
            return ConvertBytes(sendMsg.cmd, bytes); ;
        }
        /// <summary>
        /// 读取设备类型
        /// </summary>
        /// <param name="sendMsg"></param>
        /// <returns></returns>
        byte[] GetReadDevice(SendMsgData sendMsg)
        {
            byte[] bytes = Encoding.ASCII.GetBytes("JIMU");
            return ConvertBytes(sendMsg.cmd, (byte)0, bytes);
        }
        /// <summary>
        /// 读取主板信息
        /// </summary>
        /// <param name="sendMsg"></param>
        /// <returns></returns>
        byte[] GetMotherboardData(SendMsgData sendMsg)
        {
            byte[] mbVersion;
            byte power = 0;
            //新主控还是老主控
            if (UnityEngine.Random.Range(0, 2) % 2 == 0)
            {
                if (robotNeedUpdate)
                {
                    mbVersion = Encoding.ASCII.GetBytes("Jimu_p1.00");
                } else
                {
                    mbVersion = Encoding.ASCII.GetBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.MainBoard));
                }
                power = PublicFunction.Robot_Power_Max;//(byte)UnityEngine.Random.Range(PublicFunction.Robot_Power_Empty, PublicFunction.Robot_Power_Max + 1);
            }
            else
            {
                if (robotNeedUpdate)
                {
                    mbVersion = Encoding.ASCII.GetBytes("Jimu_b1.00");
                } else
                {
                    mbVersion = Encoding.ASCII.GetBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.MainBoard_new_low));
                }
                if (UnityEngine.Random.Range(0, 2) % 2 == 0)
                {//干电池
                    power = 128 + 60;//(byte)UnityEngine.Random.Range(PublicFunction.Robot_Power_Dry_Low, 61);
                }
                else
                {//锂电池
                    power = PublicFunction.Robot_Power_Max;//(byte)UnityEngine.Random.Range(PublicFunction.Robot_New_Power_Empty, PublicFunction.Robot_Power_Max + 1);
                }
            }
            if (null == mbVersion || mbVersion.Length < 10)
            {
                mbVersion = new byte[10];
            }
            Robot robot = SingletonObject<ConnectCtrl>.GetInst().GetConnectRobot();
            //舵机
            byte[] djId = null;
            if (RobotManager.GetInst().IsSetDeviceIDFlag || RobotManager.GetInst().IsCreateRobotFlag)
            {
                djId = MyBitConverter.ConvertBytes(PublicFunction.ConverToUInt32(new byte[] { 1 }));
            } else
            {
                djId = MyBitConverter.ConvertBytes(PublicFunction.ListToUInt32(robot.GetAllDjData().GetIDList()));
            }
            byte[] erIds = MyBitConverter.ConvertBytes(PublicFunction.ConverToUInt32(new byte[] { 0 }));
            byte[] djVersion;
            if (servoNeedUpdate)
            {
                djVersion = PublicFunction.HexStringToBytes("41155201");
            } else
            {
                djVersion = string.IsNullOrEmpty(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.Servo)) ? new byte[] { 0, 0, 0, 0 } : PublicFunction.HexStringToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.Servo));
            }
            byte[] erVerIds = MyBitConverter.ConvertBytes(PublicFunction.ConverToUInt32(new byte[] { 0 }));
            byte flashSize = 4;
            //红外
            byte inid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte inerid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte[] inVersion;
            if (sensorNeedUpdate)
            {
                inVersion = PublicFunction.HexStringToBytes("01161215");
            }
            else
            {
                inVersion = PublicFunction.SensorVersionToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.Infrared));
            }
            byte inerVer = PublicFunction.ConverToByte(new byte[] { 0 });
            //陀螺仪
            byte grid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte grerid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte[] grVersion = PublicFunction.SensorVersionToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.Gyro));
            byte grerVer = PublicFunction.ConverToByte(new byte[] { 0 });
            //触碰
            byte touid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte touerid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte[] touVersion = PublicFunction.SensorVersionToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.Touch));
            byte touerVer = PublicFunction.ConverToByte(new byte[] { 0 });
            //灯光
            byte lgid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte lgerid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte[] lgVersion = PublicFunction.SensorVersionToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.Light));
            byte lgerVer = PublicFunction.ConverToByte(new byte[] { 0 });
            //重力
            byte gtid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte gterid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte[] gtVersion = PublicFunction.SensorVersionToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.Gravity));
            byte gterVer = PublicFunction.ConverToByte(new byte[] { 0 });
            //超声
            byte ulid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte ulerid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte[] ulVersion = PublicFunction.SensorVersionToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.Ultrasonic));
            byte ulerVer = PublicFunction.ConverToByte(new byte[] { 0 });
            //数码管
            byte dtid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte dterid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte[] dtVersion = PublicFunction.SensorVersionToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.DigitalTube));
            byte dterVer = PublicFunction.ConverToByte(new byte[] { 0 });
            //蓝牙音箱
            byte spid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte sperid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte[] spVersion = PublicFunction.SensorVersionToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.Speaker));
            byte sperVer = PublicFunction.ConverToByte(new byte[] { 0 });
            //光感
            byte enid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte enerid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte[] enVersion = PublicFunction.SensorVersionToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.EnLight));
            byte enerVer = PublicFunction.ConverToByte(new byte[] { 0 });
            //大气压
            byte atid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte aterid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte[] atVersion = PublicFunction.SensorVersionToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.Atmosphere));
            byte aterVer = PublicFunction.ConverToByte(new byte[] { 0 });
            //声音
            byte sdid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte sderid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte[] sdVersion = PublicFunction.SensorVersionToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.Sound));
            byte sderVer = PublicFunction.ConverToByte(new byte[] { 0 });
            //温湿度
            byte tpid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte tpterid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte[] tpVersion = PublicFunction.SensorVersionToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.Temperature));
            byte tperVer = PublicFunction.ConverToByte(new byte[] { 0 });
            //颜色
            byte clid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte clerid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte[] clVersion = PublicFunction.SensorVersionToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.Color));
            byte clerVer = PublicFunction.ConverToByte(new byte[] { 0 });
            //马达
            byte mtid = 0;
            if (null != robot.MotorsData)
            {
                mtid = PublicFunction.ListToByte(robot.MotorsData.GetIds());
            }
            byte mterid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte[] mtVersion = PublicFunction.SensorVersionToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.Motor));
            byte mterVer = PublicFunction.ConverToByte(new byte[] { 0 });
            //独角兽灯
            byte rgbid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte rgberid = PublicFunction.ConverToByte(new byte[] { 0 });
            byte[] rgbVersion = PublicFunction.SensorVersionToBytes(SingletonObject<UpdateManager>.GetInst().GetHardwareVersion(TopologyPartType.RgbLight));
            byte rgberVer = PublicFunction.ConverToByte(new byte[] { 0 });

            return ConvertBytes(sendMsg.cmd, mbVersion, power, djId, erIds, djVersion, erVerIds, flashSize, inid, inerid, inVersion, inerVer,
                grid, grerid, grVersion, grerVer, touid, touerid, touVersion, touerVer, lgid, lgerid, lgVersion, lgerVer, gtid, gterid, gtVersion, gterVer,
                ulid, ulerid, ulVersion, ulerVer, dtid, dterid, dtVersion, dterVer, spid, sperid, spVersion, sperVer, enid, enerid, enVersion, enerVer,
                atid, aterid, atVersion, aterVer, sdid, sderid, sdVersion, sderVer, tpid, tpterid, tpVersion, tperVer, clid, clerid, clVersion, clerVer,
                mtid, mterid, mtVersion, mterVer, rgbid, rgberid, rgbVersion, rgberVer);
        }

        /// <summary>
        /// 自检
        /// </summary>
        /// <param name="sendMsg"></param>
        /// <returns></returns>
        byte[] GetSelfCheck(SendMsgData sendMsg)
        {
            return ConvertBytes(sendMsg.cmd, (byte)0);
        }

        byte[] GetCtrlAction(SendMsgData sendMsg)
        {
            byte result = 0;
            byte[] errids = MyBitConverter.ConvertBytes(PublicFunction.ConverToUInt32(new byte[] { 0 }));
            return ConvertBytes(sendMsg.cmd, result, errids);
        }

        byte[] GetReadBack(SendMsgData sendMsg)
        {
            ReadBackMsg msg = (ReadBackMsg)sendMsg.msg;
            if (mReadBackCount == 0)
            {
                mReadBackCount = msg.needReadBackCount;
                for (int i = 0, imax = mReadBackCount - 1; i < imax; ++i)
                {
                    ReceiveMsg(sendMsg);
                }
            }
            Robot robot = SingletonObject<ConnectCtrl>.GetInst().GetConnectRobot();
            List<byte> ids = msg.servoList;
            if (ids.Count == 1 && ids[0] == 0)
            {
                ids = robot.GetAllDjData().GetIDList();
            }
            byte id = ids[mReadBackCount - 1];
            byte result = (byte)0xAA;
            byte[] targetRota = MyBitConverter.ConvertBytes((UInt16)UnityEngine.Random.Range(1, 241));
            byte[] rota = targetRota;
            mReadBackCount--;
            return ConvertBytes(sendMsg.cmd, id, result, targetRota, rota);
        }

        /// <summary>
        /// 通用回调，就返回成功的参数
        /// </summary>
        /// <param name="sendMsg"></param>
        /// <returns></returns>
        byte[] GetCommonMsgAck(SendMsgData sendMsg)
        {
            switch (sendMsg.cmd)
            {
                case CMDCode.Robot_Update_Finish:
                    {
                        Robot robot = SingletonObject<ConnectCtrl>.GetInst().GetConnectRobot();
                        if (null != robot && robot.MotherboardData != null && robot.MotherboardData.GetMainboardType() == TopologyPartType.MainBoard)
                        {
                            Timer.Add(2, 1, 1, delegate ()
                            {
                                NotifyMsg(CMDCode.Robot_Restart_Update_Start_Ack, sendMsg.mac, new byte[] { 0 });
                            });
                            Timer.Add(10, 1, 1, delegate ()
                            {
                                robotNeedUpdate = false;
                                NotifyMsg(CMDCode.Robot_Restart_Update_Finish_Ack, sendMsg.mac, new byte[] { 0 });
                            });
                        } else
                        {
                            robotNeedUpdate = false;
                        }
                    }
                    break;
                case CMDCode.Servo_Update_Finish:
                    {
                        Timer.Add(10, 1, 1, delegate ()
                        {
                            //升级成功
                            servoNeedUpdate = false;
                            NotifyMsg(CMDCode.Servo_Update_Finish, sendMsg.mac, new byte[] {(byte)0xAA });
                            //升级失败
                            //NotifyMsg(CMDCode.Servo_Update_Finish, sendMsg.mac, new byte[] { 1, 0, 0, 0, 1 });
                        });
                    }
                    break;
            }
            return ConvertBytes(sendMsg.cmd, (byte)0);
        }

        byte[] GetPowerMsgAck(SendMsgData sendMsg)
        {
            byte isAdapter = 1;
            byte isChargingFinished = 0;
            if (UnityEngine.Random.Range(1, 10) >= 3)
            {
                isAdapter = 0;
            } else
            {
                if (UnityEngine.Random.Range(1, 10) >= 5)
                {
                    isChargingFinished = 1;
                }
            }
            byte power = (byte)UnityEngine.Random.Range(PublicFunction.Robot_New_Power_Empty, PublicFunction.Robot_Power_Max + 1);
            byte percentage = (byte)UnityEngine.Random.Range(0, 101);
            return ConvertBytes(sendMsg.cmd, isAdapter, isChargingFinished, power, percentage);
        }

        byte[] GetSensorUpdate(SendMsgData sendMsg)
        {
            byte sensorType = 0;
            if (sendMsg.cmd == CMDCode.Sensor_Update_Start)
            {
                SensorUpdateStartMsg msg = (SensorUpdateStartMsg)sendMsg.msg;
                sensorType = msg.sensorType;
            } else
            {
                SensorUpdateWriteMsg msg = (SensorUpdateWriteMsg)sendMsg.msg;
                sensorType = TopologyPartData.ConvertToSensorType(msg.sensorType);
                if (sendMsg.cmd == CMDCode.Sensor_Update_Finish)
                {
                    Timer.Add(10, 1, 1, delegate () {
                        //升级成功
                        sensorNeedUpdate = false;
                        NotifyMsg(CMDCode.Sensor_Update_Finish, sendMsg.mac, new byte[] {sensorType, (byte)0xAA });
                        //升级失败
                        //NotifyMsg(CMDCode.Sensor_Update_Finish, sendMsg.mac, new byte[] {sensorType, 1, 0, 0, 0, 1 });
                    });
                }
            }
            return ConvertBytes(sendMsg.cmd, new byte[] { sensorType, 0 });
        }

        byte[] GetQueryAllSensorData(SendMsgData sendMsg)
        {
            TopologyPartType[] allSensor = PublicFunction.Read_All_Sensor_Type;
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            byte sensorCount = 0;
            Robot robot = RobotManager.GetInst().GetCurrentRobot();
            if (null != robot && null != robot.MotherboardData)
            {
                for (int i = 0, imax = allSensor.Length; i < imax; ++i)
                {
                    SensorData sensorData = robot.MotherboardData.GetSensorData(allSensor[i]);
                    if (null != sensorData && sensorData.ids.Count > 0)
                    {
                        switch (allSensor[i])
                        {
                            case TopologyPartType.Infrared:
                                {
                                    bw.Write(TopologyPartData.ConvertToSensorType(TopologyPartType.Infrared));
                                    bw.Write(PublicFunction.ConverToByte(new byte[] { 0 }));
                                    bw.Write(PublicFunction.ListToByte(sensorData.ids));
                                    for (int sensorIndex = 0, count = sensorData.ids.Count; sensorIndex < count; ++sensorIndex)
                                    {
                                        MyBitConverter.WriteUInt16(bw, (ushort)UnityEngine.Random.Range(1, 2000));
                                    }
                                    ++sensorCount;
                                }
                                break;
                            case TopologyPartType.Touch:
                                {
                                    bw.Write(TopologyPartData.ConvertToSensorType(TopologyPartType.Touch));
                                    bw.Write(PublicFunction.ConverToByte(new byte[] { 0 }));
                                    bw.Write(PublicFunction.ListToByte(sensorData.ids));
                                    for (int sensorIndex = 0, count = sensorData.ids.Count; sensorIndex < count; ++sensorIndex)
                                    {
                                        MyBitConverter.WriteUInt16(bw, (byte)UnityEngine.Random.Range(0, 4));
                                    }
                                    ++sensorCount;
                                }
                                break;
                            case TopologyPartType.Gyro:
                                {
                                    bw.Write(TopologyPartData.ConvertToSensorType(TopologyPartType.Gyro));
                                    bw.Write(PublicFunction.ConverToByte(new byte[] { 0 }));
                                    bw.Write(PublicFunction.ListToByte(sensorData.ids));
                                    for (int sensorIndex = 0, count = sensorData.ids.Count; sensorIndex < count; ++sensorIndex)
                                    {
                                        MyBitConverter.WriteInt16(bw, (short)UnityEngine.Random.Range(-179, 180));
                                        MyBitConverter.WriteInt16(bw, (short)UnityEngine.Random.Range(-179, 180));
                                        MyBitConverter.WriteInt16(bw, (short)UnityEngine.Random.Range(-179, 180));
                                    }
                                    ++sensorCount;
                                }
                                break;
                            case TopologyPartType.Ultrasonic:
                                {
                                    bw.Write(TopologyPartData.ConvertToSensorType(TopologyPartType.Ultrasonic));
                                    bw.Write(PublicFunction.ConverToByte(new byte[] { 0 }));
                                    bw.Write(PublicFunction.ListToByte(sensorData.ids));
                                    for (int sensorIndex = 0, count = sensorData.ids.Count; sensorIndex < count; ++sensorIndex)
                                    {
                                        MyBitConverter.WriteUInt16(bw, (ushort)UnityEngine.Random.Range(1, 400));
                                    }
                                    ++sensorCount;
                                }
                                break;
                            case TopologyPartType.Color:
                                {
                                    bw.Write(TopologyPartData.ConvertToSensorType(TopologyPartType.Color));
                                    bw.Write(PublicFunction.ConverToByte(new byte[] { 0 }));
                                    bw.Write(PublicFunction.ListToByte(sensorData.ids));
                                    for (int sensorIndex = 0, count = sensorData.ids.Count; sensorIndex < count; ++sensorIndex)
                                    {
                                        bw.Write((byte)UnityEngine.Random.Range(0, 256));
                                        bw.Write((byte)UnityEngine.Random.Range(0, 256));
                                        bw.Write((byte)UnityEngine.Random.Range(0, 256));
                                        bw.Write((byte)UnityEngine.Random.Range(0, 256));
                                        bw.Write((byte)UnityEngine.Random.Range(0, 256));
                                        bw.Write((byte)UnityEngine.Random.Range(0, 256));
                                        bw.Write((byte)UnityEngine.Random.Range(0, 256));
                                        bw.Write((byte)UnityEngine.Random.Range(0, 256));
                                        bw.Write((byte)UnityEngine.Random.Range(0, 256));
                                        bw.Write((byte)UnityEngine.Random.Range(0, 256));
                                        bw.Write((byte)UnityEngine.Random.Range(0, 256));
                                    }
                                    ++sensorCount;
                                }
                                break;
                        }
                    }
                }
            }
            byte[] bytes = ms.ToArray();
            bw.Close();
            ms.Flush();
            ms.Close();
            return ConvertBytes(sendMsg.cmd, new byte[]{ 1, 1, sensorCount }, bytes);
        }

        byte[] GetSensorInitAck(SendMsgData sendMsg)
        {
            SetSensorIOStateMsg msg = (SetSensorIOStateMsg)sendMsg.msg;
            return ConvertBytes(sendMsg.cmd, msg.sensorType, PublicFunction.ListToByte(msg.ids), (byte)0);
        }

        byte[] GetChangeSensorAck(SendMsgData sendMsg)
        {
            ChangeSensorIDMsg msg = (ChangeSensorIDMsg)sendMsg.msg;
            return ConvertBytes(sendMsg.cmd, msg.sensorType, msg.targetId, (byte)0);
        }

        byte[] ConvertBytes(CMDCode cmd, params object[] param)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            byte len = 0;
            for (int i = 0, imax = param.Length; i < imax; ++i)
            {
                if (param[i] is byte)
                {
                    len += (byte)1;
                } else
                {
                    byte[] tmp = (byte[])param[i];
                    len += (byte)tmp.Length;
                }
            }
            bw.Write((byte)len);
            for (int i = 0, imax = param.Length; i < imax; ++i)
            {
                if (param[i] is byte)
                {
                    byte tmp = (byte)param[i];
                    bw.Write(tmp);
                }
                else
                {
                    byte[] tmp = (byte[])param[i];
                    bw.Write(tmp);
                }
            }
            byte[] bytes = ms.ToArray();
            bw.Flush();
            bw.Close();
            ms.Close();
            return bytes;
        }
#endif
    }
}



