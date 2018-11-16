//
//  DataPacketSend.h
//  alpha1s
//
//  Created by chenlin on 15/3/6.
//  Copyright (c) 2015年 ubtechinc. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "ProtocalPacket.h"

@interface DataPacketSend : NSObject

@property(strong, atomic) ProtocalPacket* packet;       // 要发送的数据
@property double lastSendTime;                          // 上一次发送的时间
@property double remainSendCount;                       // 剩下重发的次数

@end
