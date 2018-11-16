//
//  AlphaInfoManager.m
//  alpha1s
//
//  Created by juntian on 15/2/2.
//  Copyright (c) 2015年 ubtechinc. All rights reserved.
//

#import "AlphaInfoManager.h"
#import "AlphaInfoClientHandler.h"

@interface AlphaInfoManager ()
{
//    NSMutableArray* _alphaInfoList;
    BOOL            _mStop;
    NSThread *thread;
	BOOL mSendXTFlag;
}

@end

@implementation AlphaInfoManager

-(BOOL) isExistClient:(id<AlphaInfoClientDeletegate>) check {
    BOOL bRet = false;
    
    for (int i=0; i<[_alphaInfoList count]; i++) {
        id<AlphaInfoClientDeletegate> client = [_alphaInfoList objectAtIndex:i];

        if ([check getCBPeripheral] == [client getCBPeripheral]){
            bRet = true;
            break;
        }
    }
    
    return bRet;
}

-(id<AlphaInfoClientDeletegate>) getClientDelegate:(MyPeripheral*) myPeripheral {
    id<AlphaInfoClientDeletegate> delegate = nil;
    
    for (int i=0; i<[_alphaInfoList count]; i++) {
        id<AlphaInfoClientDeletegate> client = [_alphaInfoList objectAtIndex:i];
        
        if ([client getMyPeripheral] == myPeripheral){
            delegate = client;
            break;
        }
    }

    
    return delegate;
}

-(void) processManager:(NSObject*) param {
    while (!_mStop) {
        id<AlphaInfoClientDeletegate> remove = nil;
        for (id<AlphaInfoClientDeletegate> pClient in _alphaInfoList) {
            if ([pClient isTimeOut] && mSendXTFlag) {
                remove = pClient;
                break;
                
            }else {
                [pClient sendXT:mSendXTFlag];
            }
        }
        
        if (remove != nil) {
            [self notifyDisConnected:[remove getMyPeripheral]];
            [remove releaseClientHandler];
            
            [_alphaInfoList removeObject:remove];
            
            remove = nil;
            continue;
        }
        sleep(1);
    }
}

-(void) initAlphaInfoManager {
    
    _ClientDataProcessdeletegates = [NSMutableArray new];
    _mStop = false;
    _alphaInfoList = [NSMutableArray new];
    mSendXTFlag = true;
    thread = [[NSThread alloc]initWithTarget:self selector:@selector(processManager:) object:nil];
    
    [thread start];
    
//    dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
//        
//        while (!_mStop) {
//            id<AlphaInfoClientDeletegate> remove = nil;
//            for (id<AlphaInfoClientDeletegate> pClient in _alphaInfoList) {
//                if ([pClient isTimeOut]) {
//                    remove = pClient;
//                    break;
//                    
//                }else {
//                    [pClient sendXT];
//                }
//            }
//            
//            if (remove != nil) {
//                [self notifyDisConnected:[remove getMyPeripheral]];
//                [remove releaseClientHandler];
//                
//                [_alphaInfoList removeObject:remove];
//                
//                remove = nil;
//                continue;
//            }
//            sleep(1);
//        }
//    });
}

-(void) releaseAlphaInfoManager {
    _mStop = true;
    [thread cancel];
}

-(void) removeClient:(MyPeripheral*) pPeripheral {
    for (id<AlphaInfoClientDeletegate>client in _alphaInfoList) {
        if ([client getMyPeripheral] == pPeripheral) {
            [self notifyDisConnected:[client getMyPeripheral]];
            [client releaseClientHandler];
            [_alphaInfoList removeObject:client];
            
            return;
        }
    }
}

-(void) RemoveAllClient
{
    for (id<AlphaInfoClientDeletegate>client in _alphaInfoList) {
        //[self notifyDisConnected:[client getMyPeripheral]];
        [client releaseClientHandler];
    }
    [_alphaInfoList removeAllObjects];
}

-(void) SetSendXTState:(BOOL)state
{
	mSendXTFlag = state;
}
-(void) AddClient:(MyPeripheral*) pPeripheral ack:(id<ALphainfoClientDataProcess>) deletage
{
    for (id<AlphaInfoClientDeletegate>client in _alphaInfoList) {
        if ([client getMyPeripheral] == pPeripheral) {
            return;
        }
    }
    AlphaInfoClientHandler * client = [[AlphaInfoClientHandler alloc] init];
    client.myPeripheral = pPeripheral;
    client.delegate = deletage;
    [client initClientHandler];
    [self addClient: client];
}
-(void) addClient:(id<AlphaInfoClientDeletegate>)deletegate {
    
    if ([self isExistClient:deletegate])
        return;
    
    [_alphaInfoList addObject:deletegate];
}

-(void) sendCmd:(MyPeripheral*) myPeripheral cmd:(Byte)cmd param:(Byte*)param lens:(int) nLen {
    id<AlphaInfoClientDeletegate> client = [self getClientDelegate:myPeripheral];
    
    if (client) {
		//mSendXTFlag = true;
        [client sendCmd:cmd datas:param lens:nLen];
    }
}

-(void) sendToAllDevice:(Byte)cmd param:(Byte *)param lens:(int)nLen {
    @synchronized(self) {
        for (id<AlphaInfoClientDeletegate> client in _alphaInfoList) {
            [client sendCmd:cmd datas:param lens:nLen];
        }
    }
}

-(BOOL) isExistDataProcessDelegate:(id<BlueToothClientDataProcess>)delegate {
    for (id<BlueToothClientDataProcess> DataProcess in _ClientDataProcessdeletegates) {
        if (DataProcess == delegate)
            return YES;
    }
    
    return NO;
}

-(void) addToDataProcessdeletegate:(id<BlueToothClientDataProcess>)ClientDataProcessdeletegate
{
    //self.ClientDataProcessdeletegate = ClientDataProcessdeletegate;
    @synchronized(_ClientDataProcessdeletegates)
    {
        if ([self isExistDataProcessDelegate:ClientDataProcessdeletegate])
            return;
        
        [_ClientDataProcessdeletegates addObject:ClientDataProcessdeletegate];
    }
    
}

-(void) removeDataProcessDelegate:(id<BlueToothClientDataProcess>)delegate {
    [_ClientDataProcessdeletegates removeObject:delegate];
}

-(void) notifyDisConnected:(MyPeripheral *)pPeripheral {
    for (id<BlueToothClientDataProcess>dataProcess in _ClientDataProcessdeletegates) {
        if ([dataProcess respondsToSelector:@selector(disConnectedBlueTooth:)])
        {
            [dataProcess disConnectedBlueTooth:pPeripheral];
        }
    }
}

-(void) onRcvData:(MyPeripheral*) pMyperipheral cmd:(Byte) cmd param:(Byte*) param lens:(int) nLen
{
    @synchronized(_ClientDataProcessdeletegates)
    {
//        if (self.ClientDataProcessdeletegate != nil) {
//            [self.ClientDataProcessdeletegate onClientRcvData:pMyperipheral cmd:cmd param:param lens:nLen];
//        }
        
//        for (id<BlueToothClientDataProcess> client in _ClientDataProcessdeletegates)
//        {
//            
//            if ([client respondsToSelector:@selector(onClientRcvData:cmd:param:lens:)])
//            {
//                [client onClientRcvData:pMyperipheral cmd:cmd param:param lens:nLen];
//            }
//            
//            
//        }
        
        for (int i = 0; i < _ClientDataProcessdeletegates.count; i ++)
        {
            id<BlueToothClientDataProcess> client = _ClientDataProcessdeletegates[i];
            if ([client respondsToSelector:@selector(onClientRcvData:cmd:param:lens:)])
            {
                [client onClientRcvData:pMyperipheral cmd:cmd param:param lens:nLen];
            }
        }
        
    }
}

@end
