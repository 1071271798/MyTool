using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:ProtocolPacket.cs
/// Description:
/// Time:2015/12/3 17:19:24
/// </summary>
public class ProtocolPacket
{
    enum PROTOCOL_STATE
    {
        HEADER1,
        HEADER2,
        LENGHT,
        //SESSION_ID,
        //INDEX,
        CMD,
        PARAM,
        CHECKSUM,
        END
    }
    //字头（2B）
    private byte[] mHeader;
    // 长度（1B）
    private byte mLen;
    // SessionID（1B）
    //private byte mID;
    // 包序号(1B)
    //private byte mIndex;
    // 命令(1B)
    private byte mCmd;
    // check sum (1B)
    private byte mCheckSum;
    // end(1B)
    private byte mEnd;
    // param(nB)
    private byte[] mParam;

    // 命令参数长度
    private int mParamLen;

    private PROTOCOL_STATE mState;
    private List<byte> mBufferDecode;


    /**
     * 构造函数
     */
    public ProtocolPacket()
    {
        mState = PROTOCOL_STATE.HEADER1;
        mBufferDecode = new List<byte>();
    }

    /**
     * 获取解码之后的数据
     */
    public byte[] getRawData()
    {
        int nBytes = mBufferDecode.Count;
        byte[] rawData = new byte[nBytes];

        //rawData = mBuffer.array().clone();
        for (int i = 0; i < nBytes; i++)
        {
            rawData[i] = mBufferDecode[i];
        }

        return rawData;
    }

    /**
     * 设置数据
     * @param data
     */
    public void setRawData(byte[] data)
    {
        int nPos = 0;

        // 字头
        mHeader = new byte[2];
        Array.Copy(data, nPos, mHeader, 0, 2);
        nPos += 2;
        // 长度
        mLen = data[nPos++];
        // SessionID（1B
        //mID = data[nPos++];
        // 包序号(1B)
        //mIndex = data[nPos++];
        // 命令(1B)
        mCmd = data[nPos++];
        // param(nB)
        if ((mLen - 5) > 0)
        {
            mParam = new byte[mLen - 5];
            Array.Copy(data, nPos, mParam, 0, mLen - 5);
            nPos += mLen - 5;
            mParamLen = mLen - 5;
        }
        // check sum (1B)
        mCheckSum = data[nPos++];

        // end
        mEnd = data[nPos++];
    }

    /**
     * 保存并解码数据
     */
    public bool setData(byte data)
    {
        bool bRet = false;

        switch (mState)
        {
            case PROTOCOL_STATE.HEADER1:
                if (data != (byte)0xFB)
                {
                    break;
                }

                mBufferDecode.Clear();
                mBufferDecode.Add(data);
                mState = PROTOCOL_STATE.HEADER2;
                break;

            case PROTOCOL_STATE.HEADER2:
                if (data != (byte)0xBF)
                {
                    mState = PROTOCOL_STATE.HEADER1;
                    break;
                }

                mBufferDecode.Add(data);
                mState = PROTOCOL_STATE.LENGHT;
                break;

            case PROTOCOL_STATE.LENGHT:
                mLen = data;
                mBufferDecode.Add(data);
                mState = PROTOCOL_STATE.CMD;
                break;

            //		case SESSION_ID:
            //			mID = data;
            //			mBufferDecode.put(data);
            //			mState = PROTOCOL_STATE.INDEX;
            //			break;
            //			
            //		case INDEX:
            //			mIndex = data;
            //			mBufferDecode.put(data);
            //			mState = PROTOCOL_STATE.CMD;
            //			break;

            case PROTOCOL_STATE.CMD:
                mCmd = data;
                mBufferDecode.Add(data);
                mState = PROTOCOL_STATE.PARAM;
                mParamLen = mLen - 5;

                mParam = new byte[mParamLen];
                break;

            case PROTOCOL_STATE.PARAM:
                mBufferDecode.Add(data);
                mParamLen -= 1;
                if (mParamLen == 0)
                {
                    mState = PROTOCOL_STATE.CHECKSUM;
                }
                break;

            case PROTOCOL_STATE.CHECKSUM:
                byte[] b = mBufferDecode.ToArray();
                byte nCheckSum = getCheckSum(b, 2, mBufferDecode.Count - 1);
                if (nCheckSum != data)
                {
                    mState = PROTOCOL_STATE.HEADER1;
                    break;
                }

                mBufferDecode.Add(data);
                mState = PROTOCOL_STATE.END;
                break;

            case PROTOCOL_STATE.END:
                if (data != (byte)0xED)
                {
                    mState = PROTOCOL_STATE.HEADER1;
                    break;
                }

                mBufferDecode.Add(data);
                mState = PROTOCOL_STATE.HEADER1;
                bRet = true;

                int j = 0;
                for (int i = 0; i < mLen - 5; i++)
                {
                    mParam[j++] = mBufferDecode[4 + i];
                }
                //mParamLen = mParam.length;
                break;
        }

        return bRet;
    }

    /**
     * 计算CHECKSUM
     */
    public byte getCheckSum(byte[] data, int nStart, int nEnd)
    {
        byte nCheckSum = 0;

        for (int i = nStart; i <= nEnd; i++)
        {
            nCheckSum += data[i];
        }

        return nCheckSum;
    }

    /**
     * 打包数据
     */
    public byte[] packetData()
    {
        //			            |字头(2B) 长度(1B) 命令(1B) 
        short nTotalLen = (short)(2 + 1 + 1
            //  checksum(1B)  
                    + 1 + 1);

        if (mParamLen == 0)
            nTotalLen += 1;
        else
            nTotalLen += (short)mParamLen;

        byte[] result = new byte[nTotalLen];

        int i = 0;
        // 字头
        result[i++] = (byte)0xFB;
        result[i++] = (byte)0xBF;
        // 长度
        result[i++] = (byte)((nTotalLen - 1) & 0xff);
        //ID
        //result[i++] = mID;
        // index
        //result[i++] = mIndex;
        // 命令
        result[i++] = mCmd;
        // 参数
        if (mParamLen == 0)
        {
            result[i++] = 0;
        }
        else
        {
            for (int n = 0; n < mParamLen; n++)
            {

                result[i++] = mParam[n];
            }
        }

        result[i++] = getCheckSum(result, 2, i);
        //int nCheckSumPos = i;
        //i++;

        // 结束符
        result[i] = (byte)0xED;


        return result;
    }

    public byte[] getmHeader()
    {
        return mHeader;
    }
    public void setmHeader(byte[] mHeader)
    {
        this.mHeader = mHeader;
    }
    public byte getmLen()
    {
        return mLen;
    }
    public void setmLen(byte mLen)
    {
        this.mLen = mLen;
    }
    //	public byte getmID() {
    //		return mID;
    //	}
    //	public void setmID(byte mID) {
    //		this.mID = mID;
    //	}
    public byte getmCmd()
    {
        return mCmd;
    }
    public void setmCmd(byte mCmd)
    {
        this.mCmd = mCmd;
    }
    public byte getmCheckSum()
    {
        return mCheckSum;
    }
    public void setmCheckSum(byte mCheckSum)
    {
        this.mCheckSum = mCheckSum;
    }
    public byte getmEnd()
    {
        return mEnd;
    }
    public void setmEnd(byte mEnd)
    {
        this.mEnd = mEnd;
    }
    public byte[] getmParam()
    {
        return mParam;
    }
    public void setmParam(byte[] mParam)
    {
        this.mParam = mParam;
    }

    public int getmParamLen()
    {
        return mParamLen;
    }

    public void setmParamLen(int mParamLen)
    {
        this.mParamLen = mParamLen;
    }
}