//
//  AlphaInfoClientHandler.m
//  alpha1s
//
//  Created by juntian on 15/2/2.
//  Copyright (c) 2015年 ubtechinc. All rights reserved.
//

#import "AlphaInfoClientHandler.h"
#import "ProtocalPacket.h"
#import "ReliableBurstData.h"
#import "ProtocalPacket.h"
#include "Globle.h"
#import "DataPacketSend.h"
#include "SdkConector.h"

//新主控命令最大长度
#define CmdPackageMaxLength 60

@interface AlphaInfoClientHandler()<ReliableBurstDataDelegate>
{
    double _lastRcvTime;
    double _lastSendTime;
    ProtocalPacket* _protocalPacket;
    
    NSThread *thread;
    NSMutableArray* resendArray;
    BOOL bStopRun;
}

@property (atomic,assign) NSInteger sendDXCount;
@property (atomic,assign) NSInteger sendNormalCount;
@end



@implementation AlphaInfoClientHandler

-(id) init{
    
    if (![super init])
        return nil;
    
    _lastRcvTime = [[NSDate date] timeIntervalSince1970];
    _protocalPacket = [[ProtocalPacket alloc] init];
    
    resendArray = [NSMutableArray new];
    thread = [[NSThread alloc]initWithTarget:self selector:@selector(processResendData:) object:nil];
    self.sendDXCount = 0;
	self.sendNormalCount = 0;
    [thread start];
    
    return self;
}

-(void) processResendData:(NSObject*) param {
    while (bStopRun == false) {
        @synchronized(self) {
            DataPacketSend* remove = nil;
            for(DataPacketSend* resend in resendArray) {
                // 如果500MS没有收到回应
                if ([[NSDate date] timeIntervalSince1970]-resend.lastSendTime >= 10) {
                    // 如果超过重发次数，移除列表
                    if (resend.remainSendCount <= 0)
                    {
                        //超过重发次数的发通知
                        [[NSNotificationCenter defaultCenter] postNotificationName:@"ResendCommandNotification" object:resend userInfo:nil];
                        remove = resend;
                        break;
                    }
                    else{
                        // 重发数据
                        int nLens = 0;
                        Byte* pDatas = [resend.packet packetData:&nLens];
                        
                        CBCharacteristicWriteType Writetype;
                        NSData* pData = [NSData dataWithBytes:(const void*)pDatas length:nLens];
                        [_myPeripheral sendTransparentData:pData type:Writetype];
                        
                        resend.lastSendTime = [[NSDate date] timeIntervalSince1970];
                        resend.remainSendCount--;
                        NSLog(@"resend data %d", resend.packet.getCmd);
                        
                        free(pDatas);
                    }
                }
            }
            
            if (remove != nil) {
                [resendArray removeObject:remove];
                remove = nil;
                continue;
            }
        }
        
        [NSThread sleepForTimeInterval:1];
    }
}


-(CBPeripheral*) getCBPeripheral {
    return _myPeripheral.peripheral;
}

-(MyPeripheral*) getMyPeripheral {
    return _myPeripheral;
}

-(BOOL) isTimeOut {
    double currentTime = [[NSDate date] timeIntervalSince1970];
    
//    if (currentTime-_lastRcvTime >= 6)//6
//        return true;
    
    if(self.sendDXCount>3)
        return true;
    if(self.sendNormalCount>6)
		return true;
    return false;
}


-(void) sendXT :(BOOL)sendFlag{
    double currentTime = [[NSDate date] timeIntervalSince1970];
    if (currentTime-_lastSendTime > 2){
        
        if (sendFlag){
			self.sendDXCount++;
			[self sendCmd:DV_XT datas:NULL lens:0];
		}
		else{
			_lastRcvTime = currentTime;
		}
        _lastSendTime = currentTime;
        
        //NSLog(@"send xt");
    }
}

-(void) setLastRcvTime
{
	_lastRcvTime = [[NSDate date] timeIntervalSince1970];
}

-(void) sendCmd:(Byte)cmds datas:(Byte*)pDataToSend lens:(int)nlen {
    ProtocalPacket* packet = [[ProtocalPacket alloc] init];
    [packet setCmd:cmds];
    [packet setParam:pDataToSend lens:nlen];
    
    int nLens = 0;
    Byte* pDatas = [packet packetData:&nLens];
    
    CBCharacteristicWriteType Writetype;
    NSData* pData = [NSData dataWithBytes:(const void*)pDatas length:nLens];
    NSString *deviceName = _myPeripheral.peripheral.name.lowercaseString;
    if([deviceName hasPrefix:@"my_jimu_"]) //新主控分包处理
    {
        if(nLens > CmdPackageMaxLength)
        {
            int count = nLens / CmdPackageMaxLength;
            if(nLens % CmdPackageMaxLength > 0)
            {
                count++;
            }
            for(int i = 0; i < count; i++)
            {
                NSUInteger loc = i * CmdPackageMaxLength;
                NSUInteger len = CmdPackageMaxLength;
                if(i == count - 1)
                {
                    len = nLens - loc;
                }
                [_myPeripheral sendTransparentData:[pData subdataWithRange:NSMakeRange(loc, len)] type:Writetype];

                if(i != count - 1)
                {
                    [NSThread sleepForTimeInterval:0.005];
                }
            }
        }
        else
        {
            [_myPeripheral sendTransparentData:pData type:Writetype];
        }
    }
    else
    {
        [_myPeripheral sendTransparentData:pData type:Writetype];
    }
    
    //NSLog(@"send cmd %d params = %@", cmds, [SdkConector BytesToNSString:pDatas]);
    
    free(pDatas);

    _lastSendTime = [[NSDate date] timeIntervalSince1970];
    
    DataPacketSend* dataPacketSend = [[DataPacketSend alloc] init];
    dataPacketSend.packet = packet;
//    dataPacketSend.packet = [[ProtocalPacket alloc] init];
//    
//    [dataPacketSend.packet setCmd:[packet getCmd]];
//    Byte* param = [packet getParams];
//    int nParamLen = [packet getParamLen];
//    
//    [dataPacketSend.packet setParam:param lens:nParamLen];

    
    dataPacketSend.lastSendTime = _lastRcvTime;
    
    self.sendNormalCount++;
    @synchronized(self){
        [resendArray addObject:dataPacketSend];
    }
    
}

-(void) initClientHandler {
    self.myPeripheral.deviceInfoDelegate = self;
    self.myPeripheral.proprietaryDelegate = self;
    [self.myPeripheral setTransDataNotification:true];
    self.myPeripheral.transmit.delegate = self;
    self.myPeripheral.transDataDelegate = self;
}

-(void) releaseClientHandler {
    bStopRun = YES;
    @synchronized(self) {
        [resendArray removeAllObjects];
    }
    self.sendDXCount = 0;
	self.sendNormalCount = 0;
    [thread cancel];
}

-(void) removeResendArrayByCmd:(Byte) cmd {
    @synchronized(self) {
        DataPacketSend* remove = nil;
        for(DataPacketSend* resend in resendArray) {
            if ([resend.packet getCmd] == cmd) {
                remove = resend;
                break;
            }
        }
        
        if (remove != nil) {
            [resendArray removeObject:remove];
        }
    }
}

/*!
 *  @method                                 reliableBurstData:didSendDataWithCharacteristic:
 *
 *  @discussion                             This method is invoked when the data has been sent.
 *
 */
- (void)reliableBurstData:(ReliableBurstData *)reliableBurstData didSendDataWithCharacteristic:(CBCharacteristic *)transparentDataWriteChar {
}

- (void)MyPeripheral:(MyPeripheral *)peripheral didReceiveTransparentData:(NSData *)data {
    //NSLog(@"Rcv data");
    self.sendDXCount=0;
	self.sendNormalCount=0;
    unsigned char * pData = (unsigned char*)[data bytes];
    NSUInteger nLen = [data length];
    
    //NSLog(@"Rcv data lens = %lu", (unsigned long)nLen);
	NSMutableString* pTemp = [NSMutableString new];
    for (int i=0; i<nLen; i++) {
        [pTemp appendFormat:@"%02X ", pData[i]];
    }
    logEvent(@"receive:%@",pTemp);
    
    for (int i=0; i<nLen; i++) {
        if ([_protocalPacket setData:pData[i]] == true) {
            // 一帐数据接收完成
            if (self.delegate != nil) {
                [self.delegate onRcvData:peripheral cmd:[_protocalPacket getCmd] param:[_protocalPacket getParams] lens:[_protocalPacket getParamLen]];
            }
            
            _lastRcvTime = [[NSDate date] timeIntervalSince1970];
            
            [self removeResendArrayByCmd:[_protocalPacket getCmd]];
        }
    }
    
}

@end
