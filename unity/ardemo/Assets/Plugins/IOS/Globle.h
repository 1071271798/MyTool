//
//  Globle.h
//  alpha1s
//
//  Created by juntian on 15/2/2.
//  Copyright (c) 2015年 ubtechinc. All rights reserved.
//

#ifndef alpha1s_Globle_h
#define alpha1s_Globle_h
#import <Foundation/Foundation.h>
#import "MyPeripheral.h"

@interface ConnectResultAck : NSObject
{
}
@property BOOL result;
@property (strong, nonatomic)MyPeripheral* pPeripheral;
@end



//搜索到蓝牙设备
#define OnFoundDevice @"OnFoundDevice"
//连接蓝牙回调
#define ConnectResult @"ConnenctCallBack"
//蓝牙断开
#define DicBlueConnect @"OnDisConnenct"
//回包
#define OnMsgAck @"OnMsgAck"
//进入unity
#define GotoUnity @"GotoUnity"
//拍照返回
#define PhotographBack @"PhotographBack"
//下载模型
#define DownloadModel @"DownloadModel"
//修改名字
#define ChangeModelName @"ChangeModelName"
//删除模型
#define DeleteModel @"DeleteModel"
//调用Unity函数
#define CallUnityFunc @"CallUnityFunc"


/**
 * 握手
 */
#define DV_HANDSHAKE  0x01
/**
 * 获取动作表名
 */
#define  DV_GETACTIONFILE  0x02
/**
 * 执行动作表
 */
#define  DV_PLAYACTION  0x09
/**
 * 停止播放
 */
#define  DV_STOPPLAY  0x05
/**
 * 声音控制：0x06
	参数：	00 － 静音
 01 - 打开声音
 */
#define  DV_VOICE  0x06
/**
 * 播放控制：0x07
	参数：00 － 暂停
 01 － 继续
 */
#define  DV_PAUSE  0x07

/**
 * 心跳
 */
#define  DV_XT 0x03

/**
 * 修改设备名
 * 参数：新的设备名
 */
#define  DV_MODIFYNAME  0x09
/**
 * 读取状态：0x0a
 下位机返回：声音状态(00+声音状态(00 静音 01有声音))
 播放状态(01+(播放状态00 暂停 01非暂停))
 音量状态(02+2B(255)低字节在前)
 灯状态：（03+00:关， 01开）
 */
#define  DV_READSTATUS  0x0a

/**
 * 调整音量
 * 参数：(0~255)
 */
#define  DV_VOLUME  0x0b

/**
 * 掉电
 */
#define  DV_DIAODIAN  0x0c

/**
 * 灯控制
 * 参数：0-关
 * 		1 开
 */
#define  DV_LIGHT  0x0d

/** 时间校准 **/
#define DV_ADJUST_TIME 0x0e

/** 读取闹铃时间 **/
#define DV_READ_ALARM 0x0f
/** 设置闹铃时间 **/
#define DV_WRITE_ALARM 0x10
/** 读版本号：0x11 */
#define DV_READVERSION 0x11
/** 删除文件:0x12 */
#define DV_DELETE_ACTION    0x12
/** 修改文件0x13*/
#define DV_MODIFY_ACTION_NAME 0x13

/** 传输文件开始：0x14
 参数1：1B          文件名长度
 参数2：nB          文件名
 参数3：2B          文件总帧数
 **/
#define DV_TRANSFER_FILE_START  0x14


/**
 传输文件中：0x15
 参数1：2B    当前帧数
 参数2：245B   文件数据
 **/
#define DV_TRANSFER_FILE_PROGRESS  0x15

/**
 传输文件结束：0x16
 参数1：2B            当前帧数
 参数2：nB <=245B  文件数据
 **/
#define DV_TRANSFER_FILE_FINISH  0x16

/**
 取消传输文件
 **/
#define DV_TRANSFER_FILE_CANCEL  0x17

/** 电量 */
#define DV_READBAT      0x18

/** 读取硬件版本号 */
#define DV_READ_HARDWARE_VERSION 0x20

/** 执行动作列表需要插入复位动作:0x21
 参数1：1B   0-需要   1-不需要
 **/
#define DV_NEED_DEFAULT_ACTION 0x21

/** 动作完成命令:0x31
 下位机主动发送参数1：完成动作文件名
 **/
#define DV_FINISH_PLAY_ACTION 0x31

/**
 * 传动作表
 */
#define  UV_GETACTIONFILE  0X80
#define  UV_STOPACTIONFILE 0x81

#endif
