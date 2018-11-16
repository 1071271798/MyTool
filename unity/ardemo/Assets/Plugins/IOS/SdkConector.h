//
//  SdkConector.h
//  SDK接口文件基类;
//
//  by xj;
//

#import <Foundation/Foundation.h>
#include "CBController.h"
#include "AlphaInfoManager.h"

@interface SdkConector : NSObject<ALphainfoClientDataProcess, BlueToothClientDataProcess>
{
    CBController * mCbController;
    AlphaInfoManager * mClientMgr;
    //NSString * mAckGameObjectName;
    MyPeripheral * mConnectDevice;
	BOOL mCBCtrlInit;
}

//单例;
+(SdkConector *)SharedInstance;
+(NSString *)BytesToNSString:(Byte*)bytes;
//unity初始化
-(void)PlatformInit:(char *) gameObjectName;
//unity销毁
-(void)PlatformDestroy;
//开启蓝牙;
-(void)OpenBluetooth;
//关闭蓝牙;
-(void)CloseBluetooth;
//判断蓝牙是否开启
-(BOOL)IsOpenBluetooth;
//打开蓝牙搜索
-(void)StartScan;
//停止蓝牙搜索
-(void)StopScan;
//连接蓝牙；
-(void)ConnenctBluetooth:(char*)mac;
//断开蓝牙连接
-(void)DisConnenctBuletooth;
//取消蓝牙连接
-(void)CanelConnectBluetooth;
//发送消息;
-(void)SendMsg:(Byte)cmd Param:(Byte[])param Len:(int)len;
//保存/修改模型或者动作通知给应用;
-(void)SaveModelOrActions:(char*)contents;
//删除模型;
-(void)DelModel:(char*)contents;
//返回应用;
-(void)BackThirdApp;
//拍照
-(void)Photograph:(char*) name PicPath:(char *) path;
//保存模型
-(void)SaveModel:(char *) name ModelType:(int) type;
//发布模型
-(void)PublishModel:(char *) name;
//激活机器人
-(void)ActivationRobot:(char *)mcuId SN:(char *)sn;
//通用接口
-(void)CallPlatformFunc:(char *)funcName Arg:(char *)arg;
//设置心跳包开关
-(void)SetSendXTState:(int) state;
//蓝牙连接结果;
-(void)ConnenctCallBack:(char*)str;
//发现蓝牙已匹配过的设备
-(void)OnMatchedDevice:(char*)name;
//断开连接回调
-(void)OnDisConnenct:(char*)mac;
//收到数据回调
-(void) onRcvData:(MyPeripheral*) pMyperipheral cmd:(Byte) cmd param:(Byte*) param lens:(int) nLen;

-(void) onClientRcvData:(MyPeripheral*) pMyperipheral cmd:(Byte) cmd param:(Byte*) param lens:(int) nLen;
-(void) disConnectedBlueTooth:(MyPeripheral*) pPeripheral;

-(BOOL)GetBluetoothState;

/** 客户端主动断开蓝牙，并通知到Unity */
-(void)disconnectBluetoothNotifyUnity;

@end
