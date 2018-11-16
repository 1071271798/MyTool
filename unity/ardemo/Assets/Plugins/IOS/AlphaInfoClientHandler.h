//
//  AlphaInfoClientHandler.h
//  alpha1s
//
//  Created by juntian on 15/2/2.
//  Copyright (c) 2015年 ubtechinc. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "MyPeripheral.h"
#import "CBController.h"


@protocol AlphaInfoClientDeletegate<NSObject>

@required
-(CBPeripheral*) getCBPeripheral;
-(MyPeripheral*) getMyPeripheral;

-(void) initClientHandler;
-(void) releaseClientHandler;
-(void) sendCmd:(Byte)cmds datas:(Byte*)pDataToSend lens:(int)nlen;
-(void) sendXT:(BOOL)sendFlag;
-(void) setLastRcvTime;
//-(void) sendCmd:(unsigned char)cmd datas:(unsigned char*)pDataToSend  lens:(int)nlen;
-(BOOL) isTimeOut;


@end

@protocol ALphainfoClientDataProcess <NSObject>

-(void) onRcvData:(MyPeripheral*) pMyperipheral cmd:(Byte) cmd param:(Byte*) param lens:(int) nLen;

@end

@interface AlphaInfoClientHandler : NSObject<AlphaInfoClientDeletegate, MyPeripheralDelegate>

@property(strong, nonatomic) id<ALphainfoClientDataProcess> delegate;
@property(strong, nonatomic) MyPeripheral* myPeripheral;
@end
