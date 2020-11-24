using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace GATEECSCPSDK
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PERMISSIONINFO
    {
        //public int PermissionIndex;			//上位机权限索引，保留;
        public int CardID;							//卡号;
        //public int AreaCode;						//区域;
        public int DoorNum;					//门通道号;
        public long BegDay;				//年月日
        public long EndDay;				//年月日，如："2025-07-08";
        public int PeriodIndex;					//权限时段索引号;
        public int Password;			//密码，<=6的数字;
        public int ActivePassword;			//1启用密码，0不启用密码;
        public int StartStop;                 //1停用此权限，0使用此权限
    }

    // OSINFO定义 
    [StructLayout(LayoutKind.Sequential)]
    public struct OSINFO
    {
        public uint osVersion;
        public uint majorVersion;
        public uint minorVersion;
        public uint buildNum;
        public uint platFormId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szVersion;
    } 



    public class GATEECSSDK
    {
        #region 声明变量
        private static ListBox lbdev;
        private static ListBox lsbLog;
        private static Dictionary<string, string> dicChildHost;
        private static Label lbDeviceno;

#endregion

        #region 构造函数
        public GATEECSSDK(ListBox listboxlog, ListBox listboxdevice, Dictionary<string, string> dicHost, Label deviceno)
        {
            lsbLog = listboxlog;
            lbdev = listboxdevice;
            dicChildHost = dicHost;
            lbDeviceno = deviceno;
        }

        public GATEECSSDK()
        {

        }

#endregion

        #region 加载SDK

        #region 基础加载
        /// <summary>
        /// 初始化模块
        /// </summary>
        /// <param name="iPort">指定需要监听的端口</param>
        /// <param name="iWorkerThreadCount">指定工作者线程数（推荐CPU个数*2）</param>
        /// <param name="dwTimeOut">指定SOCKET超时时间（ms）</param>
        /// <returns>错误码</returns>
        [DllImport("GATEECS.dll")]
        public static extern uint DT_GATEECS_LOGIN(string sIp, int iPort, int iWorkerThreadCount, int dwTimeOut);
        /// <summary>
        /// 获取连接数
        /// </summary>
        /// <param name="piClientCount">连接数变量地址</param>
        /// <returns>错误码</returns>
        [DllImport("GATEECS.dll")]
        unsafe public static extern uint DT_GATEECS_GETCLIENTCOUNT(int* piClientCount);
        /// <summary>
        /// 向指定socket发送数据
        /// </summary>
        /// <param name="s">socket句柄</param>
        /// <param name="pch">发送包起始地址</param>
        /// <param name="len">错误码</param>
        /// <returns></returns>
        [DllImport(("GATEECS.dll"))]
        unsafe public static extern int DT_GATEECS_SEND(uint s, byte[] pch, uint len);
        /// <summary>
        /// 向所有socket发送数据
        /// </summary>
        /// <param name="pch">发送包起始地址</param>
        /// <param name="len">发送包长度（MAX 1024 Bytes）</param>  
        /// <returns>错误码</returns>
        [DllImport("GATEECS.dll")]
        unsafe public static extern uint DT_GATEECS_SEND2ALL(string pch, uint len);
        /// <summary>
        /// 获取最后建立连接的socket句柄
        /// </summary>
        /// <param name="ps">SOCKET句柄地址</param>
        /// <returns>错误码</returns>
        [DllImport("GATEECS.dll")]
        unsafe public static extern int DT_GATEECS_GETLAST(uint* ps);
        /// <summary>
        /// 卸载模块
        /// </summary>
        /// <returns>错误码</returns>
        [DllImport("GATEECS.dll")]
        public static extern int DT_GATEECS_SHUTDOWN();

        /// <summary>
        /// 断开指定SOCKET或所有SOCKET，iAll非0则断开所有SOCKET
        /// </summary>
        /// <returns></returns>
        [DllImport("GATEECS.dll")]
        public static extern int DT_GATEECS_DISCONNECT(uint s, int iALL);

        /// <summary>
        /// 设置回调函数
        /// </summary>
        /// <param name="funcLog">模块产生日志处理</param>
        /// <param name="funcConnect">当建立连接时</param>
        /// <param name="funcReceive">当得到数据时</param>
        /// <param name="funcSendSuc">当发送数据成功时</param>
        /// <param name="funcDisconn">当断开连接时</param>
        /// <returns>错误码</returns>
        [DllImport("GATEECS.dll")]
        public static extern uint DT_GATEECS_SETCAllBACK(P_GATEECS_CALLBACK_LOG funcLog,
                                                       P_GATEECS_CALLBACK_CONNECT funcConnect,
                                                       P_GATEECS_CALLBACK_RECEIVE funcReceive,
                                                       P_GATEECS_CALLBACK_SENDSUC funcSendSuc,
                                                       P_GATEECS_CALLBACK_DISCONN funcDisconn);

        /// <summary>
        /// 回调函数日志
        /// </summary>
        /// <param name="pch">日志文件</param>
        /// <param name="len">日志长度</param>
        unsafe public delegate void P_GATEECS_CALLBACK_LOG(string pch, uint len);
        /// <summary>
        /// 链接
        /// </summary>
        /// <param name="s">socket</param>
        /// <param name="pchIP">socketip地址</param>
        /// <param name="iPort">socket端口</param>
        public delegate void P_GATEECS_CALLBACK_CONNECT(uint s, string pchIP, int iPort);
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="s">socket</param>
        /// <param name="pData">接收到的数据</param>
        /// <param name="len">长度</param>
        unsafe public delegate void P_GATEECS_CALLBACK_RECEIVE(uint s, uint msgtype, uint numOfParameters, string parameters);
        //unsafe public delegate void P_GATEECS_CALLBACK_RECEIVE(uint s, byte* pData, uint len, uint msgtype, uint numOfParameters, string parameters);
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="s">socket</param>
        /// <param name="len">长度</param>
        public delegate void P_GATEECS_CALLBACK_SENDSUC(uint s, uint len);
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="s">socket</param>
        /// <param name="pchIP">socketip地址</param>
        /// <param name="iPort">socket端口</param>
        public delegate void P_GATEECS_CALLBACK_DISCONN(uint s, string pchIP, int iPort);
#endregion

        #region 扩展功能1

        /// <summary>
        /// 设置设备参数信息
        /// </summary>
        /// <param name="s">句柄</param>
        /// <param name="ServerIP">服务器IP</param>
        /// <param name="ServerPort">服务器的监听端口</param>
        /// <param name="DoorConIP">设备IP</param>
        /// <param name="DoorConPort">设备连接端口</param>
        /// <param name="GatewayIP">网关</param>
        /// <param name="SubnetMask">子网掩码</param>
        /// <returns></returns>
        [DllImport("GATEECS.dll")]
        public static extern uint RCLEY_SETMULTIPARAMETER(uint s, string ServerIP, int ServerPort, string DoorConIP, int DoorConPort, string GatewayIP, string SubnetMask);

        /// <summary>
        /// 读取设备信息
        /// </summary>
        /// <returns>错误码</returns>
        [DllImport("GATEECS.dll")]
        public static extern uint RCLEY_GETMULTIPARAMETER(uint s);

        /// <summary>
        /// 设置设备网络物理地址
        /// </summary>
        /// <param name="s">句柄</param>
        /// <param name="strmac">MAC号</param>
        /// <returns></returns>
        [DllImport("GATEECS.dll")]
        public static extern uint RCLEY_SETMAC(uint s, string strmac);

        /// <summary>
        /// 读取设备网络物理地址
        /// </summary>
        /// <param name="s">句柄</param>
        /// <returns>错误码</returns>
        [DllImport("GATEECS.dll")]
        public static extern uint RCLEY_GETMAC(uint s);

        /// <summary>
        /// 设置设备时间
        /// </summary>
        /// <param name="s">句柄</param>
        /// <param name="lTime">时间<param>
        /// <returns></returns>
        [DllImport("GATEECS.dll")]
        public static extern uint RCLEY_SETDEVICETIME(uint s, long lTime);

        /// <summary>
        /// 设置所有设备时间
        /// </summary>
        /// <param name="s">句柄</param>
        /// <param name="lTime">时间<param>
        /// <returns></returns>
        [DllImport("GATEECS.dll")]
        public static extern uint RCLEY_SETALLDEVICETIME(long lTime);

        /// <summary>
        /// 读取设备时间
        /// </summary>
        /// <param name="s">句柄</param>
        /// <returns>错误码</returns>
        [DllImport("GATEECS.dll")]
        public static extern uint RCLEY_GETDEVICETIME(uint s);



        [DllImport("GATEECS.dll")]
        public static extern uint RCLEY_SETALLPATA(uint s,
            int Ch01, int Ch02, int Ch03, int Ch04, int pinci,
            int Ch11, int Ch12, int Ch13, int Ch14, int Ch15,
            int Ch21, int Ch22, int Ch23, int Ch24, int Ch25,
            int Ch31, int Ch32, int Ch33, int Ch34, int Ch35,
            int Ch41, int Ch42, int Ch43, int Ch44, int Ch45);

        [DllImport("GATEECS.dll")]
        public static extern uint RCLEY_GETALLPATA(uint s);

        /// <summary>
        /// 立即执行
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        [DllImport("GATEECS.dll")]
        public static extern uint RCLEY_NOWACTION(uint s);



                
        /// <summary>
        /// 下位机测试权限
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        [DllImport("GATEECS.dll")]
        public static extern uint RCLEY_TESTPER(uint s);

        [DllImport("GATEECS.dll")]
        public static extern uint RCLEY_TESTPERGet(uint s,int num);
        
        #endregion

#endregion
      
        #region 回调函数
        unsafe public static void funcLog(string s, uint len)
        {
            if (len == 0)
            {
                return;
            }
            UpdateLable("", 0, s);
            //lb.Items.Add(s);
        }

        public static void funcConnect(uint s, string pchIP, int iPort)
        {
            UpdateLable(pchIP, iPort, "建立链接");

            //dicChildHost[pchIP + ":" + iPort] = pchIP + ":" + iPort + "(在线)";
            //dicChildHost[pchIP + ":" + iPort + s.ToString()] = pchIP + ":" + iPort + "(" + s + "在线)";
            dicChildHost[s.ToString()] = pchIP + ":" + iPort + "(" + s + "在线)";
            UpdateDevs(dicChildHost.Values.ToList());


            CommonData.muxConsole.WaitOne();
            CommonData.node.insertNode(pchIP);
            CommonData.nodeSocket.insertNode(s);
            CommonData.muxConsole.ReleaseMutex();

            int nodeCount = CommonData.node.showNodenum();
            string snodeCount = "000" + nodeCount;
            snodeCount = snodeCount.Substring(snodeCount.Length - 3);
            int outlinedev = dicChildHost.Count - nodeCount;
            string soutlinedev = "000" + outlinedev;
            soutlinedev = soutlinedev.Substring(soutlinedev.Length - 3);
            UpdatelbDeviceState(String.Format("在线设备:{0},  离线设备:{1}", snodeCount, soutlinedev));
        }
        
        unsafe public static void funcReceive(uint s,  uint msgtype, uint numOfParameters, string parameters)
        {
            ServerEventBus.GetInstance().OnMessageReceive(s, msgtype, numOfParameters, parameters);
        }

        public static void funcSendSuc(uint s, uint len)
        {

        }

        public static void funcDisconn(uint s, string pchIP, int iPort)
        {
            UpdateLable(pchIP, iPort, "断开链接" + s);

            //dicChildHost[pchIP + ":" + iPort] = pchIP + ":" + iPort + "(离线)";
            //dicChildHost[pchIP + ":" + iPort + s.ToString()] = pchIP + ":" + iPort + "(" + s + "离线)";
            dicChildHost[s.ToString()] = pchIP + ":" + iPort + "(" + s + "离线)";
            UpdateDevs(dicChildHost.Values.ToList());


            int nodeCount = CommonData.node.showNodenum();
            if (nodeCount < 1)
                return;

            int num = 0;
            bool bisfind = CommonData.nodeSocket.findNode(s, ref num);
            if (bisfind)
            {
                CommonData.muxConsole.WaitOne();
                CommonData.node.delNode(num);
                CommonData.nodeSocket.delNode(num);
                CommonData.muxConsole.ReleaseMutex();
            }

            nodeCount = CommonData.node.showNodenum();
            string snodeCount = "000" + nodeCount;
            snodeCount = snodeCount.Substring(snodeCount.Length - 3);
            int outlinedev = dicChildHost.Count - nodeCount;
            string soutlinedev = "000" + outlinedev;
            soutlinedev = soutlinedev.Substring(soutlinedev.Length - 3);
            UpdatelbDeviceState(String.Format("在线设备:{0},  离线设备:{1}", snodeCount, soutlinedev));
        }
#endregion

        public static void DeleteLstDev()
        {
            List<string> test = new List<string>(dicChildHost.Keys);
            for (int i = dicChildHost.Count - 1; i >= 0; i--)
            {
                if (dicChildHost[test[i]].ToString().Contains("离线"))
                {
                    dicChildHost.Remove(test[i]);
                }
            }

            //dicChildHost[s.ToString()] = pchIP + ":" + iPort + "(" + s + "离线)";
            UpdateDevs(dicChildHost.Values.ToList());
        }


        public static void DeleteLstLog()
        {
            try
            {
                if (lsbLog.Items.Count <= 12)
                    return;

                for (int i = lsbLog.Items.Count -12-1; i >=0 ; i--)
                {
                    lsbLog.Items.RemoveAt(i);
                }
            }
            catch (Exception ex)
            {
                //this.logger.Error("清理流水信息错误:" + ex);
            }
        }


        #region 更新消息
        private delegate void UpdateRecText(string pchIP, int iPort, object msg);
        public static void UpdateLable(string pchIP, int iPort, object msg)
        {
            if (lsbLog.InvokeRequired)
            {
                lsbLog.Invoke(new UpdateRecText(UpdateRecLable), new object[] { pchIP, iPort, msg });
            }
            else
            {
                UpdateRecLable(pchIP, iPort, msg);
            }
        }
        private static void UpdateRecLable(string pchIP, int iPort, object msg)
        {
            try
            {
                //lb.Items.Add(msg + pchIP + ":" + iPort);
                if (0 == iPort)
                {
                    lsbLog.Items.Insert(0, msg);
                }
                else
                {
                    lsbLog.Items.Insert(0, msg + pchIP + ":" + iPort);
                }

                if (lsbLog.Items.Count>200)
                {
                    //object obj = lsbLog.Items.Count - 1;
                    lsbLog.Items.RemoveAt(lsbLog.Items.Count-15-1);
                    //obj
                }
            }
            catch (Exception ex)
            {
                //this.logger.Error("清理流水信息错误:" + ex);
            }
        }

        private delegate void UpdateDownIPLstBoxText(object msg);
        private static void UpdateDevs(object msg)
        {
            if (lbdev.InvokeRequired)
            {
                lbdev.Invoke(new UpdateDownIPLstBoxText(UpdateDevsBox), msg);
            }
            else
            {
                UpdateDevsBox(msg);
            }
        }
        private static void UpdateDevsBox(object msg)
        {
            lbdev.DataSource = msg;
        }


        private delegate void UpdatelbDeviceStatedelegate(string msg);
        public static void UpdatelbDeviceState(string msg)
        {
            if (lbDeviceno.InvokeRequired)
            {
                lbDeviceno.Invoke(new UpdatelbDeviceStatedelegate(UpdateDeviceState), msg);
            }
            else
            {
                UpdateDeviceState(msg);
            }
        }

        private static void UpdateDeviceState(string msg)
        {
            lbDeviceno.Text = msg;
        }
#endregion

        #region 获取session的IP

        public static string GetIpBySession(uint session)
        {
            //string strip = "";
            int num = 0;
            bool bisfind = CommonData.nodeSocket.findNode(session, ref num);
            if (bisfind)
            {
                //CommonData.muxConsole.WaitOne();
                //CommonData.node.delNode(num);
                //CommonData.nodeSocket.delNode(num);
                //CommonData.muxConsole.ReleaseMutex();
                return CommonData.node.GetNode(num).item.ToString();
            }
            return "";
        }

        #endregion 获取session的IP

    }
}