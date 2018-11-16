using System.Collections;
using System.IO;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using Game.Platform;

public class CBaseMsg
{
    public virtual void Write(BinaryWriter bw)
    {
    }

    public virtual void Read(BinaryReader br)
    {
    }
}
/// <summary>
/// 机器是按高位低位排的，C#采用低位高位排，类型转换的时候必须用这个类转换
/// </summary>
public class MyBitConverter
{
    /// <summary>
    /// 读取一个ushort
    /// </summary>
    /// <param name="br"></param>
    /// <returns></returns>
    public static ushort ReadUInt16(BinaryReader br)
    {
        return BitConverter.ToUInt16(ReadBytes(br, 2), 0);
    }
    /// <summary>
    /// 读取一个short
    /// </summary>
    /// <param name="br"></param>
    /// <returns></returns>
    public static short ReadInt16(BinaryReader br)
    {
        ushort num = BitConverter.ToUInt16(ReadBytes(br, 2), 0);
        if (num >= 32768)
        {
            return (short)(32768 - num);
        }
        return (short)num;
    }
    /// <summary>
    /// 读取一个无符号int
    /// </summary>
    /// <param name="br"></param>
    /// <returns></returns>
    public static uint ReadUInt32(BinaryReader br)
    {
        return BitConverter.ToUInt32(ReadBytes(br, 4), 0);
    }
    /// <summary>
    /// 读取一个带符号的int
    /// </summary>
    /// <param name="br"></param>
    /// <returns></returns>
    public static int ReadInt32(BinaryReader br)
    {
        uint num = BitConverter.ToUInt32(ReadBytes(br, 4), 0);
        uint mod = 2 << 31;
        if (num >= mod)
        {
            return (int)(mod - num);
        }
        return (int)num;
    }

    /// <summary>
    /// 读取一个ulong
    /// </summary>
    /// <param name="br"></param>
    /// <returns></returns>
    public static ulong ReadUInt64(BinaryReader br)
    {
        return BitConverter.ToUInt64(ReadBytes(br, 8), 0);
    }
    /// <summary>
    /// 读取一个long
    /// </summary>
    /// <param name="br"></param>
    /// <returns></returns>
    public static long ReadInt64(BinaryReader br)
    {
        ulong num = BitConverter.ToUInt64(ReadBytes(br, 8), 0);
        ulong mod = 2 << 63;
        if (num >= mod)
        {
            return (long)(mod - num);
        }
        return (long)num;
    }

    /// <summary>
    /// 写入一位ushort
    /// </summary>
    /// <param name="bw"></param>
    /// <param name="num"></param>
    public static void WriteUInt16(BinaryWriter bw, ushort num)
    {
        byte[] bytes = BitConverter.GetBytes(num);
        bw.Write(BitReverse(bytes));
    }

    /// <summary>
    /// 写入一位short
    /// </summary>
    /// <param name="bw"></param>
    /// <param name="num"></param>
    public static void WriteInt16(BinaryWriter bw, short num)
    {
        byte[] bytes = BitConverter.GetBytes(num);
        bw.Write(BitReverse(bytes));
    }

    /// <summary>
    /// 写入一位uint
    /// </summary>
    /// <param name="bw"></param>
    /// <param name="num"></param>
    public static void WriteUInt32(BinaryWriter bw, uint num)
    {
        byte[] bytes = BitConverter.GetBytes(num);
        bw.Write(BitReverse(bytes));
    }
    /// <summary>
    /// 写入一位int
    /// </summary>
    /// <param name="bw"></param>
    /// <param name="num"></param>
    public static void WriteInt32(BinaryWriter bw, int num)
    {
        byte[] bytes = BitConverter.GetBytes(num);
        bw.Write(BitReverse(bytes));
    }
    /// <summary>
    /// 写入一位ulong
    /// </summary>
    /// <param name="bw"></param>
    /// <param name="num"></param>
    public static void WriteUInt64(BinaryWriter bw, ulong num)
    {
        byte[] bytes = BitConverter.GetBytes(num);
        bw.Write(BitReverse(bytes));
    }
    /// <summary>
    /// 写入一位long
    /// </summary>
    /// <param name="bw"></param>
    /// <param name="num"></param>
    public static void WriteInt64(BinaryWriter bw, long num)
    {
        byte[] bytes = BitConverter.GetBytes(num);
        bw.Write(BitReverse(bytes));
    }
    

    public static byte[] ConvertBytes(UInt32 num)
    {
        byte[] bytes = BitConverter.GetBytes(num);
        return BitReverse(bytes);
    }

    public static byte[] ConvertBytes(UInt16 num)
    {
        byte[] bytes = BitConverter.GetBytes(num);
        return BitReverse(bytes);
    }
    public static byte[] ConvertBytes(Int16 num)
    {
        byte[] bytes = BitConverter.GetBytes(num);
        return BitReverse(bytes);
    }
    public static byte[] ConvertBytes(Int32 num)
    {
        byte[] bytes = BitConverter.GetBytes(num);
        return BitReverse(bytes);
    }

    /// <summary>
    /// 把读取字节数逆序排列
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    private static byte[] ReadBytes(BinaryReader br, int count)
    {
        byte[] bytes = new byte[count];
        try
        {
            for (int i = count - 1; i >= 0; --i)
            {
                bytes[i] = br.ReadByte();
            }
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "BinaryReader 长度出错");
        }
        return bytes;
    }
    /// <summary>
    /// 把byte数组逆序，用于发送给机器
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] BitReverse(byte[] bytes)
    {
        if (null != bytes)
        {
            int length = bytes.Length;
            int num = length / 2;
            int mod = length - 1;
            for (int i = 0; i < num; ++i)
            {
                byte tmp = bytes[i];
                bytes[i] = bytes[mod - i];
                bytes[mod - i] = tmp;
            }
        }
        return bytes;
    }
}

/// <summary>
/// 普通消息
/// </summary>
public class CommonMsg : CBaseMsg
{
    public byte arg;
    public CommonMsg()
    {
        arg = 0;
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(arg);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }
}
/// <summary>
/// 普通数据返回
/// </summary>
public class CommonMsgAck : CBaseMsg
{
    public CommonMsgAck()
    {

    }
    public byte result;

    public override void Read(BinaryReader br)
    {
        try
        {
            result = br.ReadByte();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}

#region 握手命令
public class HandShakeMsg : CBaseMsg
{
    public byte arg;
    public HandShakeMsg()
    {
        arg = 0;
    }
    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(arg);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}
#endregion

#region 改名
public class ChangeNameMsg : CBaseMsg
{
    public string name;

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            byte[] bytes = Encoding.UTF8.GetBytes(name);
            bw.Write(bytes);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
#endregion


#region 角度回读
/// <summary>
/// 角度回读
/// </summary>
public class ReadBackMsg : CBaseMsg
{
    public ReadBackMsg()
    {
        servoList = new List<byte>();
        needReadBackCount = 0;
    }
    public List<byte> servoList;//舵机id列表
    //public byte id;//舵机id
    public byte powerDown;//是否掉电回读，00掉电，01不掉电

    public int needReadBackCount;

    public override void Write(BinaryWriter bw)
    {
        try
        {
            //bw.Write(id);
            for (int i = 0, imax = servoList.Count; i < imax; ++i)
            {
                bw.Write(servoList[i]);
                if (0 == i)
                {
                    bw.Write(powerDown);
                }
                
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}

public class ReadBackMsgAck : CBaseMsg
{
    public ReadBackMsgAck()
    {

    }
    public byte id;//舵机id
    public byte result;//错误码
    public ushort targetRota;//目标角度
    public ushort rota;//实际角度


    public override void Read(BinaryReader br)
    {
        try
        {
            id = br.ReadByte();
            result = br.ReadByte();
            if (result == (byte)0xAA)//0xAA表示成功
            {
                targetRota = MyBitConverter.ReadUInt16(br);
                rota = MyBitConverter.ReadUInt16(br);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

}
#endregion

#region 动作
/// <summary>
/// 执行动作
/// </summary>
public class CtrlActionMsg : CBaseMsg
{
    public CtrlActionMsg()
    {
        //rotas = new List<byte>();
        //ids = new List<byte>();
        dict = new Dictionary<byte, byte>();
    }
    Dictionary<byte, byte> dict;
    //public List<byte> ids;//舵机的id
    //public List<byte> rotas;//转动的角度
    public ushort sportTime;//舵机运动时间，须除以20
    public ushort endTime;//舵机完成的时间

    public override void Write(BinaryWriter bw)
    {
        try
        {
            
            UInt32 id = 0;
            List<byte> ids = new List<byte>();
            foreach (KeyValuePair<byte, byte> kvp in dict)
            {
                ids.Add(kvp.Key);
            }
            ids.Sort();
            for (int i = 0, imax = ids.Count; i < imax; ++i)
            {
                id += (UInt32)Math.Pow(2, ids[i] - 1);
            }
            MyBitConverter.WriteUInt32(bw, id);
            //bw.Write(startId);
            for (int i = 0, imax = ids.Count; i < imax; ++i)
            {
                if (dict.ContainsKey(ids[i]))
                {
                    bw.Write(dict[ids[i]]);
                }
            }
            bw.Write((byte)(sportTime / 20));
            MyBitConverter.WriteUInt16(bw, endTime);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    public void AddRota(byte id, byte rota)
    {
        dict[id] = rota;
    }

    public int GetServoCount()
    {
        return dict.Count;
    }
}

public class CtrlActionMsgAck : CBaseMsg
{
    public int len;
    public int result;
    public List<byte> servoList;

    public CtrlActionMsgAck(int len)
    {
        this.len = len;
    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            result = br.ReadByte();
            if (result != 0 && len >=5)
            {
                servoList = new List<byte>();
                UInt32 ids = MyBitConverter.ReadUInt32(br);
                for (int i = 0; i < 32; ++i)
                {
                    if ((ids & (UInt32)Math.Pow(2, i)) >= 1)
                    {
                        servoList.Add((byte)(i + 1));
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}

/// <summary>
/// 磁编码控制舵机360度转动
/// </summary>
public class CtrlServosMoveMsg : CBaseMsg
{
    public List<byte> ids;//舵机的id
    public List<ushort> rotas;//转动的角度
    public ushort sportTime;//舵机运动时间，须除以20
    public ushort endTime;//舵机完成的时间
    public CtrlServosMoveMsg()
    {
        ids = new List<byte>();
        rotas = new List<ushort>();
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {

            UInt32 id = 0;
            for (int i = 0, imax = ids.Count; i < imax; ++i)
            {
                id += (UInt32)Math.Pow(2, ids[i] - 1);
            }
            MyBitConverter.WriteUInt32(bw, id);
            //bw.Write(startId);
            if (null != rotas)
            {
                for (int i = 0, icount = rotas.Count; i < icount; ++i)
                {
                    MyBitConverter.WriteUInt16(bw, rotas[i]);
                    //bw.Write(rotas[i]);
                }
            }
            bw.Write((byte)(sportTime / 20));
            bw.Write((byte)(endTime / 20));
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }
}

/// <summary>
/// 弱锁位控制舵机360度转动
/// </summary>
public class WeakLatchesMsg : CBaseMsg
{
    public List<byte> ids;//舵机的id
    public List<ushort> rotas;//转动的角度
    public ushort sportTime;//舵机运动时间，须除以20
    public ushort endTime;//舵机完成的时间

    public WeakLatchesMsg()
    {
        ids = new List<byte>();
        rotas = new List<ushort>();
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {

            UInt32 id = 0;
            for (int i = 0, imax = ids.Count; i < imax; ++i)
            {
                id += (UInt32)Math.Pow(2, ids[i] - 1);
            }
            MyBitConverter.WriteUInt32(bw, id);
            //bw.Write(startId);
            if (null != rotas)
            {
                for (int i = 0, icount = rotas.Count; i < icount; ++i)
                {
                    MyBitConverter.WriteUInt16(bw, rotas[i]);
                    //bw.Write(rotas[i]);
                }
            }
            bw.Write((byte)(sportTime / 20));
            bw.Write((byte)(endTime / 20));
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }
}
public class PlayFlashMsg : CBaseMsg
{
    public string name;
    public PlayFlashMsg()
    {

    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            byte[] bytes = Encoding.UTF8.GetBytes(name);
            bw.Write(bytes);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
/// <summary>
/// 暂停动作
/// </summary>
public class PauseActionMsg : CBaseMsg
{
    public PauseActionMsg()
    {
        arg = 0;
    }

    public byte arg;

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(arg);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}
/// <summary>
/// 继续执行动作
/// </summary>
public class ContinueActionMsg : CBaseMsg
{
    public ContinueActionMsg()
    {
        arg = 0;
    }

    public byte arg;

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(arg);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}
/// <summary>
/// 停止动作
/// </summary>
public class StopActionMsg : CBaseMsg
{
    public StopActionMsg()
    {
        arg = 0;
    }
    public byte arg;

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(arg);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}

/// <summary>
/// 进入写外部Flash模式
/// </summary>
public class FlashStartMsg : CBaseMsg
{
    public string name;
    public ushort frameCount;

    public FlashStartMsg()
    {

    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            byte[] bytes = Encoding.UTF8.GetBytes(name);
            if (bytes.Length > 10)
            {
                name = "namemax";
                bytes = Encoding.UTF8.GetBytes(name);
            }
            byte len = (byte)bytes.Length;
            bw.Write(len);
            bw.Write(bytes);
            bw.Write(frameCount);
            //MyBitConverter.WriteUInt16(bw, frameCount);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
/// <summary>
/// 写Flash模式
/// </summary>
public class FlashWriteMsg : CBaseMsg
{
    public ushort frameNum;
    public byte[] bytes;

    public FlashWriteMsg()
    {

    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(frameNum);
            bw.Write(bytes);
            //MyBitConverter.WriteUInt16(bw, frameNum);
            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
/// <summary>
/// 取消Flash模式
/// </summary>
public class FlashStopMsg : CBaseMsg
{
    public byte arg;
    public FlashStopMsg()
    {
        arg = 0;
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(arg);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}

public class ReadAllFlashMsg : CBaseMsg
{
    public byte arg;
    public ReadAllFlashMsg()
    {
        arg = 0;
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(arg);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}

public class ReadAllFlashMsgAck : CBaseMsg
{
    public byte actionNum;
    public string name;
    public int len;
    public ReadAllFlashMsgAck(int len)
    {
        this.len = len;
    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            actionNum = br.ReadByte();
            name = Encoding.UTF8.GetString(br.ReadBytes(len - 1));
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}


#endregion

#region 连续转动
public class DjTurnMsg : CBaseMsg
{
    public List<byte> ids;
    public byte turnDirection;
    public ushort turnSpeed;

    public DjTurnMsg()
    {
        ids = new List<byte>();
        turnSpeed = 0;
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write((byte)ids.Count);
            for (int i = 0, icount = ids.Count; i < icount; ++i)
            {
                bw.Write(ids[i]);
            }
            bw.Write(turnDirection);
            MyBitConverter.WriteUInt16(bw, turnSpeed);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    public override string ToString()
    {
        ids.Sort();
        return PublicFunction.ListToString<byte>(ids);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        if (obj.GetType() != typeof(DjTurnMsg))
        {
            return false;
        }
        DjTurnMsg tmp = (DjTurnMsg)obj;
        if (ids.Count != tmp.ids.Count)
        {
            return false;
        }
        tmp.ids.Sort();
        ids.Sort();
        for (int i = 0 ,imax = ids.Count; i < imax; ++i)
        {
            if (ids[i] != tmp.ids[i])
            {
                return false;
            }
        }
        return true;
    }
}
#endregion

#region 自检
public class SelfCheckMsg : CBaseMsg
{
    public bool openFlag;
    public SelfCheckMsg()
    {

    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            if (openFlag)
            {
                bw.Write((byte)0);
            }
            else
            {
                bw.Write((byte)1);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}
/// <summary>
/// 自检普通错误回包
/// </summary>
public class SelfCheckMsgErrorAck : CBaseMsg
{
    public byte arg;

    public SelfCheckMsgErrorAck()
    {

    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            arg = br.ReadByte();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}
/// <summary>
/// 自检舵机错误回包
/// </summary>
public class SelfCheckMsgDjErrorAck : CBaseMsg
{
    public byte count;
    public List<byte> errorList;
    public SelfCheckMsgDjErrorAck(int lenght)
    {
        this.count = (byte)(lenght - 1);
        errorList = new List<byte>();
    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            for (int i = 0; i < count; ++i)
            {
                errorList.Add(br.ReadByte());
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}


public class SelfCheckDjErrorAck : CBaseMsg
{
    public List<byte> turnProtect;//堵转保护
    public List<byte> eProtect;//电流保护
    public List<byte> cProtect;//温度保护
    public List<byte> hfProtect;//过压保护
    public List<byte> lfProtect;//欠压保护
    public List<byte> otherProtect;//其他保护
    public List<byte> encryptProtect;//熔丝位或加密位错误保护


    public SelfCheckDjErrorAck()
    {
        turnProtect = new List<byte>();
        eProtect = new List<byte>();
        cProtect = new List<byte>();
        hfProtect = new List<byte>();
        lfProtect = new List<byte>();
        otherProtect = new List<byte>();
        encryptProtect = new List<byte>();
    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            UInt32 tid = MyBitConverter.ReadUInt32(br);
            UInt32 eid = MyBitConverter.ReadUInt32(br);
            UInt32 cid = MyBitConverter.ReadUInt32(br);
            UInt32 hid = MyBitConverter.ReadUInt32(br);
            UInt32 lid = MyBitConverter.ReadUInt32(br);
            UInt32 oid = MyBitConverter.ReadUInt32(br);
            UInt32 enid = MyBitConverter.ReadUInt32(br);
            for (int i = 0; i < 32; ++i)
            {
                if ((tid & (UInt32)Math.Pow(2, i)) >= 1)
                {
                    turnProtect.Add((byte)(i + 1));
                }
                if ((eid & (UInt32)Math.Pow(2, i)) >= 1)
                {
                    eProtect.Add((byte)(i + 1));
                }
                if ((cid & (UInt32)Math.Pow(2, i)) >= 1)
                {
                    cProtect.Add((byte)(i + 1));
                }
                if ((hid & (UInt32)Math.Pow(2, i)) >= 1)
                {
                    hfProtect.Add((byte)(i + 1));
                }
                if ((lid & (UInt32)Math.Pow(2, i)) >= 1)
                {
                    lfProtect.Add((byte)(i + 1));
                }
                if ((oid & (UInt32)Math.Pow(2, i)) >= 1)
                {
                    otherProtect.Add((byte)(i + 1));
                }
                if ((enid & (UInt32)Math.Pow(2, i)) >= 1)
                {
                    encryptProtect.Add((byte)(i + 1));
                }
            }

        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
#endregion

#region 读取主板信息
public class ReadMotherboardDataMsg : CBaseMsg
{
    public byte arg;//备用参数
    public ReadMotherboardDataMsg()
    {
        arg = 0;
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(arg);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}

public class ReadMotherboardDataMsgAck : CBaseMsg
{
    int len;//内容长度
    public byte power;
    public List<byte> ids;
    public List<byte> errorIds;
    public List<byte> errorVerIds;
    public string djVersion;
    public string mbVersion;
    public byte flashSize;//flash容量
    //public List<byte> infraredIds;//红外id
    //public List<byte> errorInfraredIds;//红外异常id
    Dictionary<TopologyPartType, SensorData> sensorDataDict;
    public List<byte> motors;
    public List<byte> motorErrorIds;
    public List<byte> motorErrorVerIds;
    public string motorVersion;

    public ReadMotherboardDataMsgAck(int len)
    {
        this.len = len;
        ids = new List<byte>();
        errorIds = new List<byte>();
        errorVerIds = new List<byte>();
        //infraredIds = new List<byte>();
        //errorInfraredIds = new List<byte>();
        sensorDataDict = new Dictionary<TopologyPartType, SensorData>();
    }

    public TopologyPartType GetMainboardType()
    {
        if (!string.IsNullOrEmpty(mbVersion))
        {
            if (mbVersion.StartsWith("Jimu_p"))
            {
                return TopologyPartType.MainBoard;
            } else if (mbVersion.StartsWith("Jimu_b"))
            {
                return TopologyPartType.MainBoard_new_low;
            }
        }
        return TopologyPartType.None;
    }
    /// <summary>
    /// 获取传感器数据
    /// </summary>
    /// <param name="partType"></param>
    /// <returns></returns>
    public SensorData GetSensorData(TopologyPartType partType)
    {
        if (sensorDataDict.ContainsKey(partType))
        {
            return sensorDataDict[partType];
        }
        return null;
    }
#if UNITY_EDITOR

    public void AddSensorData(TopologyPartType partType, SensorData sensorData)
    {
        sensorDataDict[partType] = sensorData;
    }
#endif

    public override void Read(BinaryReader br)
    {
        base.Read(br);
        try
        {
            mbVersion = Encoding.UTF8.GetString(br.ReadBytes(10));
            power = br.ReadByte();
            UInt32 djIds = MyBitConverter.ReadUInt32(br);
            UInt32 erIds = MyBitConverter.ReadUInt32(br);
            djVersion = PublicFunction.BytesToHexString(br.ReadBytes(4));
            UInt32 erVerIds = MyBitConverter.ReadUInt32(br);
            for (int i = 0; i < 32; ++i)
            {
                if ((djIds & (UInt32)Math.Pow(2, i)) >= 1)
                {
                    ids.Add((byte)(i + 1));
                }
                if ((erIds & (UInt32)Math.Pow(2, i)) >= 1)
                {
                    errorIds.Add((byte)(i + 1));
                }
                if ((erVerIds & (UInt32)Math.Pow(2, i)) >= 1)
                {
                    errorVerIds.Add((byte)(i + 1));
                }
            }
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, string.Format("servo id = {0}, errorIds = {1} , errorVerIds = {2} version = {3}", PublicFunction.ListToString(ids), PublicFunction.ListToString(errorIds), PublicFunction.ListToString(errorVerIds), djVersion));
            flashSize = br.ReadByte();
            len -= 28;
            TopologyPartType[] sensorTypes = PublicFunction.Open_Topology_Part_Type;
            //先读到颜色传感器
            int firstSensorMax = sensorTypes.Length >= 13 ? 13 : sensorTypes.Length;
            for (int i = 0; i < firstSensorMax; ++i)
            {
                if (len >= 7)
                {
                    SensorData data = new SensorData();
                    data.Read(br);
                    sensorDataDict[sensorTypes[i]] = data;
                    len -= 7;
                }
            }
            if (len >= 7)
            {//马达
                byte moIds = br.ReadByte();
                byte moErIds = br.ReadByte();
                motorVersion = PublicFunction.BytesToHexString(br.ReadBytes(4));
                byte moErVerIds = br.ReadByte();
                len -= 7;
                
                if (moIds > 0)
                {
                    motors = new List<byte>();
                }
                if (moErIds > 0)
                {
                    motorErrorIds = new List<byte>();
                }
                if (moErVerIds > 0)
                {
                    motorErrorVerIds = new List<byte>();
                }
                for (int i = 0; i < 8; ++i)
                {
                    if (null != motors && (moIds & (byte)Math.Pow(2, i)) >= 1)
                    {
                        motors.Add((byte)(i + 1));
                    }
                    if (null != motorErrorIds && (moErIds & (byte)Math.Pow(2, i)) >= 1)
                    {
                        motorErrorIds.Add((byte)(i + 1));
                    }
                    if (null != motorErrorVerIds && (moErVerIds & (byte)Math.Pow(2, i)) >= 1)
                    {
                        motorErrorVerIds.Add((byte)(i + 1));
                    }
                }
                PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, string.Format("motor ids = {0} errorIds = {1} errorVersionIds = {2} version = {3}", PublicFunction.ListToString(motors), PublicFunction.ListToString(motorErrorIds), PublicFunction.ListToString(motorErrorVerIds), motorVersion));
            }
            //读剩下的传感器
            if (sensorTypes.Length > firstSensorMax)
            {
                for (int i = firstSensorMax, imax = sensorTypes.Length; i < imax; ++i)
                {
                    if (len >= 7)
                    {
                        SensorData data = new SensorData();
                        data.Read(br);
                        sensorDataDict[sensorTypes[i]] = data;
                        len -= 7;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}

public class SensorData
{
    public List<byte> ids;
    public List<byte> errorIds;
    public List<byte> errorVerIds;
    public string version;

    public SensorData()
    {
        ids = new List<byte>();
        errorIds = new List<byte>();
        errorVerIds = new List<byte>();
    }

    public void Read(BinaryReader br)
    {
        try
        {
            byte tmpIds = br.ReadByte();
            byte erIds = br.ReadByte();
            StringBuilder sb = new StringBuilder();
            sb.Append(br.ReadByte().ToString().PadLeft(2, '0'));
            sb.Append(br.ReadByte().ToString().PadLeft(2, '0'));
            sb.Append(br.ReadByte().ToString().PadLeft(2, '0'));
            sb.Append(br.ReadByte().ToString().PadLeft(2, '0'));
            version = sb.ToString();
            byte errver = br.ReadByte();
            for (int i = 0; i < 8; ++i)
            {
                if ((tmpIds & (byte)Math.Pow(2, i)) >= 1)
                {
                    ids.Add((byte)(i + 1));
                }
                if ((erIds & (byte)Math.Pow(2, i)) >= 1)
                {
                    errorIds.Add((byte)(i + 1));
                }
                if ((errver & (byte)Math.Pow(2, i)) >= 1)
                {
                    errorVerIds.Add((byte)(i + 1));
                }
            }
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, ToString());
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }


    public override string ToString()
    {
        return string.Format("id = {0}, errorIds = {1} , errorVerIds = {2} version = {3}", PublicFunction.ListToString(ids), PublicFunction.ListToString(errorIds), PublicFunction.ListToString(errorVerIds), version);
    }
}
#endregion

#region 修改设备舵机id
public class ChangeDeviceIdMsg : CBaseMsg
{
    public byte startId;//起始id
    public List<byte> ids;//修改后的id列表

    public ChangeDeviceIdMsg()
    {
        ids = new List<byte>();
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(startId);
            for (int i = 0, icount = ids.Count; i < icount; ++i)
            {
                bw.Write(ids[i]);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}
#endregion

#region 循环所有动作表
public class WhileAllActionMsg : CBaseMsg
{
    public byte count;//0表示无限循环
    public byte waitTime;//单位s

    public WhileAllActionMsg()
    {
        count = 0;
        waitTime = 0;
    }


    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(count);
            bw.Write(waitTime);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
#endregion

#region 读取MCU芯片的ID号
/// <summary>
/// 读取MCU ID
/// </summary>
public class ReadMcuIdMsg : CBaseMsg
{
    public byte arg;
    public ReadMcuIdMsg()
    {
        arg = 0;
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(arg);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}

public class ReadMcuIdMsgAck : CBaseMsg
{
    public string id;

    public ReadMcuIdMsgAck()
    {
        id = string.Empty;
    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            id = PublicFunction.BytesToHexString(br.ReadBytes(12));
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}

#endregion

#region 主IC Flash中配置参数
/// <summary>
/// 写入IcFlash参数
/// </summary>
public class WriteIcFlashMsg : CBaseMsg
{
    public byte argType;
    public string arg;

    public WriteIcFlashMsg()
    {
        arg = string.Empty;
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(argType);
            byte[] bytes = Encoding.UTF8.GetBytes(arg);
            if (bytes.Length > 15)
            {
                bw.Write((byte)15);
                byte[] destAry = new byte[15];
                Array.Copy(bytes, destAry, 15);
                bw.Write(destAry);
            }
            else
            {
                bw.Write((byte)(bytes.Length));
                bw.Write(bytes);
            }
            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
/// <summary>
/// 从主IC Flash中读配置参数
/// </summary>
public class ReadIcFlashMsg : CBaseMsg
{
    public byte argType;

    public ReadIcFlashMsg()
    {

    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(argType);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
/// <summary>
/// 读取sn返回
/// </summary>
public class ReadIcFlashSnAck : CBaseMsg
{
    public byte len;
    public string deviceSn;

    public ReadIcFlashSnAck(byte len)
    {
        this.len = len;
    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            //len = br.ReadByte();
            deviceSn = Encoding.UTF8.GetString(br.ReadBytes(len));
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
#endregion

#region 主板程序升级
/// <summary>
/// 主板升级开始
/// </summary>
public class RobotUpdateStartMsg : CBaseMsg
{
    public string fileName;//文件名
    public ushort frameCount;//文件总帧数
    public RobotUpdateStartMsg()
    {

    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            byte[] bytes = Encoding.UTF8.GetBytes(fileName);
            if (null != bytes)
            {
                bw.Write((byte)bytes.Length);
                bw.Write(bytes);
                bw.Write(frameCount);
                //MyBitConverter.WriteUInt16(bw, frameCount);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}

/// <summary>
/// 主板在线升级消息
/// </summary>
public class UpdateWriteMsg : CBaseMsg
{
    public ushort frameNum;
    public byte[] bytes;
    public UpdateWriteMsg()
    {

    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(frameNum);
            bw.Write(bytes);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }
}

#endregion

#region 舵机升级
/// <summary>
/// 舵机升级开始
/// </summary>
public class ServoUpdateStartMsg : CBaseMsg
{
    public byte id;//要升级的舵机ID,ID为0，表示升级所有的舵机
    public ushort frameCount;//文件总帧数
    public UInt32 crc32;//crc校验码
    public ServoUpdateStartMsg()
    {

    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(id);
            bw.Write(frameCount);
            bw.Write(crc32);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
/// <summary>
/// 舵机升级程序写入
/// </summary>
public class ServoUpdateWriteMsg : UpdateWriteMsg
{
    public ServoUpdateWriteMsg()
    {

    }
}


/// <summary>
/// 舵机升级失败回包
/// </summary>
public class ServoUpdateFailAck : CBaseMsg
{
    public List<byte> servoList;
    public ServoUpdateFailAck()
    {
        servoList = new List<byte>();
    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            br.ReadByte();
            UInt32 ids = MyBitConverter.ReadUInt32(br);
            for (int i = 0; i < 32; ++i)
            {
                if ((ids & (UInt32)Math.Pow(2, i)) >= 1)
                {
                    servoList.Add((byte)(i + 1));
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}

#endregion

#region 传感器升级
/// <summary>
/// 开始升级传感器
/// </summary>
public class SensorUpdateStartMsg : CBaseMsg
{
   // public SensorBaseData sensorData;
    public byte sensorType;

    public byte ids;
    public ushort frameCount;//文件总帧数
    public string fileName;//文件名

    public SensorUpdateStartMsg()
    {
        //sensorData = new SensorBaseData();
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(sensorType);
            bw.Write(ids);
            bw.Write(frameCount);
            byte[] bytes = Encoding.UTF8.GetBytes(fileName);
            if (null != bytes)
            {
                bw.Write((byte)bytes.Length);
                bw.Write(bytes);
            }
            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
/// <summary>
/// 写入传感器升级文件
/// </summary>
public class SensorUpdateWriteMsg : UpdateWriteMsg
{
    public TopologyPartType sensorType;
    
    public SensorUpdateWriteMsg()
    {

    }

    public override void Write(BinaryWriter bw)
    {
        bw.Write(TopologyPartData.ConvertToSensorType(sensorType));
        base.Write(bw);
    }
}

public class SensorStopUpdateMsg : CBaseMsg
{
    public TopologyPartType sensorType;
    public byte arg = 0;
    public SensorStopUpdateMsg()
    {

    }

    public override void Write(BinaryWriter bw)
    {
        base.Write(bw);
        bw.Write(TopologyPartData.ConvertToSensorType(sensorType));
        bw.Write(arg);
    }
}


public class SensorUpdateAck : CBaseMsg
{
    public TopologyPartType sensorType;
    public byte result;

    public SensorUpdateAck()
    {

    }

    public override void Read(BinaryReader br)
    {
        base.Read(br);
        sensorType = TopologyPartData.ConvertToPartType(br.ReadByte());
        result = br.ReadByte();
    }
}

public class SensorUpdateFailAck : CBaseMsg
{
    public List<byte> sensorList;
    public SensorUpdateFailAck()
    {
        sensorList = new List<byte>();
    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            UInt32 ids = MyBitConverter.ReadUInt32(br);
            for (int i = 0; i < 32; ++i)
            {
                if ((ids & (UInt32)Math.Pow(2, i)) >= 1)
                {
                    sensorList.Add((byte)(i + 1));
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
#endregion

#region 读取系统版本号
public class ReadSystemVersionAck : CBaseMsg
{
    public byte len;
    public string version;
    public ReadSystemVersionAck(byte len)
    {
        this.len = len;
    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            version = Encoding.UTF8.GetString(br.ReadBytes(len));
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
#endregion


#region 读取电量
/// <summary>
/// 读取电量回包系，统在充电时，显示的电量和百分比都是不准确的。
/// </summary>
public class ReadPowerMsgAck : CBaseMsg
{
    public bool isAdapter;//true表示接了适配器
    public bool isChargingFinished;//true表示充电完成，false表示正在充电
    public byte power;//当前电压值需除以100
    public byte percentage;//电量百分比，需乘以100
    public static bool isLithium = true;//true表示锂电池

    public ReadPowerMsgAck()
    {
#if UNITY_EDITOR
        isAdapter = false;
        isChargingFinished = false;
        power = 78;
        percentage = 60;
#else
        isAdapter = false;
        isChargingFinished = true;
        power = 0;
        percentage = 0;
#endif
    }

    public bool IsPowerLow()
    {
        if (isLithium && !isAdapter && percentage <= 20)
        {
            return true;
        }
        if (!isLithium && percentage <= 20)
        {
            return true;
        }
        return false;
    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            isAdapter = br.ReadByte() == 1 && isLithium ? true : false;
            isChargingFinished = br.ReadByte() == 1 ? true : false;
            power = br.ReadByte();
            percentage = br.ReadByte();
            if (power <= PublicFunction.Robot_Power_Empty && ReadPowerMsgAck.isLithium)
            {
                percentage = 0;
            }
            if (percentage > 100)
            {
                percentage = 100;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
#endregion

#region 读取设备蓝牙类型回包
public class ReadDeviceTypeMsgAck : CBaseMsg
{
    public int len;
    public byte ret;
    public string name;

    public ReadDeviceTypeMsgAck(int len)
    {
        this.len = len;
    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            ret = br.ReadByte();
            name = Encoding.UTF8.GetString(br.ReadBytes(len));
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}
#endregion


#region 开启或者关闭传感器传输功能
public class SetSensorIOStateMsg : CBaseMsg
{
    public byte sensorType;
    public List<byte> ids;
    public bool openFlag;

    public SetSensorIOStateMsg()
    {
        ids = new List<byte>();
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(sensorType);
            byte id = 0;
            if (ids.Count > 1 || ids[0] != 0)
            {
                for (int i = 0, imax = ids.Count; i < imax; ++i)
                {
                    id |= (byte)Math.Pow(2, ids[i] - 1);
                }
            }
            bw.Write(id);
            if (openFlag)
            {
                bw.Write((byte)0);
            }
            else
            {
                bw.Write((byte)1);
            }

        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
#endregion

#region 获取传感器数据

public class SensorBaseData
{
    /// <summary>
    /// 传感器类型
    /// </summary>
    public byte sensorType;
    /// <summary>
    /// id
    /// </summary>
    public List<byte> ids;

    public SensorBaseData()
    {
        ids = new List<byte>();
    }

    public void Write(BinaryWriter bw)
    {
        try
        {
            bw.Write(sensorType);
            byte id = 0;
            if (ids.Count > 1 || ids.Count == 1 && ids[0] != 0)
            {
                for (int i = 0, imax = ids.Count; i < imax; ++i)
                {
                    id |= (byte)(1 << (ids[i] - 1));
                }
            }
            bw.Write(id);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public void Read(BinaryReader br)
    {
        try
        {
            sensorType = br.ReadByte();
            byte id = br.ReadByte();
            if (0 != id)
            {
                for (int i = 0; i < 8; ++i)
                {
                    if ((id & (1 << i)) > 0)
                    {
                        ids.Add((byte)(i + 1));
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
/// <summary>
/// 获取传感器数据命令
/// </summary>
public class ReadSensorDataMsg : CBaseMsg
{
    public SensorBaseData sensorData;
    /// <summary>
    /// 部分传感器需要参数，默认00
    /// </summary>
    public byte arg;

    public ReadSensorDataMsg()
    {
        sensorData = new SensorBaseData();
        arg = 0;
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            sensorData.Write(bw);
            bw.Write(arg);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}

/// <summary>
/// 获取传感器数据信息（可同时获取多条数据）
/// </summary>
public class ReadSensorDataOtherMsg : CBaseMsg
{
    public List<SensorBaseData> sensorList;

    public byte arg;//如果是陀螺仪则需要发送该参数
    public byte arg1;//保留的

    public ReadSensorDataOtherMsg()
    {
        sensorList = new List<SensorBaseData>();
        arg = 0;
        arg1 = 0;
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write((byte)sensorList.Count);
            for (int i = 0, imax = sensorList.Count; i < imax; ++i)
            {
                sensorList[i].Write(bw);
                if (sensorList[i].sensorType == TopologyPartData.ConvertToSensorType(TopologyPartType.Gyro))
                {
                    bw.Write(arg);
                    bw.Write(arg1);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}


public class ReadSensorDataOtherMsgAck : CBaseMsg
{
    public byte totalFrame;
    public byte nowFrame;
    public byte sensorNum;

    public ReadSensorDataOtherMsgAck()
    {
    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            totalFrame = br.ReadByte();
            nowFrame = br.ReadByte();
            sensorNum = br.ReadByte();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
/// <summary>
/// 读取传感器数据的头部数据
/// </summary>
public class ReadSensorDataMsgAck : CBaseMsg
{
    public byte sensorType;

    public ReadSensorDataMsgAck()
    {
    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            sensorType = br.ReadByte();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
/// <summary>
/// 读取红外数据返回
/// </summary>
public class ReadInfraredDataMsgAck : CBaseMsg
{
    public UInt16 arg;

    public ReadInfraredDataMsgAck()
    {

    }

    public override void Read(BinaryReader br)
    {
        
        try
        {
            base.Read(br);
            arg = MyBitConverter.ReadUInt16(br);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}

/// <summary>
/// 读取触碰数据返回
/// </summary>
public class ReadTouchDataMsgAck : CBaseMsg
{
    public byte arg;//0未触发，1单击，2双击，3长按

    public ReadTouchDataMsgAck()
    {

    }

    public override void Read(BinaryReader br)
    {

        try
        {
            base.Read(br);
            arg = br.ReadByte();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}

/// <summary>
/// 读取陀螺仪数据返回
/// </summary>
public class ReadGyroDataMsgAck : CBaseMsg
{
    public GyroBackData gyroData;

    public ReadGyroDataMsgAck()
    {
        gyroData = new GyroBackData();
    }

    public override void Read(BinaryReader br)
    {

        try
        {
            base.Read(br);
            gyroData.direction.z = MyBitConverter.ReadInt16(br);//航向
            gyroData.direction.y = MyBitConverter.ReadInt16(br);//俯仰
            gyroData.direction.x = MyBitConverter.ReadInt16(br);//横滚
            /*gyroData.acceleration.x = MyBitConverter.ReadUInt16(br);
            gyroData.acceleration.y = MyBitConverter.ReadUInt16(br);
            gyroData.acceleration.z = MyBitConverter.ReadUInt16(br);*/
            /*gyroData.gyro.x = MyBitConverter.ReadUInt16(br);
            gyroData.gyro.y = MyBitConverter.ReadUInt16(br);
            gyroData.gyro.z = MyBitConverter.ReadUInt16(br);*/
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}


public class ReadSpeakerDataMsgAck : CBaseMsg
{
    public SpeakerInfoData data;
    int len;

    public ReadSpeakerDataMsgAck(int len)
    {
        data = new SpeakerInfoData();
        this.len = len;
    }

    public override void Read(BinaryReader br)
    {

        try
        {
            base.Read(br);
            data.speakerMac = PublicFunction.BytesToHexString(br.ReadBytes(6));
            data.speakerName = Encoding.ASCII.GetString(br.ReadBytes(len - 6));
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}

public class ReadColorDataMsgAck : CBaseMsg
{
    public ColorSensorAckData colorData;

    public ReadColorDataMsgAck()
    {
        colorData = new ColorSensorAckData();
    }

    public override void Read(BinaryReader br)
    {

        try
        {
            base.Read(br);
            colorData.color_r = br.ReadByte();
            colorData.color_g = br.ReadByte();
            colorData.color_b = br.ReadByte();
            colorData.AD1_H = br.ReadByte();
            colorData.AD1_L = br.ReadByte();
            colorData.AD2_H = br.ReadByte();
            colorData.AD2_L = br.ReadByte();
            colorData.AD3_H = br.ReadByte();
            colorData.AD3_L = br.ReadByte();
            colorData.AD4_H = br.ReadByte();
            colorData.AD4_L = br.ReadByte();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
#endregion


#region 发送数据到传感器
/// <summary>
/// 发送数据到传感器基类
/// </summary>
public class SendSensorDataMsg : CBaseMsg
{
    public SensorBaseData sensorData;

    public SendSensorDataMsg()
    {
        sensorData = new SensorBaseData();
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            sensorData.Write(bw);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}


public class SendSensorDataMsgAck : CBaseMsg
{
    public SensorBaseData sensorData;
    public byte result;

    public SendSensorDataMsgAck()
    {
        sensorData = new SensorBaseData();
    }

    public override void Read(BinaryReader br)
    {
        
        try
        {
            base.Read(br);
            sensorData.Read(br);
            result = br.ReadByte();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}

/// <summary>
/// 发送数码管数据
/// </summary>
public class SendDigitalTubeDataMsg : SendSensorDataMsg
{
    public byte controlType = 0;
    /// <summary>
    /// 需显示的数字位数，1表示个位，2表示十分位，3表示百分位，4表示千分位
    /// </summary>
    public List<byte> showNum;
    /// <summary>
    /// 需要显示的点的位数，1表示个位，2表示十分位，3表示百分位，4表示千分位
    /// </summary>
    public List<byte> showSubPoint;
    /// <summary>
    /// 是否显示冒号
    /// </summary>
    public bool showColon;
    /// <summary>
    /// 是否是负数
    /// </summary>
    public bool isNegativeNum;
    /// <summary>
    /// 闪烁的次数
    /// </summary>
    public byte flickerTimes;
    /// <summary>
    /// 闪烁或数值变化的频率
    /// </summary>
    public UInt32 flickerTimeout;
    /// <summary>
    /// 起始值
    /// </summary>
    public UInt32 startValue;
    /// <summary>
    /// 结束值
    /// </summary>
    public UInt32 endValue;

    public SendDigitalTubeDataMsg()
    {
        showNum = new List<byte>();
        showSubPoint = new List<byte>();
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(controlType);
            UInt16 controlWay = 0;
            for (int i = 0, imax = showNum.Count; i < imax; ++i)
            {
                controlWay |= (UInt16)(1 << (showNum[i] - 1));
            }
            for (int i = 0, imax = showSubPoint.Count; i < imax; ++i)
            {
                controlWay |= (UInt16)(1 << (3 + showSubPoint[i]));
            }
            if (showColon)
            {//显示冒号
                controlWay |= (UInt16)(1 << 8);
            }
            if (isNegativeNum)
            {//负数
                controlWay |= (UInt16)(1 << 9);
            }
            MyBitConverter.WriteUInt16(bw, controlWay);
            bw.Write(flickerTimes);
            MyBitConverter.WriteUInt32(bw, flickerTimeout);
            MyBitConverter.WriteUInt32(bw, startValue);
            MyBitConverter.WriteUInt32(bw, endValue);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}

/// <summary>
/// 发送灯光表情数据
/// </summary>
public class SendEmojiDataMsg : SendSensorDataMsg
{
    public byte lightType;
    //public UInt16 duration;//单次表情持续时间，ms
    public UInt16 times;//0xFFFF不断重复
    public string rgb;//#FFFFFF|0XFFFFFF

    public SendEmojiDataMsg()
    {

    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(lightType);
            //bw.Write((byte)(duration / 100));
            MyBitConverter.WriteUInt16(bw, times);
            //bw.Write(times);
            byte[] rgbAry = PublicFunction.HexStringToRGB(rgb);
            bw.Write(rgbAry[0]);
            bw.Write(rgbAry[1]);
            bw.Write(rgbAry[2]);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}

/// <summary>
/// 发送灯光帧数据
/// </summary>
public class SendLightDataMsg : SendSensorDataMsg
{
    public UInt32 duration;//单次表情持续时间，s
    public List<LightShowData> showData;
    public bool newVersion;

    public SendLightDataMsg()
    {
        showData = new List<LightShowData>();
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            if (duration >= 255000)
            {
                bw.Write((byte)0xff);
            }
            else
            {
                if (newVersion)
                {
                    if (duration >= 25500)
                    {
                        bw.Write((byte)0xff);
                    } else
                    {
                        bw.Write((byte)PublicFunction.Rounding(duration / 100.0f));
                    }
                }
                else
                {
                    bw.Write((byte)PublicFunction.Rounding(duration / 1000.0f));
                }
            }
            bw.Write((byte)showData.Count);
            for (int i = 0, imax = showData.Count; i < imax; ++i)
            {
                showData[i].Write(bw);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
}

public class LightShowData
{
    public List<byte> ids;
    public string rgb;

    public LightShowData()
    {
        ids = new List<byte>();
    }

    public void Write(BinaryWriter bw)
    {
        try
        {
            byte id = 0;
            if (ids.Count == 1 && ids[0] == 0)
            {
                id = 0xFF;
            }
            else
            {
                for (int i = 0, imax = ids.Count; i < imax; ++i)
                {
                    id |= (byte)(1 << (ids[i] - 1));
                }
            }
            bw.Write(id);
            byte[] tmp = PublicFunction.HexStringToRGB(rgb);
            bw.Write(tmp[0]);
            bw.Write(tmp[1]);
            bw.Write(tmp[2]);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}


public class SendUltrasonicDataMsg : SendSensorDataMsg
{
    public string rgb;
    public byte mode;
    public byte speed;
    public UInt32 time;

    public SendUltrasonicDataMsg()
    {

    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            byte[] tmp = PublicFunction.HexStringToRGB(rgb);
            bw.Write(tmp[0]);
            bw.Write(tmp[1]);
            bw.Write(tmp[2]);
            bw.Write(mode);
            bw.Write(speed);
            if (time >= 6553500)
            {
                bw.Write((UInt16)0xffff);
            }
            else
            {
                MyBitConverter.WriteUInt16(bw, (UInt16)PublicFunction.Rounding(time / 100.0f));
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }
}
#endregion


#region 控制传感器灯光

/// <summary>
/// 控制传感器led灯的显示
/// </summary>
public class CtrlSensorLEDMsg : CBaseMsg
{
    public enum ControlType : byte
    {
        Always_On = 0,//常亮
        Always_Off,//熄灭
        Single_Flash,//单闪
        Double_Flash,//双闪
    }
    public SensorBaseData sensorData;
    public ControlType controlType;
    public UInt16 duration;
    public byte times;//0不断重复

    public CtrlSensorLEDMsg()
    {
        sensorData = new SensorBaseData();
        controlType = ControlType.Single_Flash;
        duration = 300;
        times = 3;
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            sensorData.Write(bw);
            bw.Write((byte)controlType);
            bw.Write((byte)(duration / 100));
            bw.Write(times);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
#endregion


#region 修改传感器ID
public class ChangeSensorIDMsg : CBaseMsg
{
    /// <summary>
    /// 传感器类型
    /// </summary>
    public byte sensorType;
    /// <summary>
    /// 原id
    /// </summary>
    public byte id;
    /// <summary>
    /// 目标id
    /// </summary>
    public byte targetId;

    public ChangeSensorIDMsg()
    {

    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(sensorType);
            bw.Write(id);
            bw.Write(targetId);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }


}



public class ChangeSensorIDMsgAck : CBaseMsg
{
    /// <summary>
    /// 传感器类型
    /// </summary>
    public byte sensorType;
    /// <summary>
    /// id
    /// </summary>
    public byte id;
    /// <summary>
    /// 结果
    /// </summary>
    public byte result;

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            sensorType = br.ReadByte();
            id = br.ReadByte();
            result = br.ReadByte();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
#endregion


#region 读取传感器ID

/// <summary>
/// 读取传感器id发送消息
/// </summary>
public class ReadSensorIDMsg : CBaseMsg
{
    public SensorBaseData sensorData;
    public byte arg;

    public ReadSensorIDMsg()
    {
        sensorData = new SensorBaseData();
        arg = 0;
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            sensorData.Write(bw);
            bw.Write(arg);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}

/// <summary>
/// 读取传感器ID返回
/// </summary>
public class ReadSensorIDMsgAck : CBaseMsg
{
    public SensorBaseData sensorData;
    public byte result;

    public ReadSensorIDMsgAck()
    {
        sensorData = new SensorBaseData();
    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            sensorData.Read(br);
            result = br.ReadByte();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
#endregion


#region 设置蓝牙通讯超时机制

/// <summary>
/// 设置蓝牙通讯超时机制
/// </summary>
public class SetBLEOutTimeMsg : CBaseMsg
{
    public bool openFlag;

    public SetBLEOutTimeMsg()
    {
        openFlag = true;
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            if (openFlag)
            {
                bw.Write((byte)0);
            }
            else
            {
                bw.Write((byte)1);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
#endregion


#region 自动修复舵机异常命令

/// <summary>
/// 修复舵机异常
/// </summary>
public class RepairServoExceptionMsg : CBaseMsg
{
    public byte arg;

    public RepairServoExceptionMsg()
    {
        arg = 0;
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write(arg);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
#endregion


#region 马达命令

public class SendMotorData
{
    public byte id;
    public TurnDirection direction;
    public short speed;
    public ushort time;

    public TurnDirection lastDirection;
    public short lastSpeed;

    public SendMotorData()
    {
        lastDirection = TurnDirection.turnStop;
        lastSpeed = 0;
        direction = TurnDirection.turnStop;
        speed = 0;
    }

    public SendMotorData(SendMotorData data)
    {
        this.id = data.id;
        this.direction = data.direction;
        this.speed = data.speed;
        this.time = data.time;
        this.lastDirection = data.lastDirection;
        this.lastSpeed = data.lastSpeed;
    }

    public override string ToString()
    {
        return "[id : " + id + " direction : " + direction + " speed : " + speed + " time : " + time + " lastDirection : " + lastDirection + " lastSpeed : " + lastSpeed + "]";
    }
}

public class CtrlMotorMsg : CBaseMsg
{
    public List<SendMotorData> motorList;

    public CtrlMotorMsg()
    {

    }

    public override string ToString()
    {
        if (null != motorList)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0, imax = motorList.Count; i < imax; ++i)
            {
                sb.Append(motorList[i].ToString());
                sb.Append("\n");
            }
            return sb.ToString();
        }
        return base.ToString();
    }


    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write((byte)0x01);
            if (null != motorList)
            {
                motorList.Sort(delegate (SendMotorData a, SendMotorData b) {
                    if (a == null && b == null)
                    {
                        return 0;
                    }
                    if (a == null && b != null)
                    {
                        return -1;
                    }
                    if (b == null && a != null)
                    {
                        return 1;
                    }
                    if (a.id > b.id)
                    {
                        return 1;
                    }
                    else if (a.id < b.id)
                    {
                        return -1;
                    }
                    return 0;
                });
                byte id = 0;
                for (int i = 0, imax = motorList.Count; i < imax; ++i)
                {
                    id |= (byte)(1 << (motorList[i].id - 1));
                }
                bw.Write(id);
                for (int i = motorList.Count - 1; i >= 0; --i)
                {
                    if (motorList[i].direction == TurnDirection.turnByDisclock)
                    {
                        MyBitConverter.WriteInt16(bw, (short)(-motorList[i].speed));
                    }
                    else
                    {
                        MyBitConverter.WriteInt16(bw, motorList[i].speed);
                    }
                    MyBitConverter.WriteUInt16(bw, motorList[i].time);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public bool ContainsID(List<byte> ids)
    {
        if (null == motorList || null == ids)
        {
            return true;
        }
        if (motorList.Count < ids.Count)
        {
            return false;
        }
        byte thisId = 0;
        byte argId = 0;
        for (int i = 0, imax = motorList.Count; i < imax; ++i)
        {
            thisId |= (byte)(1 << (motorList[i].id - 1));
            if (i < ids.Count)
            {
                argId |= (byte)(1 << (ids[i] - 1));
            }
        }
        if (thisId == argId)
        {
            return true;
        }
        byte tmpId = (byte)(thisId ^ argId);
        //PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "thisId = " + thisId + " argId = " + argId + " tmpId =" + tmpId + " tmpId | thisId = " + (tmpId | thisId));
        if (tmpId != 0 && (tmpId | thisId) == thisId)
        {
            return true;
        }
        return false;
    }

    public bool ContainsID(CtrlMotorMsg msg)
    {
        if (null == motorList || null == msg.motorList)
        {
            return true;
        }
        if (motorList.Count < msg.motorList.Count)
        {
            return false;
        }
        byte thisId = 0;
        byte argId = 0;
        for (int i = 0, imax = motorList.Count; i < imax; ++i)
        {
            thisId |= (byte)(1 << (motorList[i].id - 1));
            if (i < msg.motorList.Count)
            {
                argId |= (byte)(1 << (msg.motorList[i].id - 1));
            }
        }
        if (thisId == argId)
        {
            return true;
        }
        byte tmpId = (byte)(thisId ^ argId);
        //PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "thisId = " + thisId + " argId = " + argId + " tmpId =" + tmpId + " tmpId | thisId = " + (tmpId | thisId));
        if (tmpId != 0 && (tmpId | thisId) == thisId)
        {
            return true;
        }
        return false;
    }

    public bool EqualsID(List<byte> ids)
    {
        if (null == motorList || null == ids)
        {
            return true;
        }
        if (motorList.Count != ids.Count)
        {
            return false;
        }
        byte thisId = 0;
        byte argId = 0;
        for (int i = 0, imax = motorList.Count; i < imax; ++i)
        {
            thisId |= (byte)(1 << (motorList[i].id - 1));
            argId |= (byte)(1 << (ids[i] - 1));
        }
        if (thisId != argId)
        {
            return false;
        }
        return true;
    }

    public bool EqualsID(CtrlMotorMsg msg)
    {
        if (null == motorList || null == msg.motorList)
        {
            return true;
        }
        if (motorList.Count != msg.motorList.Count)
        {
            return false;
        }
        byte thisId = 0;
        byte argId = 0;
        for (int i = 0, imax = motorList.Count; i < imax; ++i)
        {
            thisId |= (byte)(1 << (motorList[i].id - 1));
            argId |= (byte)(1 << (msg.motorList[i].id - 1));
        }
        if (thisId != argId)
        {
            return false;
        }
        return true;
    }
}


public class CtrlMotorMsgAck : CBaseMsg
{
    public int len;
    public int result;
    public int num;
    public List<byte> ids;
    public CtrlMotorMsgAck(int len)
    {
        this.len = len;
    }

    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            result = br.ReadByte();
            if (result != 0 && len >= 2)
            {
                num = br.ReadByte();
                if (num == 1)
                {
                    byte id = br.ReadByte();
                    if (0 != id)
                    {
                        ids = new List<byte>();
                        for (int i = 0; i < 8; ++i)
                        {
                            if ((id & (1 << i)) > 0)
                            {
                                ids.Add((byte)(i + 1));
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}


public class StopMotorMsg : CBaseMsg
{
    public List<byte> ids;

    public StopMotorMsg()
    {

    }

    public override string ToString()
    {
        return PublicFunction.ListToString(ids);
    }


    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write((byte)0x01);
            byte id = 0;
            if (null != ids)
            {
                for (int i = 0, imax = ids.Count; i < imax; ++i)
                {
                    id |= (byte)(1 << (ids[i] - 1));
                }
            }
            bw.Write(id);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public bool EqualsID(List<byte> ids)
    {
        if (null == this.ids || null == ids)
        {
            return true;
        }
        if (this.ids.Count != ids.Count)
        {
            return false;
        }
        byte thisId = 0;
        byte argId = 0;
        for (int i = 0, imax = ids.Count; i < imax; ++i)
        {
            thisId |= (byte)(1 << (this.ids[i] - 1));
            argId |= (byte)(1 << (ids[i] - 1));
        }
        if (thisId != argId)
        {
            return false;
        }
        return true;
    }

    public bool ContainsID(List<byte> ids)
    {
        if (null == this.ids || null == ids)
        {
            return true;
        }
        if (this.ids.Count < ids.Count)
        {
            return false;
        }
        byte thisId = 0;
        byte argId = 0;
        for (int i = 0, imax = this.ids.Count; i < imax; ++i)
        {
            thisId |= (byte)(1 << (this.ids[i] - 1));
            if (i < ids.Count)
            {
                argId |= (byte)(1 << (ids[i] - 1));
            }
        }
        if (thisId == argId)
        {
            return true;
        }
        byte tmpId = (byte)(thisId ^ argId);
        //PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "thisId = " + thisId + " argId = " + argId + " tmpId =" + tmpId + " tmpId | thisId = " + (tmpId | thisId));
        if (tmpId != 0 && (tmpId | thisId) == thisId)
        {
            return true;
        }
        return false;
    }

    public bool ContainsID(CtrlMotorMsg msg)
    {
        if (null == this.ids || null == msg.motorList)
        {
            return true;
        }
        if (this.ids.Count < msg.motorList.Count)
        {
            return false;
        }
        byte thisId = 0;
        byte argId = 0;
        for (int i = 0, imax = this.ids.Count; i < imax; ++i)
        {
            thisId |= (byte)(1 << (this.ids[i] - 1));
            if (i < msg.motorList.Count)
            {
                argId |= (byte)(1 << (msg.motorList[i].id - 1));
            }
        }
        if (thisId == argId)
        {
            return true;
        }
        byte tmpId = (byte)(thisId ^ argId);
        //PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "thisId = " + thisId + " argId = " + argId + " tmpId =" + tmpId + " tmpId | thisId = " + (tmpId | thisId));
        if (tmpId != 0 && (tmpId | thisId) == thisId)
        {
            return true;
        }
        return false;
    }
}
#endregion

#region RGB灯珠显示

public enum RgbLightControlType : byte
{
    Show_Color = 0,
    Change_Color,
    Show_Emoji,
}

public class SendRgbLightDataMsg : SendSensorDataMsg
{
    public RgbLightControlType controlType;
    public UInt16 time;//单位1ms,0x0000 - 不显示，0xFFFF - 一直显示
    public UInt16 flash; //单位 Hz，0x0000 - 不闪烁
    public string rgb;
    public string targetRgb;
    public UInt16 emojiIndex;//0000-睡眠，0001-害羞...

    public SendRgbLightDataMsg() : base()
    {
        sensorData.sensorType = TopologyPartData.ConvertToSensorType(TopologyPartType.RgbLight);
    }

    public override void Write(BinaryWriter bw)
    {
        try
        {
            base.Write(bw);
            bw.Write((byte)controlType);
            switch (controlType)
            {
                case RgbLightControlType.Show_Color:
                case RgbLightControlType.Change_Color:
                    {
                        MyBitConverter.WriteUInt16(bw, time);
                        MyBitConverter.WriteUInt16(bw, flash);
                        byte[] tmp = PublicFunction.HexStringToRGB(rgb);
                        bw.Write(tmp[0]);
                        bw.Write(tmp[1]);
                        bw.Write(tmp[2]);
                        if (controlType == RgbLightControlType.Change_Color)
                        {
                            byte[] tmp1 = PublicFunction.HexStringToRGB(targetRgb);
                            bw.Write(tmp1[0]);
                            bw.Write(tmp1[1]);
                            bw.Write(tmp1[2]);
                        }
                    }
                    break;
                case RgbLightControlType.Show_Emoji:
                    {
                        MyBitConverter.WriteUInt16(bw, emojiIndex);
                        MyBitConverter.WriteUInt16(bw, time);
                        byte[] tmp = PublicFunction.HexStringToRGB(rgb);
                        bw.Write(tmp[0]);
                        bw.Write(tmp[1]);
                        bw.Write(tmp[2]);
                    }
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}


public class SendRgbLightDataMsgAck : CBaseMsg
{
    public SensorBaseData sensorData;
    public RgbLightControlType controlType;
    public byte result;

    public SendRgbLightDataMsgAck()
    {
        sensorData = new SensorBaseData();
    }
    public override void Read(BinaryReader br)
    {
        try
        {
            base.Read(br);
            sensorData.Read(br);
            controlType = (RgbLightControlType)br.ReadByte();
            result = br.ReadByte();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
}
#endregion
