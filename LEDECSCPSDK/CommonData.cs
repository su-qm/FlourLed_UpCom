using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GATEECSCPSDK
{
    public class CommonData
    {
        /// <summary>
        /// 公用链表存数
        /// </summary>
        public static Node node = new Node();
        /// <summary>
        /// 用户存放socket的id
        /// </summary>
        public static Node nodeSocket = new Node();
        /// <summary>
        /// 公用锁
        /// </summary>
        public static Mutex muxConsole = new Mutex();
        /// <summary>
        /// 收到的一个数据包
        /// </summary>
        public static byte[] recData = new byte[34 + 1];

    }
}
