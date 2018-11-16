//
//  SdkConector.h
//  IOS_SDK
//
//  Created by xj on 15-09-11.
//
//

#import "SdkConector.h"
#import "sys/utsname.h"

#include <sys/sysctl.h>
#include <net/if.h>
#include <net/if_dl.h>
#import "Globle.h"
#import "GTMBase64.h"
#import "Jimu-Swift.h"

//对unity的接口
#if defined(__cplusplus)
extern "C"{
#endif
    extern void UnitySendMessage(const char *, const char *, const char *);
    extern NSString* CreateNSString (const char* string);
#if defined(__cplusplus)
}
#endif
//实现对外接口, C#调用Objective C;
#if defined(__cplusplus)
extern "C"{
#endif
    
    NSString* CreateNSString (const char* string)
    {
        if (string)
            return [NSString stringWithUTF8String: string];
        else
            return [NSString stringWithUTF8String: ""];
    }
    
    char* MakeStringCopy (const char* string)
    {
        if (string == NULL)
            return NULL;
        
        char* res = (char*)malloc(strlen(string) + 1);
        strcpy(res, string);
        return res;
    }
	
	void IosPlatformInit(char * gameObjectName)
	{
		[[SdkConector SharedInstance] PlatformInit:gameObjectName];
	}
	
	void IosPlatformDestroy()
	{
		[[SdkConector SharedInstance] PlatformDestroy];
	}
    
    void IosOpenBluetooth(char * gameObjectName)
    {
        [[SdkConector SharedInstance] OpenBluetooth];
    }
    
    void IosCloseBluetooth()
    {
        [[SdkConector SharedInstance] CloseBluetooth];
    }
	
	bool IosIsOpenBluetooth()
	{
		return [[SdkConector SharedInstance] IsOpenBluetooth];
	}
    
    void IosStartScan()
    {
        [[SdkConector SharedInstance] StartScan];
    }
	
	void IosStopScan()
	{
		[[SdkConector SharedInstance] StopScan];
	}
    
    void IosConnenctBluetooth(char * mac)
    {
		[[SdkConector SharedInstance] ConnenctBluetooth:mac];
    }
    
    void IosDisConnenctBuletooth()
    {
        [[SdkConector SharedInstance] DisConnenctBuletooth];
    }
	
	void IosCannelConnectBluetooth()
	{
		//[[SdkConector SharedInstance] CanelConnectBluetooth];
	}
    
    void IosSendMsg(Byte cmd, Byte param[], int len)
    {
        [[SdkConector SharedInstance] SendMsg:cmd Param:param Len:len];
    }
    
    void IosSaveModelOrActions(char * contents)
    {
        [[SdkConector SharedInstance] SaveModelOrActions:contents];
    }
    
    void IosDelModel(char * contents)
    {
        return [[SdkConector SharedInstance] DelModel:contents];
    }

    
    void IosBackThirdApp()
    {
        [[SdkConector SharedInstance] BackThirdApp];
    }
	
	void IosPhotograph(char * name, char * path)
	{
		[[SdkConector SharedInstance] Photograph:name PicPath:path];
	}
	
	void IosSaveModel(char * name, int type)
	{
		[[SdkConector SharedInstance] SaveModel:name ModelType:type];
	}

	void IosPublishModel(char *  name)
	{
		[[SdkConector SharedInstance] PublishModel:name];
	}

	void IosActivationRobot(char * mcuId, char * sn)
	{
		[[SdkConector SharedInstance] ActivationRobot:mcuId SN:sn];
	}
	
	void IosCallPlatformFunc(char * funcName, char * arg)
	{
		[[SdkConector SharedInstance] CallPlatformFunc:funcName Arg:arg];
	}
	
	void IosSetSendXTState(int state)
	{
		[[SdkConector SharedInstance] SetSendXTState:state];
	}
	
	char* IosGetUserData(char * dataType)
	{
        if(strcmp(dataType, "language") == 0)
        {
            return MakeStringCopy([[JMLanguagesUtilities languageCodeToUnity] UTF8String]);
        }
        else if (strcmp(dataType, "userId") == 0)
        {
            return MakeStringCopy([[JMUnityInterface instance].currentUserId UTF8String]);
        }
		return MakeStringCopy("");
	}
    
    char* IosGetData(char *dataType, char *jsonString)
    {
        if(strcmp(dataType, "program") == 0) //获取程序列表
        {
            NSString *str = [[NSString alloc] initWithCString:jsonString encoding:NSUTF8StringEncoding];
            JMProgramsManager *manager = [[JMProgramsManager alloc] initWithJsonString:str];
            NSString *result = [manager fetchProgramsJsonString];
            [str release];
            [manager release];
            return MakeStringCopy([result UTF8String]);
        }
        else if (strcmp(dataType, "diyStepData") == 0) //获取DIY浏览步数信息
        {
            NSString *jsonString = [[JMDIYBrowseUnityModel shared] getBrowseStepsInfoForUnity];
            return MakeStringCopy([jsonString UTF8String]);
        }
        else if (strcmp(dataType, "guideState") == 0)   //获取当前页面是否需要指引
        {
            NSString *str = [[NSString alloc] initWithCString:jsonString encoding:NSUTF8StringEncoding];
            AppDelegate *delegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
            JMCoreViewController *vc = [delegate rootViewController];
            NSString *jsonString = [vc isNeedGuide:str];

            return MakeStringCopy([jsonString UTF8String]);
        }
        
        return MakeStringCopy("");
    }
    
    /** 连接蓝牙音箱 */
    void connectSpeaker(char * mac)
    {
        NSURL *url = [NSURL URLWithString:@"prefs:root=Bluetooth"];
        if ([[UIApplication sharedApplication] canOpenURL:url])
        {
            [[UIApplication sharedApplication] openURL:url];
        }
    }
    
    void IosLogEvent(char *str)
    {
        NSString *log=[[NSString alloc] initWithUTF8String:str];
//        logEvent(@"%@", log);
        [log release];
    }
    
    void IosLogInfo(char *str)
    {
        NSString *log=[[NSString alloc] initWithUTF8String:str];
        logInfo(@"%@", log);
        [log release];
    }
    
    void IosLogDebug(char *str)
    {
        NSString *log=[[NSString alloc] initWithUTF8String:str];
        logDebug(@"%@", log);
        [log release];
    }
    
    /** 
     unity文件操作记录
     modelId:与文件关联的模型id
     modelType:与文件关联的模型类型,0:官方模型;1:自建模型;
     filePath:文件绝对路径
     oType:操作类型,1:新增;2:修改;3:删除;
     */
    bool operateFile(char *modelId, int modelType, char *filePath, int oType)
    {
        BOOL succeed=NO;

        if(modelId!=NULL && strlen(modelId)>0 && filePath!=NULL && strlen(filePath)>0)
        {
            if(modelType==0)
                modelType=1;
            else if(modelType==1)
                modelType=2;

            NSString *mid=[[NSString alloc] initWithCString:modelId encoding:NSUTF8StringEncoding];
            NSString *path=[[NSString alloc] initWithCString:filePath encoding:NSUTF8StringEncoding];
            SyncLocalFileManager *localFileManager = [[SyncLocalFileManager alloc] init];
            if(oType==1 || oType==2)
            {
                succeed = [localFileManager updateFileWithPath:path modelType:modelType modelCustomId:mid modelServerId:[JMUnityInterface instance].currentModelServerId];
            }
            else if(oType==3)
            {
                succeed = [localFileManager deleteFileWithPath:path];
            }
            [mid release];
            [path release];
            [localFileManager release];
        }

        return succeed;
    }
#if defined(__cplusplus)
}
#endif

@implementation SdkConector
static SdkConector * mInstance = nil;

/**
 *  单例;
 */
+(SdkConector *)SharedInstance
{
    if (mInstance == nil)
    {
        mInstance = [[self alloc] init];               
    }
    return mInstance;
}

+(NSString *)BytesToNSString:(Byte*)bytes
{
    NSString * hexStr = @"";
    if (nil != bytes && NULL != bytes) {
        for (int i = 0, imax = sizeof(bytes); i < imax; ++i) {
            NSString * newHexStr = [NSString stringWithFormat:@"%x", bytes[i]&0xff];
            if ([newHexStr length] == 1) {
                hexStr = [NSString stringWithFormat:@"%@0%@", hexStr, newHexStr];
            }
            else
            {
                hexStr = [NSString stringWithFormat:@"%@%@", hexStr, newHexStr];
            }
        }
    }
    return hexStr;
}

-(id)init
{
    [super init];
    mConnectDevice = nil;
    mCbController = [[CBController alloc] init];
    mClientMgr = [[AlphaInfoManager alloc] init];
    [mClientMgr initAlphaInfoManager];
	mCBCtrlInit = false;
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(FoundDeviceNotify:)  name:OnFoundDevice object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(ConnectBlueNotify:)  name:ConnectResult object:nil];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(PhotographBackNotify:)  name:PhotographBack object:nil];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(GotoUnityNotify:)  name:GotoUnity object:nil];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(DownloadModelNotify:)  name:DownloadModel object:nil];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(ChangeNameNotify:)  name:ChangeModelName object:nil];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(DeleteModelNotify:)  name:DeleteModel object:nil];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(CallUnityFuncNotify:)  name:CallUnityFunc object:nil];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(UnityBlueInitNotify:)  name:@"UnityBlueInit" object:nil];
    return self;
}

-(void)dealloc
{
    [super dealloc];
    [mClientMgr releaseAlphaInfoManager];
    [mClientMgr release];
    [mCbController release];
    mConnectDevice = nil;
	mCBCtrlInit = false;
    [[NSNotificationCenter defaultCenter] removeObserver:self name:OnFoundDevice object:nil];
    [[NSNotificationCenter defaultCenter] removeObserver:self name:ConnectResult object:nil];
	[[NSNotificationCenter defaultCenter] removeObserver:self name:PhotographBack object:nil];
	[[NSNotificationCenter defaultCenter] removeObserver:self name:GotoUnity object:nil];
	[[NSNotificationCenter defaultCenter] removeObserver:self name:DownloadModel object:nil];
	[[NSNotificationCenter defaultCenter] removeObserver:self name:ChangeModelName object:nil];
	[[NSNotificationCenter defaultCenter] removeObserver:self name:DeleteModel object:nil];
	[[NSNotificationCenter defaultCenter] removeObserver:self name:CallUnityFunc object:nil];
	[[NSNotificationCenter defaultCenter] removeObserver:self name:@"UnityBlueInit" object:nil];
}

//unity初始化
-(void)PlatformInit:(char *) gameObjectName
{
	
}

//unity销毁
-(void)PlatformDestroy
{

}

//开启蓝牙;
-(void)OpenBluetooth
{
	//mAckGameObjectName = CreateNSString(gameObjectName);
	if (!mCBCtrlInit){
		[mCbController viewDidLoad];
		mCBCtrlInit = true;
	}
    
}
//关闭蓝牙;
-(void)CloseBluetooth
{
}
//判断蓝牙是否开启
-(BOOL)IsOpenBluetooth
{
	[self OpenBluetooth];
	return mCbController.isBlueOpenFlag;
}
//打开蓝牙搜索
-(void)StartScan
{
    [mCbController startScan];
}
//停止蓝牙搜索
-(void)StopScan
{
	[mCbController stopScan];
}
//连接蓝牙；
-(void)ConnenctBluetooth:(char*)mac
{
    [mCbController ConnectDevice:CreateNSString(mac)];
}
//断开蓝牙连接
-(void)DisConnenctBuletooth
{
	if (nil != mConnectDevice){
		[mCbController DisConnectDevice];
		[mClientMgr RemoveAllClient];
		[mClientMgr removeDataProcessDelegate:self];
		mConnectDevice = nil;
        
        [[NSNotificationCenter defaultCenter] postNotificationName:@"DisconnectBLE" object:nil];
	}
}
//取消蓝牙连接
-(void)CanelConnectBluetooth
{
	//[mCbController cancelConnection];
}
//发送消息;
-(void)SendMsg:(Byte)cmd Param:(Byte[])param Len:(int)len
{
    @synchronized(self) {
        if (nil != mConnectDevice)
        {
            [mClientMgr sendCmd:mConnectDevice cmd:cmd param:param lens:len];
        }
    }
    
}
//保存/修改模型或者动作通知给应用;
-(void)SaveModelOrActions:(char*)contents
{
}
//删除模型;
-(void)DelModel:(char*)contents
{
}
//返回应用;
-(void)BackThirdApp
{
    [[NSNotificationCenter defaultCenter] postNotificationName:@"UnityBackApp" object:nil];
}

//拍照
-(void)Photograph:(char*) name PicPath:(char *) path;
{
    [[NSNotificationCenter defaultCenter] postNotificationName:@"UnityPhotograph" object:[NSDictionary dictionaryWithObjectsAndKeys:CreateNSString(name), @"modelID",CreateNSString(path), @"picPath", nil]];
}
//保存模型
-(void)SaveModel:(char *) name ModelType:(int) type
{
    [[NSNotificationCenter defaultCenter] postNotificationName:@"UnitySaveModel" object:[NSDictionary dictionaryWithObjectsAndKeys:CreateNSString(name), @"modelID",[@(type) stringValue], @"modelType", nil]];
}
//发布模型
-(void)PublishModel:(char *) name
{
	[[NSNotificationCenter defaultCenter] postNotificationName:@"UnityPublishModel" object:[NSDictionary dictionaryWithObjectsAndKeys:CreateNSString(name), @"modelID", nil]];
}
//激活机器人
-(void)ActivationRobot:(char *)mcuId SN:(char *)sn
{
	[[NSNotificationCenter defaultCenter] postNotificationName:@"UnityActivationRobot" object:[NSDictionary dictionaryWithObjectsAndKeys:CreateNSString(mcuId), @"mcuId",CreateNSString(sn), @"deviceSn", nil]];
}

//通用接口
-(void)CallPlatformFunc:(char *)funcName Arg:(char *)arg
{
	[[NSNotificationCenter defaultCenter] postNotificationName:@"UnityCallPlatformFunc" object:[NSDictionary dictionaryWithObjectsAndKeys:CreateNSString(funcName), @"funcName",CreateNSString(arg), @"arg", nil]];
}
//设置心跳包开关
-(void)SetSendXTState:(int) state
{
	if (nil != mClientMgr)
	{
		if (1 == state)
		{
			[mClientMgr SetSendXTState:true];
		}
		else{
			[mClientMgr SetSendXTState:false];
		}
	}
}
//蓝牙连接结果;
-(void)ConnenctCallBack:(char*)str
{
}
//发现蓝牙已匹配过的设备
-(void)OnMatchedDevice:(char*)name
{
}


//断开连接回调
-(void)OnDisConnenct:(char*)mac
{
}

-(BOOL)GetBluetoothState
{
	if (nil != mConnectDevice)
		return true;
	return false;
}
/** 客户端主动断开蓝牙，并通知到Unity */
-(void)disconnectBluetoothNotifyUnity
{
    @synchronized(self)
    {
        if (nil != mConnectDevice)
        {
            NSString *advName = mConnectDevice.advName;
            [self DisConnenctBuletooth];
            [self SendUnityMsg: DicBlueConnect params:advName];
        }
    }
}
//未收到心跳断开连接
-(void) disConnectedBlueTooth:(MyPeripheral*) pPeripheral
{
    @synchronized(self)
    {
        if (mConnectDevice == pPeripheral) {
            [mCbController DisConnectDevice];
            mConnectDevice = nil;
            [self SendUnityMsg: DicBlueConnect params:pPeripheral.advName];
            
            [[NSNotificationCenter defaultCenter] postNotificationName:@"DisconnectBLE" object:nil];
        }
    }
}
//收到数据回调
-(void) onRcvData:(MyPeripheral*) pMyperipheral cmd:(Byte) cmd param:(Byte*) param lens:(int) nLen
{
    if (cmd != DV_XT) {
        Byte byte1[] = {(Byte)nLen, cmd};
        NSData* macData = [pMyperipheral.advName dataUsingEncoding:NSUTF8StringEncoding];
        if (nil != macData) {
            Byte * macBytes = (Byte *)[macData bytes];
            Byte macLen = (Byte)macData.length;
            int length = nLen + 3 + macLen;
            NSMutableData * byteData = [NSMutableData new];
            //Byte bytes[length];
            //bytes[0] = (Byte)macLen;
            [byteData appendBytes:&macLen length:1];
            [byteData appendBytes:macBytes length:macLen];
            [byteData appendBytes:byte1 length:2];
            [byteData appendBytes:param length:nLen];
            /*[self CopyBytes:macBytes SrcStart:0 Dest:bytes DestStart:1 Len:macLen];
            [self CopyBytes:byte1 SrcStart:0 Dest:bytes DestStart:1 + macLen Len:2];
            [self CopyBytes:param SrcStart:0 Dest:bytes DestStart:3 + macLen Len:nLen];*/
            NSData * data = [[NSData alloc] initWithBytes:[byteData bytes] length:length];
            //Byte* ssss = (Byte *)[byteData bytes];
            //NSLog(@"ssss ====%@", [SdkConector BytesToNSString:ssss]);
            NSString * base64Str = [[NSString alloc] initWithData:[GTMBase64 encodeData:data] encoding:NSUTF8StringEncoding];
            //NSString * base64Str = [GTMBase64 base64StringBystring:[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding]];
            [self SendUnityMsg: OnMsgAck params:base64Str];
            [byteData release];
            [data release];
            [base64Str autorelease];
            
        }
        
    }
}

-(void)CopyBytes:(Byte*)src SrcStart:(int) srcStart Dest:(Byte*) dest DestStart:(int) destStart Len:(int)len
{
    int destLen = sizeof(dest);
    int count = sizeof(src) >= srcStart + len ? srcStart + len : sizeof(src);
    for (int i = srcStart; i < count; ++i) {
        int destIndex = destStart + i - srcStart;
        if (destIndex < destLen) {
            dest[destIndex] = src[i];
        }
    }
}

//发现蓝牙设备通知
-(void)FoundDeviceNotify:(NSNotification *)notify
{
    NSDictionary * dict = [notify userInfo];
    NSString * str = (NSString *)[dict objectForKey:OnFoundDevice];
    if (nil != str) {
        [self SendUnityMsg: OnFoundDevice params:str];
    }
}

//连接蓝牙结果
-(void)ConnectBlueNotify:(NSNotification *)notify
{
    NSDictionary * dict = [notify userInfo];
    ConnectResultAck * result = (ConnectResultAck *)[dict objectForKey:ConnectResult];
    if (nil != result) {
        NSString * mac = @"";
        if (result.result) {
            mConnectDevice = result.pPeripheral;
            mac = result.pPeripheral.advName;
            [mClientMgr AddClient:result.pPeripheral ack: self];
            [mClientMgr addToDataProcessdeletegate:self];
            //[mClientMgr sendCmd:result.pPeripheral cmd:DV_HANDSHAKE param:NULL lens:0];
        }
        [self SendUnityMsg: ConnectResult params:mac];
    }
}
//拍照返回
-(void)PhotographBackNotify:(NSNotification *)notify
{
	NSDictionary * dict = [notify userInfo];
	NSString * picPath = [dict objectForKey:@"picPath"];
	if (nil != picPath)
	{
        [self SendUnityMsg: PhotographBack params:picPath];
	}
}
//进入unity
-(void)GotoUnityNotify:(NSNotification *)notify
{
	[self OpenBluetooth];
    NSDictionary * dict = [notify userInfo];
    /*NSDictionary * dict = [NSDictionary dictionaryWithObjectsAndKeys:
                           modelID, @"modelID",
                           modelName, @"modelName",
                           picPath, @"picPath",
                           modelType, @"modelType", nil];*/
    NSData* jsonData = [NSJSONSerialization dataWithJSONObject:dict options:kNilOptions error:nil];
    if (nil != jsonData)
    {
        NSString * jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        if (nil != jsonString)
        {
            [self SendUnityMsg: @"GotoScene" params:jsonString];
            [jsonString release];
        }
    }
}

//下载新模型
-(void)DownloadModelNotify:(NSNotification *)notify
{
	NSDictionary * dict = [notify userInfo];
	NSData* jsonData = [NSJSONSerialization dataWithJSONObject:dict options:kNilOptions error:nil];
    if (nil != jsonData)
    {
        NSString * jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        if (nil != jsonString)
        {
            [self SendUnityMsg: @"DownloadModel" params:jsonString];
            [jsonString release];
        }
    }
}

//修改名字
-(void)ChangeNameNotify:(NSNotification *)notify
{
	NSDictionary * dict = [notify userInfo];
	NSString * name = (NSString *)[dict objectForKey:@"modelName"];
	[self SendUnityMsg: @"ChangeRobotShowName" params:name];
}

//删除模型
-(void)DeleteModelNotify:(NSNotification *)notify
{
	NSDictionary * dict = [notify userInfo];
	NSData* jsonData = [NSJSONSerialization dataWithJSONObject:dict options:kNilOptions error:nil];
    if (nil != jsonData)
    {
        NSString * jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        if (nil != jsonString)
        {
            [self SendUnityMsg: @"DeleteModel" params:jsonString];
            [jsonString release];
        }
    }
}
//调用unity函数
-(void)CallUnityFuncNotify:(NSNotification *)notify
{
	NSDictionary * dict = [notify userInfo];
	NSData* jsonData = [NSJSONSerialization dataWithJSONObject:dict options:kNilOptions error:nil];
    if (nil != jsonData)
    {
		NSString * funcName = (NSString *)[dict objectForKey:@"funcName"];
		if (nil != funcName && [funcName isEqualToString:@"UnitySetupSteeringEngineID"]){
			[self OpenBluetooth];
		}
        NSString * jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        if (nil != jsonString)
        {
            [self SendUnityMsg: @"CallUnityFunc" params:jsonString];
            [jsonString release];
        }
    }
}

-(void)UnityBlueInitNotify:(NSNotification *)notify
{
	[[SdkConector SharedInstance] OpenBluetooth];
}


//主动断开蓝牙连接
-(void)DicConnectNotify:(NSNotification *)notify
{
    @synchronized(self) {
        NSDictionary * dict = [notify userInfo];
        NSString * mac = (NSString *)[dict objectForKey:DicBlueConnect];
        if (nil != mac) {
            if (nil != mConnectDevice && mac == mConnectDevice.advName) {
                mConnectDevice = nil;
            }
            [self SendUnityMsg: DicBlueConnect params:mac];
        }
    }
    
}

-(void) SendUnityMsg:(NSString *)methodName params:(NSString *)param
{
    if (!methodName || !param) {
        NSLog(@"SendUnityMsg error methodName = nil || param = nil");
        return;
    }
    //UnitySendMessage("MainClient", [methodName UTF8String], [param UTF8String]);
    dispatch_async(dispatch_get_main_queue(), ^{
        UnitySendMessage("MainClient", [methodName UTF8String], [param UTF8String]);
    });
}

@end



