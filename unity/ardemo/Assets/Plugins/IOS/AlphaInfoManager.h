//
//  AlphaInfoManager.h
//  alpha1s
//
//  Created by juntian on 15/2/2.
//  Copyright (c) 2015年 ubtechinc. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "AlphaInfoClientHandler.h"

@protocol BlueToothClientDataProcess <NSObject>

@optional
-(void) onClientRcvData:(MyPeripheral*) pMyperipheral cmd:(Byte) cmd param:(Byte*) param lens:(int) nLen;
-(void) disConnectedBlueTooth:(MyPeripheral*) pPeripheral;

@end

@interface AlphaInfoManager : NSObject<ALphainfoClientDataProcess>

@property (strong, atomic)NSMutableArray* ClientDataProcessdeletegates;

@property (nonatomic, strong, readonly) NSMutableArray *alphaInfoList;

// 初始化客户端管理类
-(void) initAlphaInfoManager;
// 释放资源
-(void) releaseAlphaInfoManager;

// 添加客户端
-(void) addClient:(id<AlphaInfoClientDeletegate>)deletegate;
-(void) AddClient:(MyPeripheral*) pPeripheral ack:(id<ALphainfoClientDataProcess>) deletage;
-(void) removeClient:(MyPeripheral*) pPeripheral;
-(void) RemoveAllClient;

-(BOOL) isSelectClient;

-(void) addToDataProcessdeletegate:(id<BlueToothClientDataProcess>)ClientDataProcessdeletegate;
-(void) removeDataProcessDelegate:(id<BlueToothClientDataProcess>)delegate;

-(void) notifyDisConnected:(MyPeripheral*) pPeripheral;

-(void) sendCmd:(MyPeripheral*) myPeripheral cmd:(Byte)cmd param:(Byte*)param lens:(int) nLen;
-(void) sendToAllDevice:(Byte)cmd param:(Byte*)param lens:(int) nLen;

-(void) SetSendXTState:(BOOL)state;

@end
