//
//  ProtocalPacket.m
//  alpha1s
//
//  Created by juntian on 15/2/2.
//  Copyright (c) 2015年 ubtechinc. All rights reserved.
//

#import "ProtocalPacket.h"

 typedef enum {
    HEADER1,
    HEADER2,
    LENGHT,
    //SESSION_ID,
    //INDEX,
    CMD,
    PARAM,
    CHECKSUM,
    END
}PROTOCAL_STATE;

@interface ProtocalPacket()
{
    // 字头
    unsigned char _header[2];
    // 长度
    Byte _len;
    // 命令
    Byte _cmd;
    // check sum
    Byte _checkSum;
    // end
    Byte _end;
    // param
    NSMutableData* _mParam;
    
    // 命令长度
    int _paramLen;
    
    PROTOCAL_STATE mState;
    NSMutableData* _mBufferDecode;
    
}
@end

@implementation ProtocalPacket

-(id) init{
    if (![super init]) {
        return nil;
    }

    mState = HEADER1;
    _mBufferDecode = [NSMutableData new];
    [_mBufferDecode resetBytesInRange:NSMakeRange(0, [_mBufferDecode length])];
    [_mBufferDecode setLength:0];
    //[_mBufferDecode removeAllObjects];
    
    _mParam = [NSMutableData new];
    
    return self;
}

-(Byte) getCmd {
    return self->_cmd;
}

-(Byte*) getParams {
    return (Byte*)[_mParam bytes];
}

-(Byte*) getRawData:(int *)pLen {
    
    return (Byte*)[_mBufferDecode bytes];
}

-(int) getParamLen {
    return (int)[_mParam length];
}

/**
 * 计算CHECKSUM
 */
- (Byte) getCheckSum:(const Byte*) data start:(int)nStart end:(int) nEnd{
    Byte nCheckSum = 0;
    
    for(int i=nStart; i<=nEnd; i++){
        nCheckSum += data[i];
    }
    
    return nCheckSum;
}

-(BOOL) setData:(Byte)data {
    BOOL bRet = false;
    
    switch (mState) {
        case HEADER1:
            if (data != 0xFB)
                break;
            
            [_mBufferDecode resetBytesInRange:NSMakeRange(0, [_mBufferDecode length])];
            [_mBufferDecode setLength:0];
            [_mBufferDecode appendBytes:&data length:1];
            mState = HEADER2;
            break;
            
        case HEADER2:
            if (data != 0XBF) {
                mState = HEADER1;
                if (data == 0xfb) {
                    [self setData:data];
                }
                break;
            }
            
            [_mBufferDecode appendBytes:&data length:1];
            mState = LENGHT;

            break;
            
        case LENGHT:
            _len = data;
            
            [_mBufferDecode appendBytes:&data length:1];
            mState = CMD;
            break;
            
        case CMD:
            self->_cmd = data;
            
            [_mBufferDecode appendBytes:&data length:1];
            mState = PARAM;
            
            _paramLen = _len-5;
            break;
            
        case PARAM:
            [_mBufferDecode appendBytes:&data length:1];
            _paramLen -= 1;
            if (_paramLen == 0){
                mState = CHECKSUM;
            }
            break;
            
        case CHECKSUM:
        {
            NSUInteger datalen = [_mBufferDecode length];
            const Byte* pDataToCheck = (const Byte*)[_mBufferDecode bytes];
            
            Byte nCheckSum =[self getCheckSum:pDataToCheck start:2 end:((int)datalen-1)];
            if (nCheckSum != data) {
                mState = HEADER1;
                if (data == 0xfb) {
                    [self setData:data];
                }
                break;
            }
            
            [_mBufferDecode appendBytes:&data length:1];
            mState = END;
        }
            break;
            
        case END:
            if (data != 0xED) {
                mState = HEADER1;
                if (data == 0xfb) {
                    [self setData:data];
                }
                break;
            }
            
            mState = HEADER1;
            
            [_mBufferDecode appendBytes:&data length:1];
            bRet = true;
            
            [_mParam resetBytesInRange:NSMakeRange(0, [_mParam length])];
            [_mParam setLength:0];
            
            const Byte* pDecode = (const Byte*)[_mBufferDecode bytes];
            [_mParam appendBytes:&pDecode[4] length:_len-5];
//            for (int i=0; i<_len-5; i++) {
//                NSNumber* p = [_mBufferDecode objectAtIndex:i];
//                Byte dat = [p unsignedCharValue];
//                //[_mParam addObject:p];
//                [_mParam appendBytes:&dat length:1];
//            }
            
            break;
            
        default:
            break;
    }
    return bRet;
}

-(void) setCmd:(Byte)cmd {
    self->_cmd = cmd;
}

-(void) setParam:(Byte *)pParam lens:(int)len {
    [_mParam resetBytesInRange:NSMakeRange(0, [_mParam length])];
    [_mParam setLength:0];
    
    if (pParam == NULL || len == 0)
        return;
    
    [_mParam appendBytes:pParam length:len];
    
//    for (int i=0; i<len; i++) {
//        NSNumber* p = [NSNumber numberWithUnsignedChar:pParam[i]];
//        Byte dat = [p unsignedCharValue];
//        [_mParam appendBytes:&dat length:1];
//        //[_mParam addObject:p];
//    }
    
    self->_paramLen = len;
}

-(Byte*) packetData :(int *)pLen{
    //			            |字头(2B) 长度(1B) 命令(1B)
    Byte nTotalLen = (Byte) (2         + 1     + 1
                             //  checksum(1B)
                             + 1    +1);
    
    if (self->_paramLen == 0)
        nTotalLen += 1;
    else
        nTotalLen += self->_paramLen;
    
    Byte* result = (Byte*)malloc(nTotalLen);
    
    int i = 0;
    // 字头
    result[i++] =  0xFB;
    result[i++] =  0xBF;
    // 长度
    result[i++] =  (nTotalLen-1);
    //ID
    //result[i++] = mID;
    // index
    //result[i++] = mIndex;
    // 命令
    result[i++] = self->_cmd;
    // 参数
    if (_paramLen == 0){
        result[i++] = 0;
    }
    else{
        const Byte* pParam = (const Byte*)[_mParam bytes];
        NSUInteger nLens = [_mParam length];
//        for(int n=0; n<nLens; n++){
//            //NSNumber* p = [_mParam objectAtIndex:i];
//            //Byte dat = [_mParam get]
//            result[i++] = pParam[i];
//        }
        memcpy(&result[i], pParam, nLens);
        i += nLens;
    }
    
    Byte check = [self getCheckSum:result start:2 end:i-1];
    result[i++] = check;
    
    result[i] = 0xED;
    
    *pLen = i+1;
    
    return result;
}


@end
