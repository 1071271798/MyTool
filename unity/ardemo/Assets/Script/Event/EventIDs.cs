using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Game.Event
{
    public enum EventID
    {
        //蓝牙
        BLUETOOTH_OPEN,                                                           //打开蓝牙
        BLUETOOTH_CLOSE,                                                          //关闭蓝牙
        BLUETOOTH_ON_DEVICE_FOUND,                                      //当发现蓝牙设备（参数EventArg(设备名称)）
        BLUETOOTH_ON_MATCHED_DEVICE_FOUND,                     //当匹配过的设备发现
        BLUETOOTH_MATCH_RESULT,                                           //蓝牙连接成功
        Connected_Start_OutTime_Check,
        Connected_Jimu_Result,//连接jimu蓝牙成功
        //UI
        UI_EDITFRAME_ADD,                                                        //添加模型帧
        UI_EDITFRAME_MODIFY,                                                  //修改模型帧
        UI_EDITFRAME_SELECT,
        UI_EDITFRAME_Del,                  //删除模型帧数据
        UI_EDITFRAME_DelFalse,            //删除虚的
        UI_EDITFRAME_READBACK,//回读
        UI_MODELFRAME_ADD,                                                    //添加模型帧数据
        UI_MODELFRAME_MODIFY,
        UI_EDITFRAME_SELECT_For_Drag,
        UI_Save_Actions,//保存
        UI_Open_Save_Actions_Get_Name,
        UI_Create_Actions,
        UI_Open_Actions,
        UI_New_Gray, //置灰
        UI_Add_Frame_Save,//新建动作帧，点了保存
        UI_Add_Frame_Dummy, //新建动作帧，点击新建
        UI_Modify_Frame_Save,//修改动作帧，点了保存
        UI_New_Cancel,   //取消新建
        UI_Copy_Frame,//复制动作帧
        UI_Clear_Copy_Frame,//清除复制的动作帧
        UI_Paste_Frame,//粘贴复制的动作帧
        UI_Select_Frame,//选中某帧
        UI_Select_Frame_Paste,//选中某帧可粘贴
        UI_Set_Save_Actions_Btn_State,//设置保存动作按钮的状态
        UI_ShowRun_window,
        UI_Edit_swap,  //控制界面交换
        UI_Edit_rename, //控制界面重命名
        UI_Edit_deleta,  //控制界面删除
        UI_Edit_new,     //控制界面新建
        UI_MainRightbar_hide,  //主界面侧边栏显示隐藏
        ModelLoadFinish, // 模型加载完毕
        UI_EditAction_Show_btn,

        Main_Model_Load_Finished,//主页模型加载完成

        Select_Duoji_For_Robot,//通过机器人选中舵机
        Adjust_Angle_For_Robot,//通过机器人调节角度
        Select_Duoji_For_UI,//通过id选中舵机
        Adjust_Angle_For_UI,//通过id调整角度

        OpenOrClose_Duoji_ID_Change,//打开或关闭舵机id调节
        OpenOrClose_Duoji_Rota_Change,//打开或关闭舵机角度调节
        

        Set_Change_DuoJi_Data,//修改舵机信息
        Set_DuoJI_Start_ID,//设置舵机起始id

        Trigger_Robot_Box,//出发了空白区域
        Ctrl_Robot_Action,//调节机器人到某个动作
        Ctrl_Robot_Move, // 机器人仿真某个动作

        Read_Start_Rota_Ack,//读取机器人角度返回
        Read_Back_Msg_Ack,//回读返回
        Read_Back_Msg_Ack_Success,//回读成功
        Read_Back_Msg_Ack_Failed,//回读失败

        Read_Power_Msg_Ack,//获取电量数据返回
        Update_Finished,//升级完成
        Update_Error,//升级异常
        Update_Fail,//升级失败
        Update_Progress,//升级进度
        Update_Cannel,//取消升级
        Update_Wait,//等待主板升级
        
        PowerDown_ReadBack_Switch,//掉电回读状态切换

        UI_Post_Robot_Select_ID,//通过ui告诉模型旋转的舵机id

        Robot_Position_Revert,//模型位置还原

        Switch_Parent_Child_Relationship,//切换父子关系

        Set_Edit_Rota_State,//设置角度调节的状态
        Set_Edit_Time_State,//设置时间调节的状态

        Close_Set_Model_ID_UI,//关闭了设置模型id的界面
        Close_Set_Device_ID_UI,//关闭了设置设备id的界面
        Set_Device_ID_ReadData_Result,//设置设备id的界面蓝牙连接情况
        Set_Device_ID_Msg_Ack,//修改设备id回包
        Change_Sensor_ID_Msg_Ack,//修改传感器ID返回
        Quit_Edit_Actions_Scene,//退出编程界面
        Into_Set_Scene,//进入设置界面
        Cancel_Item_Select,//取消动作帧的选择
        Change_Device_ID,//修改了舵机ID

        Stop_Robot_Actions,//停止设备动作
        Stop_music_Play, //停止音乐播放

        Refresh_List,//刷新列表
        Set_Choice_Robot,//设置选中的模型
        Photograph_Back,//拍照返回
        Rename_Robot, // 模型名字修改
        Change_Robot_Name_Back,//改变机器人的名字
        SetDefaultOver, //默认动作设置完毕
        //音乐播放
        PlayWithMusic,
        
        OpenWheelModel,   //开启轮滑模式
        CloseWheelModel,  //关闭轮滑模式

        Back_Test_Scene,//返回测试场景

        //guide 
        GuideNeedWait,
        SwitchEdit, //edit console
        SwitchToogle,
        GuideShutdown,

        Exit_Blue_Connect,//退出蓝牙连接
        Blue_Connect_Finished,//蓝牙连接完成

        PopPromptMsg,
        ClosePromptMsg,

        Read_Speaker_Data_Ack,//读取喇叭数据返回

        Change_Servo_Model,//改变舵机模式
        //////////////////////////////////////////////////////////////////////////
        //拓扑图
        Item_Drag_Drop_Start,
        Item_Drag_Drop_End,

        Servo_Drag_Drop_Start,
        Servo_Drag_Drop_End,
        Servo_Press_Hold,
        Servo_Press_Hold_Recover,
        Servo_Press,

        Save_Topology_Data,
    }
}
