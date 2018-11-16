//
//  DataPacketSend.m
//  alpha1s
//
//  Created by chenlin on 15/3/6.
//  Copyright (c) 2015年 ubtechinc. All rights reserved.
//

#import "DataPacketSend.h"

@implementation DataPacketSend

-(id) init {
    if (![super init])
        return nil;
    
    self.remainSendCount = 0;
    
    return self;
}

@end
