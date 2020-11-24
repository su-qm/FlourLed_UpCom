using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.IO;
using CommLibrarys;
using GATEECSCPSDK;

namespace PsecstoolOut
{
    public partial class FrmMain : Form
    {
        GATEECSSDK.P_GATEECS_CALLBACK_LOG p1 = (GATEECSSDK.funcLog);
        GATEECSSDK.P_GATEECS_CALLBACK_CONNECT p2 = GATEECSSDK.funcConnect;
        unsafe GATEECSSDK.P_GATEECS_CALLBACK_RECEIVE p3 = (GATEECSSDK.funcReceive);
        GATEECSSDK.P_GATEECS_CALLBACK_SENDSUC p4 = (GATEECSSDK.funcSendSuc);
        GATEECSSDK.P_GATEECS_CALLBACK_DISCONN p5 = (GATEECSSDK.funcDisconn);
        private Thread thread;

        int Quju_Index = 0;
        private Dictionary<string, string> dicChildHost;
        private string selecteditemip = "";
        private string selecteditemPort = "";
        private string selecteditemSocketId = "";
        private bool m_bCloseFrm = false;
        private int m_ReadheadNum = 0xFF;

        private int MAX_BATCHPER_NUM = 60;

        public FrmMain()
        {
            InitializeComponent();
            dicChildHost = new Dictionary<string, string>();
            ServerEventBus.MessageReceived += new ServerEventBus.MessageReceivedEvent(ServerEventBus_MessageReceived);
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            GATEECSSDK cc = new GATEECSSDK(lstLog, lstBoxDownIPS, dicChildHost, lbDeviceState);

            IPHostEntry iPHostEntry = Dns.Resolve(Dns.GetHostName());
            for (int i = 0; i < iPHostEntry.AddressList.Length; i++)
            {
                IPAddress iPAddress = iPHostEntry.AddressList[i];
                this.cmbServerIp.Items.Add(iPAddress.ToString());
            }
            if (this.cmbServerIp.Items.Count > 0)
            {
                this.cmbServerIp.SelectedIndex = 0;
            }


            this.btnStartup.Enabled = false;
            this.btnShutdown.Enabled = false;
            this.tabDeviceInfo.SelectedIndex = 1;
            //this.cmbCh1Dlsx.SelectedIndex = 0;
            //this.cmbCh2Dlsx.SelectedIndex = 1;
            //this.cmbCh3Dlsx.SelectedIndex = 2;
            //this.cmbCh4Dlsx.SelectedIndex = 3;
            this.ch1_r.Checked = true;
            this.ch2_y.Checked = true;
            this.ch3_b.Checked = true;
            this.ch4_g.Checked = true;
        

        }

        private void btnSetcallback_Click(object sender, EventArgs e)
        {
            uint i = GATEECSSDK.DT_GATEECS_SETCAllBACK(p1, p2, p3, p4, p5);
            if (0 != i)
            {
                UpdateOpraResult(String.Format("初始化错误。错误码：{0}", i));
                return;
            }

            this.btnStartup.Enabled = true;
            this.btnSetcallback.Enabled = false;
        }

        private void btnStartup_Click(object sender, EventArgs e)
        {
            if (this.cmbServerIp.Text =="")
            {
                MessageBox.Show("请选择服务器ip地址！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!RegIPAndPort.IsIPAddress(cmbServerIp.Text.Trim()))
            {
                DialogBox.Message("输入监听IP内容有误，请确认！", DialogBox.DialogType.Error);
                return;
            }
            if (!RegIPAndPort.IsIPPort(tbServerPort.Text.Trim()))
            {
                DialogBox.Message("输入监听端口号内容有误，请确认！", DialogBox.DialogType.Error);
                return;
            }

            string strIp = this.cmbServerIp.Text.Trim();
            if (IsValidIp(strIp))
            {
                uint i = GATEECSSDK.DT_GATEECS_LOGIN(this.cmbServerIp.Text, int.Parse(this.tbServerPort.Text.Trim()), 8 * 2, 2 *1000);
                if (i != 0)
                {
                    UpdateOpraResult(String.Format("初始化错误。错误码：{0}", i));
                }
            }
            else
            {
                MessageBox.Show("输入的服务器地址有误！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            this.btnStartup.Enabled = false;
            this.btnShutdown.Enabled = true;
        }

        private void btnShutdown_Click(object sender, EventArgs e)
        {
            uint socketid = 0;

            int nodeCount = CommonData.node.showNodenum();
            if (nodeCount < 1)
            {
                UpdateOpraResult("操作成功，无设备在线！");
                int j = GATEECSSDK.DT_GATEECS_SHUTDOWN();

                this.btnStartup.Enabled = true;
                this.btnShutdown.Enabled = false;
                return;
            }

            if (GetDeviceIdFromLists(ref socketid))
            {
                int i = GATEECSSDK.DT_GATEECS_DISCONNECT(uint.Parse(socketid.ToString()), 1);

                UpdateOpraResult("成功关闭所有连接！");
                int j = GATEECSSDK.DT_GATEECS_SHUTDOWN();
            }
            else
            {
                MessageBox.Show("未选中任何在线设备！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                UpdateOpraResult("操作失败");
            }

            this.btnStartup.Enabled = true;
            this.btnShutdown.Enabled = false;
        }

        private void btnFindSocketid_Click(object sender, EventArgs e)
        {
            int nodeCount = CommonData.node.showNodenum();
            if (nodeCount<1)
            {
                UpdateOpraResult("链表不存在");
                return;
            }

            int num = 0;
            bool bisfind = CommonData.node.findNode(txbSocketIP.Text, ref num);
            if (bisfind)
            {
                CommonData.muxConsole.WaitOne();
                object o = CommonData.node.showNode(num);
                object socketid = CommonData.nodeSocket.showNode(num);
                CommonData.muxConsole.ReleaseMutex();


                UpdateLable("", 0, "socketid为：" + socketid + "存在，IP为：" + o);
            }

            UpdateOpraResult("操作成功");
        }

        private void txbSocketid_TextChanged(object sender, EventArgs e)
        {
            txbSocketid.ForeColor = Color.Black;
        }

        private void txbSocketIP_TextChanged(object sender, EventArgs e)
        {
            txbSocketIP.ForeColor = Color.Black;
        }

                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          private void btnFindSocketIP_Click(object sender, EventArgs e)
        {
            int nodeCount = CommonData.node.showNodenum();
            if (nodeCount < 1)
            {
                UpdateOpraResult("链表不存在");
                return;
            }

            int num = 0;
            bool bisfind = CommonData.nodeSocket.findNode(int.Parse(txbSocketid.Text.Trim()), ref num);
            if (bisfind)
            {
                CommonData.muxConsole.WaitOne();
                object o = CommonData.node.showNode(num);
                object socketid = CommonData.nodeSocket.showNode(num);
                CommonData.muxConsole.ReleaseMutex();


                UpdateLable("", 0, String.Format("socketid为：{0}存在，IP为：{1}", socketid, o));
                UpdateOpraResult("操作成功");
            }
            else
            {
                UpdateOpraResult("操作成功, 查询无此连接");
            }

        }





        #region 操作文本框
        private delegate void UpdateRecText(string pchIP, int iPort, object msg);
        private void UpdateLable(string pchIP, int iPort, object msg)
        {
            if (lstLog.InvokeRequired)
            {
                lstLog.Invoke(new UpdateRecText(UpdateRecLable), new object[] { pchIP, iPort, msg });
            }
            else
            {
                UpdateRecLable(pchIP, iPort, msg);
            }
        }
        private void UpdateRecLable(string pchIP, int iPort, object msg)
        {
            try
            {
                lstLog.Items.Insert(0, msg + pchIP + ":" + iPort);
                if (lstLog.Items.Count > 1000)
                {
                    lstLog.Items.RemoveAt(lstLog.Items.Count - 1);
                }
            }
            catch (Exception ex)
            {
                //this.logger.Error("清理流水信息错误:" + ex);
            }
        }

        private delegate void UpdateLableText(string msg);
        private void UpdateOpraResult(string msg)
        {
            if (lbOpraResult.InvokeRequired)
            {
                lbOpraResult.Invoke(new UpdateLableText(UpdateOpraResultText), msg);
            }
            else
            {
                UpdateOpraResultText(msg);
            }
        }
        private void UpdateOpraResultText(string msg)
        {
            lbOpraResult.Text = msg;
        }


        private delegate void UpdateSeverIpText(string msg);
        private void UpdateSeverIp(string msg)
        {
            if (tbSeverIp.InvokeRequired)
            {
                tbSeverIp.Invoke(new UpdateSeverIpText(UpdatetbSeverIpText), msg);
            }
            else
            {
                UpdatetbSeverIpText(msg);
            }
        }
        private void UpdatetbSeverIpText(string msg)
        {
            tbSeverIp.Text = msg;
        }


        private delegate void UpdateSeverPortText(string msg);
        private void UpdateSeverPort(string msg)
        {
            if (tbSeverPort.InvokeRequired)
            {
                tbSeverPort.Invoke(new UpdateSeverPortText(UpdatetbSeverPortText), msg);
            }
            else
            {
                UpdatetbSeverPortText(msg);
            }
        }
        private void UpdatetbSeverPortText(string msg)
        {
            tbSeverPort.Text = msg;
        }

        private delegate void UpdateDeviceipText(string msg);
        private void UpdateDeviceip(string msg)
        {
            if (tbDeviceip.InvokeRequired)
            {
                tbDeviceip.Invoke(new UpdateDeviceipText(UpdatetbDeviceipText), msg);
            }
            else
            {
                UpdatetbDeviceipText(msg);
            }
        }
        private void UpdatetbDeviceipText(string msg)
        {
            tbDeviceip.Text = msg;
        }

        private delegate void UpdateDevicePortText(string msg);
        private void UpdateDevicePort(string msg)
        {
            if (tbDevicePort.InvokeRequired)
            {
                tbDevicePort.Invoke(new UpdateDevicePortText(UpdatetbDevicePortText), msg);
            }
            else
            {
                UpdatetbDevicePortText(msg);
            }
        }
        private void UpdatetbDevicePortText(string msg)
        {
            tbDevicePort.Text = msg;
        }

        private delegate void UpdateDeviceSubText(string msg);
        private void UpdateDeviceSub(string msg)
        {
            if (tbDeviceSub.InvokeRequired)
            {
                tbDeviceSub.Invoke(new UpdateDeviceSubText(UpdatetbDeviceSubText), msg);
            }
            else
            {
                UpdatetbDeviceSubText(msg);
            }
        }
        private void UpdatetbDeviceSubText(string msg)
        {
            tbDeviceSub.Text = msg;
        }

        private delegate void UpdateDeviceGatwayText(string msg);
        private void UpdateDeviceGatway(string msg)
        {
            if (tbDeviceGatway.InvokeRequired)
            {
                tbDeviceGatway.Invoke(new UpdateDeviceGatwayText(UpdatetbDeviceGatwayText), msg);
            }
            else
            {
                UpdatetbDeviceGatwayText(msg);
            }
        }
        private void UpdatetbDeviceGatwayText(string msg)
        {
            tbDeviceGatway.Text = msg;
        }
        
        private delegate void UpdateMACText(string msg);
        private void UpdateMAC(string msg)
        {                    
            if (tbMAC.InvokeRequired)
            {
                tbMAC.Invoke(new UpdateMACText(UpdateteMACText), msg);
            }
            else
            {
                UpdateteMACText(msg);
            }
        }
        private void UpdateteMACText(string msg)
        {
            tbMAC.Text = msg;
        }



        private delegate void UpdatePARAText(string[] splitStrs);
        private void UpdatePARA(string[] splitStrs)
        {
            if (tbChufaPinci.InvokeRequired)
            {
                this.Invoke(new UpdatePARAText(UpdatetePARAText), new object[] { splitStrs });
            }
            else
            {
                UpdatetePARAText(splitStrs);
            }
        }

        private void UpdatetePARAText(string[] splitStrs)//需要增加
        {
            tbChufaPinci.Text = splitStrs[4];
            //this.cmbCh1Dlsx.SelectedIndex = int.Parse(splitStrs[0]);
            //this.cmbCh2Dlsx.SelectedIndex = int.Parse(splitStrs[1]);
            //this.cmbCh3Dlsx.SelectedIndex = int.Parse(splitStrs[2]);
            //this.cmbCh4Dlsx.SelectedIndex = int.Parse(splitStrs[3]);

            this.tbCh1Shuchuyanshi.Text = splitStrs[5];
            this.tbCh1Faguangshijian.Text = splitStrs[6];
            //this.tbCh1Faguangyanshi.Text = splitStrs[7];
            //this.tbCh1Xjchufashijian.Text = splitStrs[8];
            this.tbCh1Xjchufashichang.Text = splitStrs[9];

            this.tbCh2Shuchuyanshi.Text = splitStrs[10];
            this.tbCh2Faguangshijian.Text = splitStrs[11];
            //this.tbCh2Faguangyanshi.Text = splitStrs[12];
            //this.tbCh2Xjchufashijian.Text = splitStrs[13];
            this.tbCh2Xjchufashichang.Text = splitStrs[14];

            this.tbCh3Shuchuyanshi.Text = splitStrs[15];
            this.tbCh3Faguangshijian.Text = splitStrs[16];
            //this.tbCh3Faguangyanshi.Text = splitStrs[17];
            //this.tbCh3Xjchufashijian.Text = splitStrs[18];
            this.tbCh3Xjchufashichang.Text = splitStrs[19];

            this.tbCh4Shuchuyanshi.Text = splitStrs[20];
            this.tbCh4Faguangshijian.Text = splitStrs[21];
            //this.tbCh4Faguangyanshi.Text = splitStrs[22];
            //this.tbCh4Xjchufashijian.Text = splitStrs[23];
            this.tbCh4Xjchufashichang.Text = splitStrs[24];
        }


#endregion

        //选择对应的设备
        private void lstBoxDownIPS_SelectedIndexChanged(object sender, EventArgs e)
        {
            string temip = lstBoxDownIPS.SelectedItem.ToString();
            int ind = temip.IndexOf(':');
            int ind2 = temip.IndexOf('(');
            selecteditemip = temip.Substring(0, ind);
            selecteditemPort = temip.Substring(ind + 1, ind2 - ind - 1);
            if (temip.Contains("在"))
            {
                selecteditemSocketId = temip.Substring(ind2 + 1, temip.IndexOf('在') - ind2 - 1);
            }
            else if (temip.Contains("离"))
            {
                selecteditemSocketId = temip.Substring(ind2 + 1, temip.IndexOf('离') - ind2 - 1);
            }
            else
            {
                selecteditemSocketId = "0";
            }
                        
            //tbDeviceip.Text = selecteditemip;
            //tbDevicePort.Text = selecteditemPort;
            tbSocketId.Text = selecteditemSocketId;
        }

        

        //设置主板信息
        private void btnSetMtiPara_Click(object sender, EventArgs e)
        {
            if (!RegIPAndPort.IsIPAddress(tbSeverIp.Text.Trim()))
            {
                DialogBox.Message("输入服务器IP内容有误，请确认！", DialogBox.DialogType.Error);
                return;
            }
            if (!RegIPAndPort.IsIPAddress(tbDeviceip.Text.Trim()))
            {
                DialogBox.Message("输入设备IP内容有误，请确认！", DialogBox.DialogType.Error);
                return;
            }
            if (!RegIPAndPort.IsIPAddress(tbDeviceSub.Text.Trim()))
            {
                DialogBox.Message("输入子网掩码内容有误，请确认！", DialogBox.DialogType.Error);
                return;
            }
            if (!RegIPAndPort.IsIPAddress(tbDeviceGatway.Text.Trim()))
            {
                DialogBox.Message("输入默认网关内容有误，请确认！", DialogBox.DialogType.Error);
                return;
            }
            if (!RegIPAndPort.IsIPPort(tbSeverPort.Text.Trim()))
            {
                DialogBox.Message("输入服务器端口号内容有误，请确认！", DialogBox.DialogType.Error);
                return;
            }
            if (!RegIPAndPort.IsIPPort(tbDevicePort.Text.Trim()))
            {
                DialogBox.Message("输入设备端口号内容有误，请确认！", DialogBox.DialogType.Error);
                return;
            }
            
            uint socketid = 0;
            if (GetDeviceIdFromLists(ref socketid))
            {
                uint s = uint.Parse(socketid.ToString());
                string ServerIP=tbSeverIp.Text;
                int ServerPort = int.Parse(tbSeverPort.Text);
                string DoorConIP = tbDeviceip.Text;
                int DoorConPort = int.Parse(tbDevicePort.Text);
                string GatewayIP = tbDeviceGatway.Text;
                string SubnetMask = tbDeviceSub.Text;

                uint i = GATEECSSDK.RCLEY_SETMULTIPARAMETER(s, ServerIP, ServerPort, DoorConIP, DoorConPort, GatewayIP, SubnetMask);

                if (0 == i)
                    UpdateOpraResult("操作成功");
                else
                    UpdateOpraResult("操作失败");
            }
            else
            {
                MessageBox.Show("未选中任何在线设备！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                UpdateOpraResult("操作失败");
            }
        }

        //读取主板信息
        private void btnGetMtiPara_Click(object sender, EventArgs e)
        {
            uint socketid = 0;
            if (GetDeviceIdFromLists(ref socketid))
            {
                uint i = GATEECSSDK.RCLEY_GETMULTIPARAMETER(uint.Parse(socketid.ToString()));

                if (0 == i)
                    UpdateOpraResult("操作成功");
                else
                    UpdateOpraResult("操作失败");
            }
            else
            {
                MessageBox.Show("未选中任何在线设备！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                UpdateOpraResult("操作失败");
            }
            
        }

        private bool GetDeviceIdFromLists(ref uint socketid)
        {
            int nodeCount = CommonData.node.showNodenum();
            if (nodeCount < 1)
            {
                UpdateOpraResult("链表不存在");
                return false;
            }

            int num = 0;
            bool bisfind = CommonData.node.findNode(selecteditemip, ref num);
            if (bisfind)
            {
                CommonData.muxConsole.WaitOne();
                object objsocketid = CommonData.nodeSocket.showNode(num);
                CommonData.muxConsole.ReleaseMutex();
                socketid = uint.Parse(objsocketid.ToString());

                return true;
            }
            return false;
        }

        private void cmbServerIp_Click(object sender, EventArgs e)
        {

        }

        private void tbServerPort_TextChanged(object sender, EventArgs e)
        {
            tbServerPort.ForeColor = Color.Black;
        }


        bool IsValidIp(string strIn)
        {
            //C#中验证IP地址的正则表达式是什么
            //((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))
            //具体如下:
            //1. 250-255：特点：三位数，百位是2，十位是5，个位是0~5，用正则表达式可以写成：25[0-5]
            //2. 200-249：特点：三位数，百位是2，十位是0~4，个位是0~9，用正则表达式可以写成：2[0-4]\d
            //3. 0-99的正则表达式可以合写为[1-9]?\d，那么0-199用正则表达式就可以写成(1\d{2})|([1-9]?\d)，
            //4. 这样0~255的正则表达式就可以写成(25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))

            //所以最终为 0-255的表达式重复三次{3}再跟一次没点的 

            //return Regex.IsMatch(strIn, @"^(25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.
                                                                                       //(25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.
                                                                                       //(25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.
                                                                                       //(25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))$");

            //return Regex.IsMatch(strIn, @"^(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5])$");
            return Regex.IsMatch(strIn, @"\d{0,3}\.\d{0,3}\.\d{0,3}\.\d{0,3}");
        }

        private void btnSetMAC_Click(object sender, EventArgs e)
        {
            uint socketid = 0;
            if (GetDeviceIdFromLists(ref socketid))
            {
                uint s = uint.Parse(socketid.ToString());
                string strmac = tbMAC.Text;//"11-22-33-44-55-66"
                uint i = GATEECSSDK.RCLEY_SETMAC(s, strmac);

                if (0 == i)
                    UpdateOpraResult("操作成功");
                else
                    UpdateOpraResult("操作失败");
            }
            else
            {
                MessageBox.Show("未选中任何在线设备！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                UpdateOpraResult("操作失败");
            }
        }

        private void btnGetMAC_Click(object sender, EventArgs e)
        {
            uint socketid = 0;
            if (GetDeviceIdFromLists(ref socketid))
            {
                uint i = GATEECSSDK.RCLEY_GETMAC(uint.Parse(socketid.ToString()));

                if (0 == i)
                    UpdateOpraResult("操作成功");
                else
                    UpdateOpraResult("操作失败");
            }
            else
            {
                MessageBox.Show("未选中任何在线设备！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                UpdateOpraResult("操作失败");
            }
        }


        private void ServerEventBus_MessageReceived(object sender, uint session, uint msgtype, uint numOfParameters, string parameters)
        {
            GATEECSSDK.UpdateLable("", 0, String.Format("收到消息，消息类型为0x{0}，参数序列为:{1}", Convert.ToString(msgtype, 16), parameters));
            string[] splitStrs= new string[30];
            bool bSplit = SplitTheParameters(numOfParameters, parameters, ref splitStrs);
            if (!bSplit)
            {
                UpdateOpraResult(String.Format("收到消息，消息不能解析。参数序列为:{0}", parameters));
                return;
            }

            #region 根据消息做相应的处理
            switch (msgtype)
            {
                case 0x70:
                    if (numOfParameters == 1 && splitStrs[0].Equals("1"))
                    {
                        UpdateOpraResult("设置设备信息成功");
                    }
                    break;
                case 0x71:
                    UpdatePARA(splitStrs);
                    break;
                case 0x72:
                case 0x73:
                    if (numOfParameters == 6)
                    {
                        UpdateOpraResult("读取设备信息成功");
                        //this.tbSeverIp.Text = splitStrs[0];
                        UpdateSeverIp(splitStrs[0]);
                        //this.tbSeverPort.Text = splitStrs[1];
                        UpdateSeverPort(splitStrs[1]);
                        //this.tbDeviceip.Text = splitStrs[2];
                        UpdateDeviceip(splitStrs[2]);
                        //this.tbDevicePort.Text = splitStrs[3];
                        UpdateDevicePort(splitStrs[3]);
                        //this.tbDeviceGatway.Text = splitStrs[4];
                        UpdateDeviceGatway(splitStrs[4]);
                        //this.tbDeviceSub.Text = splitStrs[5];
                        UpdateDeviceSub(splitStrs[5]);
                    }
                    else
                    {
                        UpdateOpraResult("返回参数错误");
                    }
                    break;
                case 0x74:
                    if (numOfParameters == 2 && splitStrs[1].Equals("1"))
                    {
                        UpdateOpraResult("操作成功！MAC为：" + splitStrs[0]);
                    }
                    else if (numOfParameters == 2 && splitStrs[1].Equals("2"))
                    {
                        UpdateOpraResult("操作失败！MAC有错误");
                    }
                    break;
                case 0x75:
                    if (numOfParameters == 1)
                    {
                        UpdateMAC(splitStrs[0]);
                        //UpdateOpraResult("操作成功！MAC为：" + splitStrs[0]);
                    }
                    else
                    {
                        UpdateOpraResult("返回参数错误");
                    }
                    break;
                case 0x76:

                    break;
                case 0x77:

                    break;
                default:
                    break;
            }
            #endregion
            
        }

        private bool SplitTheParameters(uint numOfParameters, string parameters, ref string[] outParas)
        {
            bool succ =false;
            if (!succ)
            {
                if (numOfParameters > 0)
                {
                    outParas = parameters.Split('|');
                }
                succ = true;
            }

            return succ;
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!m_bCloseFrm)
            {
                if (MessageBox.Show("确认退出？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    m_bCloseFrm = true;
                    GATEECSSDK.DT_GATEECS_SHUTDOWN();

                    Application.Exit();
                }
                else
                {
                    m_bCloseFrm = false;
                    e.Cancel = true;
                }
            }
            else
            {
                m_bCloseFrm = false;
            }

        }

        //  关闭连接
        private void btnCloseCurSocket_Click(object sender, EventArgs e)
        {
            uint socketid = 0;
            if (GetDeviceIdFromLists(ref socketid))
            {
                int i = GATEECSSDK.DT_GATEECS_DISCONNECT(uint.Parse(socketid.ToString()), 0);
                if (0 == i)
                    UpdateOpraResult("设置时间成功");
                else
                    UpdateOpraResult("操作失败");
            }
            else
            {
                MessageBox.Show("未选中任何在线设备！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                UpdateOpraResult("操作失败");
            }
        }

        //  关闭所有连接
        private void btnCloseAllSocket_Click(object sender, EventArgs e)
        {
            int i = GATEECSSDK.DT_GATEECS_DISCONNECT(0, 1);
            UpdateOpraResult("成功关闭所有连接！");
        }


        private void cmbServerIp_MouseEnter(object sender, EventArgs e)
        {
            ToolTip tooltip = new ToolTip();
            tooltip.Show("只能选择，不能手动输入，如果没有对应IP，请查看网络配置！", cmbServerIp);
        }

        private void MatchNum(TextBox txb)
        {
            string oldstring = txb.Text.Trim();
            if (oldstring.Length >= 1)
                oldstring = oldstring.Substring(0, oldstring.Length - 1);
            else
                oldstring = "";

            Regex reg = new Regex("^[0-9]*$");//验证textBox1中输入的是否为数字
            if (txb.Text != string.Empty)
            {
                if (reg.IsMatch(txb.Text.Trim()))
                {
                    //int m = int.Parse(txb.Text);
                }
                else
                {
                    DialogBox.Message("输入内容有误，请输入数字！", DialogBox.DialogType.Message);
                    txb.Text = oldstring;
                }
            }
        }

        private void tbServerPort_KeyUp(object sender, KeyEventArgs e)
        {
            MatchNum(tbServerPort);
        }

        private void tbSeverPort_KeyUp(object sender, KeyEventArgs e)
        {
            MatchNum(tbSeverPort);
        }

        private void tbDevicePort_KeyUp(object sender, KeyEventArgs e)
        {
            MatchNum(tbDevicePort);
        }

        private void btnGetAlarmpwd_Click(object sender, EventArgs e)
        {     
            uint socketid = 0;
            if (GetDeviceIdFromLists(ref socketid))
            {
                uint i = GATEECSSDK.RCLEY_GETALLPATA(uint.Parse(socketid.ToString()));

                if (0 == i)
                    UpdateOpraResult("操作成功");
                else
                    UpdateOpraResult("操作失败");
            }
            else
            {
                MessageBox.Show("未选中任何在线设备！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                UpdateOpraResult("操作失败");
            }
        }

        private void btnSetAlarmpwd_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("输入报警密码长度不合法，请输入3位数字！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

            if (!RegIPAndPort.IsHefaValue(tbCh1Shuchuyanshi.Text.Trim()) || 
                !RegIPAndPort.IsHefaValue(tbCh1Faguangshijian.Text.Trim()) || 
                //需要增加
               
                !RegIPAndPort.IsHefaValue(tbCh1Xjchufashichang.Text.Trim()) || 

                !RegIPAndPort.IsHefaValue(tbCh2Shuchuyanshi.Text.Trim()) || 
                !RegIPAndPort.IsHefaValue(tbCh2Faguangshijian.Text.Trim()) || 
                
                !RegIPAndPort.IsHefaValue(tbCh2Xjchufashichang.Text.Trim()) || 

                !RegIPAndPort.IsHefaValue(tbCh3Shuchuyanshi.Text.Trim()) || 
                !RegIPAndPort.IsHefaValue(tbCh3Faguangshijian.Text.Trim()) || 
                
                !RegIPAndPort.IsHefaValue(tbCh3Xjchufashichang.Text.Trim()) || 

                !RegIPAndPort.IsHefaValue(tbCh4Shuchuyanshi.Text.Trim()) || 
                !RegIPAndPort.IsHefaValue(tbCh4Faguangshijian.Text.Trim()) || 
               
                !RegIPAndPort.IsHefaValue(tbCh4Xjchufashichang.Text.Trim())                 )
            {
                DialogBox.Message("通道配置参数内容有误，请确认！", DialogBox.DialogType.Error);
                return;
            }




            int Ch01 = 0;//红
            int Ch02 = 0;//黄
            int Ch03 = 0;//蓝
            int Ch04 = 0;//绿


            if (ch1_r.Checked == true) Ch01 = 1;
            else  if (ch2_r.Checked == true) Ch01 = 2;
            else if (ch3_r.Checked == true) Ch01 = 3;
            else if (ch4_r.Checked == true) Ch01 = 4;

            if (ch1_y.Checked == true) Ch02 = 1;
            else if (ch2_y.Checked == true) Ch02 = 2;
            else if (ch3_y.Checked == true) Ch02 = 3;
            else if (ch4_y.Checked == true) Ch02 = 4;

            if (ch1_b.Checked == true) Ch03 = 1;
            else if (ch2_b.Checked == true) Ch03 = 2;
            else if (ch3_b.Checked == true) Ch03 = 3;
            else if (ch4_b.Checked == true) Ch03 = 4;

            if (ch1_g.Checked == true) Ch04 = 1;
            else if (ch2_g.Checked == true) Ch04 = 2;
            else if (ch3_g.Checked == true) Ch04 = 3;
            else if (ch4_g.Checked == true) Ch04 = 4;




            int pinci = int.Parse(tbChufaPinci.Text);

            int Ch11 = int.Parse(tbCh1Shuchuyanshi.Text);//通道1 输出延时
            int Ch12 = (int)(0x000000ffffff & long.Parse(tbCh1Faguangshijian.Text));//通道1 发光延时  低24位
            int Ch13 = (int)(0xffffff000000 & long.Parse(tbCh1Faguangshijian.Text)>>24);//通道1 发光延时  高24位
            int Ch14 = int.Parse(tbCh1Xjchufashichang.Text);//通道1  触发时长



            int Ch15 = int.Parse(tbCh2Shuchuyanshi.Text);//通道2 输出延时
            int Ch21 = (int)(0x000000ffffff & long.Parse(tbCh2Faguangshijian.Text));//通道2 发光延时  低24位
            int Ch22 = (int)(0xffffff000000 & long.Parse(tbCh2Faguangshijian.Text) >> 24);//通道2 发光延时  高24位
            int Ch23 = int.Parse(tbCh2Xjchufashichang.Text);//通道2  触发时长

            int Ch24 = int.Parse(tbCh3Shuchuyanshi.Text);//通道3 输出延时
            int Ch25 = (int)(0x000000ffffff & long.Parse(tbCh3Faguangshijian.Text));//通道3 发光延时  低24位
            int Ch31 = (int)(0xffffff000000 & long.Parse(tbCh3Faguangshijian.Text) >> 24);//通道3 发光延时  高24位
            int Ch32 = int.Parse(tbCh3Xjchufashichang.Text);//通道3  触发时长



            int Ch33 = int.Parse(tbCh4Shuchuyanshi.Text);//通道4 输出延时
            int Ch34 = (int)(0x000000ffffff & long.Parse(tbCh4Faguangshijian.Text));//通道4 发光延时  低24位
            int Ch35 = (int)(0xffffff000000 & long.Parse(tbCh4Faguangshijian.Text) >> 24);//通道4 发光延时  高24位
            int Ch41 = int.Parse(tbCh4Xjchufashichang.Text);//通道4  触发时长



            int Ch42 =int.Parse( p13.Text);
            int Ch43 = int.Parse(p14.Text);
            int Ch44 = 0;
            int Ch45 = 0;
            int temp = 0;

            if (Ch43 > 100) DialogBox.Message("亮度不在范围内！（0~100）", DialogBox.DialogType.Error);
            if (Ch43 < 10) Ch43 = 0;

            if (Ch01 >= Ch02)
            {
                if (Ch01 >= Ch03)
                {
                    if (Ch01 >= Ch04)  temp = Ch01;  else temp = Ch04;
                }
                else
                {
                    if (Ch03 >= Ch04) temp = Ch03; else temp = Ch04;
                }
            }
            else
            {
                if (Ch02 >= Ch03) { if (Ch02 >= Ch04) temp = Ch02; else temp = Ch04; }
                else   {if (Ch03 >= Ch04) temp = Ch03; else temp = Ch04;}    
            }
            if(temp==4)
            {
                 if ((Ch01 != temp - 1) && (Ch02 != temp - 1) && (Ch03 != temp - 1) && (Ch04 != temp - 1)) DialogBox.Message("点亮顺序有缺失！", DialogBox.DialogType.Error);
                 if ((Ch01 != temp - 2) && (Ch02 != temp - 2) && (Ch03 != temp - 2) && (Ch04 != temp - 2)) DialogBox.Message("点亮顺序有缺失！", DialogBox.DialogType.Error);
                 if ((Ch01 != temp - 3) && (Ch02 != temp - 3) && (Ch03 != temp - 3) && (Ch04 != temp - 3)) DialogBox.Message("点亮顺序有缺失！", DialogBox.DialogType.Error);
         
            }
            else if (temp == 3)
            {
                if ((Ch01 != temp - 1) && (Ch02 != temp - 1) && (Ch03 != temp - 1) && (Ch04 != temp - 1)) DialogBox.Message("点亮顺序有缺失！", DialogBox.DialogType.Error);
                if ((Ch01 != temp - 2) && (Ch02 != temp - 2) && (Ch03 != temp - 2) && (Ch04 != temp - 2)) DialogBox.Message("点亮顺序有缺失！", DialogBox.DialogType.Error);
        
            }
            else if (temp == 2)
            {
                if ((Ch01 != temp - 1) && (Ch02 != temp - 1) && (Ch03 != temp - 1) && (Ch04 != temp - 1)) DialogBox.Message("点亮顺序有缺失！", DialogBox.DialogType.Error);
             
            }
           
           // if (Ch14 + Ch15 > Ch11 + Ch12 + Ch13 ||
         //       Ch24 + Ch25 > Ch21 + Ch22 + Ch23 ||
          //      Ch34 + Ch35 > Ch31 + Ch32 + Ch33 ||
        //        Ch44 + Ch45 > Ch41 + Ch42 + Ch43)
      //      {
      //          DialogBox.Message("输入参数规则不合理，相机参数和应 <= 灯配置参数之和，请确认！", DialogBox.DialogType.Error);
               // return;
      //      }

            uint socketid = 0;
            if (GetDeviceIdFromLists(ref socketid))
            {
                uint i = GATEECSSDK.RCLEY_SETALLPATA(uint.Parse(socketid.ToString()),
                    Ch01, Ch02, Ch03, Ch04, pinci,
                    Ch11, Ch12, Ch13, Ch14, Ch15,
                    Ch21, Ch22, Ch23, Ch24, Ch25,
                    Ch31, Ch32, Ch33, Ch34, Ch35,
                    Ch41, Ch42, Ch43, Ch44, Ch45);

                if (0 == i)
                    UpdateOpraResult("操作成功");
                else
                    UpdateOpraResult("操作失败");
            }
            else
            {
                MessageBox.Show("未选中任何在线设备！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                UpdateOpraResult("操作失败");
            }
        }


        private void tbMAC_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSetDeviceTime_Click(object sender, EventArgs e)
        {
            uint socketid = 0;
            if (GetDeviceIdFromLists(ref socketid))
            {
                uint i = GATEECSSDK.RCLEY_SETDEVICETIME(uint.Parse(socketid.ToString()), DateTimeEx.GetTicks(DateTime.Now));

                if (0 == i)
                    UpdateOpraResult("设置时间成功");
                else
                    UpdateOpraResult("操作失败");
            }
            else
            {
                MessageBox.Show("未选中任何在线设备！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                UpdateOpraResult("操作失败");
            }
        }

        private void btnGetDeviceTime_Click(object sender, EventArgs e)
        {
            uint socketid = 0;
            if (GetDeviceIdFromLists(ref socketid))
            {
                uint i = GATEECSSDK.RCLEY_GETDEVICETIME(uint.Parse(socketid.ToString()));

                if (0 == i)
                    UpdateOpraResult("读取时间成功");
                else
                    UpdateOpraResult("操作失败");
            }
            else
            {
                MessageBox.Show("未选中任何在线设备！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                UpdateOpraResult("操作失败");
            }
        }

        private void btnClearOfflineDev_Click(object sender, EventArgs e)
        {
            GATEECSSDK.DeleteLstDev();
            string soutlinedev = lbDeviceState.Text;
            soutlinedev = soutlinedev.Substring(0, soutlinedev.Length - 3) + "000";
            GATEECSSDK.UpdatelbDeviceState(soutlinedev);
        }

        private void btnClearLstLog_Click(object sender, EventArgs e)
        {
            GATEECSSDK.DeleteLstLog();
        }

        private void tbCh1Shuchuyanshi_MouseEnter(object sender, EventArgs e)
        {
            ToolTip ttpSettings = new ToolTip();
            ttpSettings.InitialDelay = 200;
            ttpSettings.AutoPopDelay = 10 * 1000;
            ttpSettings.ReshowDelay = 200;
            ttpSettings.ShowAlways = true;
            ttpSettings.IsBalloon = true;

            string tipOverwrite = "范围：0-16777215。单位：us";
            ttpSettings.SetToolTip(tbCh1Shuchuyanshi, tipOverwrite);
        }

        private void tbCh1Faguangshijian_MouseEnter(object sender, EventArgs e)
        {
            ToolTip ttpSettings = new ToolTip();
            ttpSettings.InitialDelay = 200;
            ttpSettings.AutoPopDelay = 10 * 1000;
            ttpSettings.ReshowDelay = 200;
            ttpSettings.ShowAlways = true;
            ttpSettings.IsBalloon = true;

            string tipOverwrite = "范围：0-16777215。单位：0.1us";
            ttpSettings.SetToolTip(tbCh1Faguangshijian, tipOverwrite);
        }

 

        private void tbCh2Shuchuyanshi_MouseEnter(object sender, EventArgs e)
        {
            ToolTip ttpSettings = new ToolTip();
            ttpSettings.InitialDelay = 200;
            ttpSettings.AutoPopDelay = 10 * 1000;
            ttpSettings.ReshowDelay = 200;
            ttpSettings.ShowAlways = true;
            ttpSettings.IsBalloon = true;

            string tipOverwrite = "范围：0-16777215。单位：us";
            ttpSettings.SetToolTip(tbCh2Shuchuyanshi, tipOverwrite);
        }

        private void tbCh3Shuchuyanshi_MouseEnter(object sender, EventArgs e)
        {
            ToolTip ttpSettings = new ToolTip();
            ttpSettings.InitialDelay = 200;
            ttpSettings.AutoPopDelay = 10 * 1000;
            ttpSettings.ReshowDelay = 200;
            ttpSettings.ShowAlways = true;
            ttpSettings.IsBalloon = true;

            string tipOverwrite = "范围：0-16777215。单位：us";
            ttpSettings.SetToolTip(tbCh3Shuchuyanshi, tipOverwrite);
        }

        private void tbCh4Shuchuyanshi_MouseEnter(object sender, EventArgs e)
        {
            ToolTip ttpSettings = new ToolTip();
            ttpSettings.InitialDelay = 200;
            ttpSettings.AutoPopDelay = 10 * 1000;
            ttpSettings.ReshowDelay = 200;
            ttpSettings.ShowAlways = true;
            ttpSettings.IsBalloon = true;

            string tipOverwrite = "范围：0-16777215。单位：us";
            ttpSettings.SetToolTip(tbCh4Shuchuyanshi, tipOverwrite);
        }

        private void tbCh2Faguangshijian_MouseEnter(object sender, EventArgs e)
        {
            ToolTip ttpSettings = new ToolTip();
            ttpSettings.InitialDelay = 200;
            ttpSettings.AutoPopDelay = 10 * 1000;
            ttpSettings.ReshowDelay = 200;
            ttpSettings.ShowAlways = true;
            ttpSettings.IsBalloon = true;

            string tipOverwrite = "范围：0-16777215。单位：0.1us";
            ttpSettings.SetToolTip(tbCh2Faguangshijian, tipOverwrite);
        }

        private void tbCh3Faguangshijian_MouseEnter(object sender, EventArgs e)
        {
            ToolTip ttpSettings = new ToolTip();
            ttpSettings.InitialDelay = 200;
            ttpSettings.AutoPopDelay = 10 * 1000;
            ttpSettings.ReshowDelay = 200;
            ttpSettings.ShowAlways = true;
            ttpSettings.IsBalloon = true;

            string tipOverwrite = "范围：0-16777215。单位：0.1us";
            ttpSettings.SetToolTip(tbCh3Faguangshijian, tipOverwrite);
        }

        private void tbCh4Faguangshijian_MouseEnter(object sender, EventArgs e)
        {
            ToolTip ttpSettings = new ToolTip();
            ttpSettings.InitialDelay = 200;
            ttpSettings.AutoPopDelay = 10 * 1000;
            ttpSettings.ReshowDelay = 200;
            ttpSettings.ShowAlways = true;
            ttpSettings.IsBalloon = true;

            string tipOverwrite = "范围：0-16777215。单位：0.1us";
            ttpSettings.SetToolTip(tbCh4Faguangshijian, tipOverwrite);
        }

     


        private void tbChufaPinci_MouseEnter(object sender, EventArgs e)
        {
            ToolTip ttpSettings = new ToolTip();
            ttpSettings.InitialDelay = 200;
            ttpSettings.AutoPopDelay = 10 * 1000;
            ttpSettings.ReshowDelay = 200;
            ttpSettings.ShowAlways = true;
            ttpSettings.IsBalloon = true;

            string tipOverwrite = "说明：0为连续，最大值65535";
            ttpSettings.SetToolTip(tbChufaPinci, tipOverwrite);
        }

        private void lstLog_MouseClick(object sender, MouseEventArgs e)
        {
            if (null == lstLog.SelectedItem)
            {
                return;
            }
            Clipboard.SetDataObject(lstLog.SelectedItem.ToString());
        }

        private void btnAction_Click(object sender, EventArgs e)
        {
            uint socketid = 0;
            if (GetDeviceIdFromLists(ref socketid))
            {
                uint s = uint.Parse(socketid.ToString());
                uint i = GATEECSSDK.RCLEY_NOWACTION(s);

                if (0 == i)
                    UpdateOpraResult("操作成功");
                else
                    UpdateOpraResult("操作失败");
            }
            else
            {
                MessageBox.Show("未选中任何在线设备！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                UpdateOpraResult("操作失败");
            }
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void frequency_TextChanged(object sender, EventArgs e)
        {
            int temp =1;
            int sum, result;
            try
            {
                if (temp < 100 & temp >= 1)
                {
                    temp = Convert.ToInt32(frequency.Text);
                  
                    sum = Convert.ToInt32(tbCh1Shuchuyanshi.Text) + Convert.ToInt32(tbCh1Faguangshijian.Text) + + Convert.ToInt32(tbCh1Xjchufashichang.Text) +
                                  Convert.ToInt32(tbCh2Shuchuyanshi.Text) + Convert.ToInt32(tbCh2Faguangshijian.Text)   + Convert.ToInt32(tbCh2Xjchufashichang.Text) +
                                  Convert.ToInt32(tbCh3Shuchuyanshi.Text) + Convert.ToInt32(tbCh3Faguangshijian.Text)  + Convert.ToInt32(tbCh3Xjchufashichang.Text) +
                                  Convert.ToInt32(tbCh4Shuchuyanshi.Text) + Convert.ToInt32(tbCh4Faguangshijian.Text) +  Convert.ToInt32(tbCh4Xjchufashichang.Text);
                    result = ((1000000 / temp)-sum);
                    result = result / 4;

              //      this.tbCh1Xjchufashijian.Text = result.ToString();
               //     this.tbCh2Xjchufashijian.Text = result.ToString();
               //     this.tbCh3Xjchufashijian.Text = result.ToString();
               //     this.tbCh4Xjchufashijian.Text = result.ToString();
                }
            }

            catch
            {
 
            }
           
        }

      

        private void ch1_r_CheckedChanged(object sender, EventArgs e)
        {
            if (ch1_r.Checked == true)
            {
                ch2_r.Enabled = false;
                ch3_r.Enabled = false;
                ch4_r.Enabled = false;
            }

            else
            {
                ch2_r.Enabled = true;
                ch3_r.Enabled = true;
                ch4_r.Enabled = true;
            }
        }

        private void ch2_r_CheckedChanged(object sender, EventArgs e)
        {
            if (ch2_r.Checked == true)
            {
                ch1_r.Enabled = false;
                ch3_r.Enabled = false;
                ch4_r.Enabled = false;
            }

            else
            {
                ch1_r.Enabled = true;
                ch3_r.Enabled = true;
                ch4_r.Enabled = true;
            }
        }

        private void ch3_r_CheckedChanged(object sender, EventArgs e)
        {
            if (ch3_r.Checked == true)
            {
                ch1_r.Enabled = false;
                ch2_r.Enabled = false;
                ch4_r.Enabled = false;
            }

            else
            {
                ch1_r.Enabled = true;
                ch2_r.Enabled = true;
                ch4_r.Enabled = true;
            }
        }

        private void ch4_r_CheckedChanged(object sender, EventArgs e)
        {
            if (ch4_r.Checked == true)
            {
                ch1_r.Enabled = false;
                ch2_r.Enabled = false;
                ch3_r.Enabled = false;
            }

            else
            {
                ch1_r.Enabled = true;
                ch2_r.Enabled = true;
                ch3_r.Enabled = true;
            }
        }

        private void ch1_y_CheckedChanged(object sender, EventArgs e)
        {
            if (ch1_y.Checked == true)
            {
                ch2_y.Enabled = false;
                ch3_y.Enabled = false;
                ch4_y.Enabled = false;
            }

            else
            {
                ch2_y.Enabled = true;
                ch3_y.Enabled = true;
                ch4_y.Enabled = true;
            }
        }

        private void ch2_y_CheckedChanged(object sender, EventArgs e)
        {
            if (ch2_y.Checked == true)
            {
                ch1_y.Enabled = false;
                ch3_y.Enabled = false;
                ch4_y.Enabled = false;
            }

            else
            {
                ch1_y.Enabled = true;
                ch3_y.Enabled = true;
                ch4_y.Enabled = true;
            }
        }

        private void ch3_y_CheckedChanged(object sender, EventArgs e)
        {
            if (ch3_y.Checked == true)
            {
                ch1_y.Enabled = false;
                ch2_y.Enabled = false;
                ch4_y.Enabled = false;
            }

            else
            {
                ch1_y.Enabled = true;
                ch2_y.Enabled = true;
                ch4_y.Enabled = true;
            }
        }

        private void ch4_y_CheckedChanged(object sender, EventArgs e)
        {
            if (ch4_y.Checked == true)
            {
                ch1_y.Enabled = false;
                ch2_y.Enabled = false;
                ch3_y.Enabled = false;
            }

            else
            {
                ch1_y.Enabled = true;
                ch2_y.Enabled = true;
                ch3_y.Enabled = true;
            }
        }

        private void ch1_b_CheckedChanged(object sender, EventArgs e)
        {
            if (ch1_b.Checked == true)
            {
                ch2_b.Enabled = false;
                ch3_b.Enabled = false;
                ch4_b.Enabled = false;
            }

            else
            {
                ch2_b.Enabled = true;
                ch3_b.Enabled = true;
                ch4_b.Enabled = true;
            }
        }

        private void ch2_b_CheckedChanged(object sender, EventArgs e)
        {
            if (ch2_b.Checked == true)
            {
                ch1_b.Enabled = false;
                ch3_b.Enabled = false;
                ch4_b.Enabled = false;
            }

            else
            {
                ch1_b.Enabled = true;
                ch3_b.Enabled = true;
                ch4_b.Enabled = true;
            }
        }

        private void ch3_b_CheckedChanged(object sender, EventArgs e)
        {
            if (ch3_b.Checked == true)
            {
                ch1_b.Enabled = false;
                ch2_b.Enabled = false;
                ch4_b.Enabled = false;
            }

            else
            {
                ch1_b.Enabled = true;
                ch2_b.Enabled = true;
                ch4_b.Enabled = true;
            }
        }

        private void ch4_b_CheckedChanged(object sender, EventArgs e)
        {
            if (ch4_b.Checked == true)
            {
                ch1_b.Enabled = false;
                ch2_b.Enabled = false;
                ch3_b.Enabled = false;
            }

            else
            {
                ch1_b.Enabled = true;
                ch2_b.Enabled = true;
                ch3_b.Enabled = true;
            }

        }

        private void ch1_g_CheckedChanged(object sender, EventArgs e)
        {
            if (ch1_g.Checked == true)
            {
                ch2_g.Enabled = false;
                ch3_g.Enabled = false;
                ch4_g.Enabled = false;
            }

            else
            {
                ch2_g.Enabled = true;
                ch3_g.Enabled = true;
                ch4_g.Enabled = true;
            }
        }

        private void ch2_g_CheckedChanged(object sender, EventArgs e)
        {
            if (ch2_g.Checked == true)
            {
                ch1_g.Enabled = false;
                ch3_g.Enabled = false;
                ch4_g.Enabled = false;
            }

            else
            {
                ch1_g.Enabled = true;
                ch3_g.Enabled = true;
                ch4_g.Enabled = true;
            }
        }

        private void ch3_g_CheckedChanged(object sender, EventArgs e)
        {
            if (ch3_g.Checked == true)
            {
                ch1_g.Enabled = false;
                ch2_g.Enabled = false;
                ch4_g.Enabled = false;
            }

            else
            {
                ch1_g.Enabled = true;
                ch2_g.Enabled = true;
                ch4_g.Enabled = true;
            }
        }

        private void ch4_g_CheckedChanged(object sender, EventArgs e)
        {
            if (ch4_g.Checked == true)
            {
                ch1_g.Enabled = false;
                ch2_g.Enabled = false;
                ch3_g.Enabled = false;
            }

            else
            {
                ch1_g.Enabled = true;
                ch2_g.Enabled = true;
                ch3_g.Enabled = true;
            }
        }

  

      





  

     


    }
}
