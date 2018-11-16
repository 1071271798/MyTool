using System;
using System.Collections.Generic;

public class MotorData
{
    private List<byte> motors;

    public MotorData(List<byte> list)
    {
        this.motors = list;
        this.motors.Sort();
    }

    public int Count
    {
        get
        {
            if (null == motors)
            {
                return 0;
            }
            return motors.Count;
        }
    }

    public byte Get(int i)
    {
        if (null == motors || i < 0 || i >= motors.Count)
        {
            return 0;
        }
        return motors[i];
    }

    public List<byte> GetIds()
    {
        return motors;
    }

    public bool Contains(byte id)
    {
        if (null != motors && motors.Contains(id))
        {
            return true;
        }
        return false;
    }

    public ErrorCode EqualsID(List<byte> ids)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        int selfCount = motors == null ? 0 : motors.Count;
        int argCount = ids == null ? 0 : ids.Count;
        if (selfCount != argCount)
        {
            ret = ErrorCode.Result_Motor_Num_Inconsistent;
        } else if (selfCount > 0)
        {
            for (int i = 0,imax = motors.Count; i < imax; ++i)
            {
                if (motors[i] != ids[i])
                {
                    ret = ErrorCode.Result_Motor_ID_Inconsistent;
                    break;
                }
            }
        }
        return ret;
    }

    public List<byte> GetLackID(List<byte> ids)
    {
        if (null == motors)
        {
            return null;
        }
        if (null == ids || ids.Count == 0)
        {
            return motors;
        }
        List<byte> lackIds = null;
        for (int i = 0, imax = motors.Count; i < imax; ++i)
        {
            if (!ids.Contains(motors[i]))
            {
                if (null == lackIds)
                {
                    lackIds = new List<byte>();
                }
                lackIds.Add(motors[i]);
            }
        }
        return lackIds;
    }
}

