using System;
using System.Collections.Generic;
using System.Text;

namespace GATEECSCPSDK
{
    public class Node
    {
        public object item;
        public Node next;
        private Node head;
        private int index;

        #region 构造函数
        public Node()
        {
            head = new Node("head");
            index = 0;
        }
        public Node(object v)
        {
            item = v; next = null;
        }
        /**/
        /// <summary>
        /// 可以自定义开始头节点
        /// </summary>
        public void headNode()
        {
            head = new Node("head");
            index = 0;
        }
        #endregion

        #region 插入
        /**/
        /// <summary>
        /// 从后面插入
        /// </summary>
        /// <param name="ob">要插入的数据</param>
        public void insertNode(object ob)//从后面插入
        {
            Node x = head;
            Node t = new Node(ob);
            if (ob == null)
            {
                return;
            }
            for (int i = 0; i < index; i++)
                x = x.next;

            t.next = x.next;
            x.next = t;

            index = index + 1;
        }
        /**/
        /// <summary>
        /// 指定插入(插入指定参数的下面)l从0开始插入第一个的前面
        /// </summary>
        /// <param name="ob">要插入的数据</param>
        /// <param name="l">要插入的数据的位置</param>
        /// <returns>true为插入成功，反之失败</returns>
        public bool insertNode(object ob, int l)//指定插入(插入指定参数的下面)l从0开始插入第一个的前面
        {
            if ((l >= 0) && (l <= index))
            {
                Node x = head;
                for (int i = 0; i < l; i++)
                    x = x.next;

                Node t = new Node(ob);
                t.next = x.next;
                x.next = t;

                index = index + 1;
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 删除
        /**/
        /// <summary>
        /// 删除最后一个
        /// </summary>
        public void delNode()//删除最后一个
        {
            Node x = head;
            for (int i = 0; i < index - 1; i++)
                x = x.next;
            x.next = x.next.next;
            index = index - 1;
        }
        /**/
        /// <summary>
        /// 指定删除(l从1开始)
        /// </summary>
        /// <param name="l">指定删除位置</param>
        /// <returns>true删除成功，反之失败</returns>
        public bool delNode(int l)//指定删除l从1开始
        {
            if ((l > 0) && (l <= index))
            {
                Node x = head;
                for (int i = 0; i < l - 1; i++)
                    x = x.next;
                x.next = x.next.next;
                index = index - 1;
                return true;
            }
            else
            {
                return false;
            }
        }
        /**/
        /// <summary>
        /// 查找删除
        /// </summary>
        /// <param name="ob">输入要删除的输入</param>
        /// <returns>true删除成功，反之失败</returns>
        public bool delNode(object ob)//查找删除
        {
            Node x = head;
            Node t;
            bool b = false;
            for (int i = 0; i < index; i++)
            {

                t = x.next;
                if (t.item == ob)
                {
                    x.next = x.next.next;
                    index = index - 1;
                    b = true;
                }
                x = x.next;
            }
            return b;

        }
        #endregion

        #region 上下移动
        /**/
        /// <summary>
        /// 上移动
        /// </summary>
        /// <param name="l">指定要移动的位置</param>
        public void Upnode(int l)
        {
            if ((l > 1) && (l <= index))
            {
                object o = this.showNode(l - 1);
                this.delNode(l - 1);
                this.insertNode(o, l - 1);
            }
        }
        /**/
        /// <summary>
        /// 下移动
        /// </summary>
        /// <param name="l">指定要移动的位置</param>
        public void Downnode(int l)
        {
            if ((l > 0) && (l < index))
            {
                object o = this.showNode(l);
                this.delNode(l);
                this.insertNode(o, l);
            }
        }
        #endregion

        #region 排序 和反链
        /**/
        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="b">true（正）从小到大，false（反）</param>
        public void compositorNode(bool b)//排序true（正）从小到大，false（反）
        {
            if (b == true)
            {
                for (int i = 1; i < index; i++)
                    for (int j = 1; j < index - i + 1; j++)
                        if (this.CharNode(j) > this.CharNode(j + 1))
                            this.Downnode(j);
            }
            else
            {
                for (int i = 1; i < index; i++)
                    for (int j = 1; j < index - i + 1; j++)
                        if (this.CharNode(j) < this.CharNode(j + 1))
                            this.Downnode(j);
            }
        }
        private char CharNode(int l)
        {
            string s = this.showNode(l).ToString();
            char[] c = s.ToCharArray();
            return c[0];
        }
        /**/
        /// <summary>
        /// 反链
        /// </summary>
        public void rollbackNode()//反链(其实是反链head后的)
        {
            Node t, y, r = null;
            y = head.next;
            while (y != null)
            {
                t = y.next; y.next = r; r = y; y = t;
            }
            head.next = r;//把head链上最后一个
        }
        #endregion

        #region 显示节点和接点数
        /**/
        /// <summary>
        /// 返回节点数方法
        /// </summary>
        /// <returns>节点数</returns>
        public int showNodenum()
        {
            return index;
        }
        /**/
        /// <summary>
        /// 显示指定数据
        /// </summary>
        /// <param name="l">指定位置</param>
        /// <returns>返回数据</returns>
        public object showNode(int l)//显示指定l从1开始
        {
            if ((l <= index) && (l > 0))
            {
                Node x = head;
                for (int i = 0; i < l; i++)
                {
                    x = x.next;
                }
                return x.item;
            }
            else
            {
                return head.item;
            }
        }
        ///**//// <summary>
        /// 显示所有
        /// </summary>
        /// <returns>返回一个ObQueue对象</returns>
        //public ObQueue showAllNode()//显示所有
        //{
        //    ObQueue oq = new ObQueue();
        //    Node x = head;
        //    for (int i = 0; i < index; i++)
        //    {
        //        x = x.next;
        //        oq.qput(x.item);
        //    }
        //    return oq;
        //}

        public bool findNode(object ob, ref int ind)
        {
            Node x = head;
            Node t;
            bool b = false;
            for (int i = 0; i < index; i++)
            {

                t = x.next;
                if (t.item.Equals(ob) )
                {
                    ind = i + 1;
                    //x.next = x.next.next;
                    //index = index - 1;
                    b = true;
                    break;
                }
                else if (t.item.ToString() == ob.ToString())
                {
                    ind = i + 1;
                    //x.next = x.next.next;
                    //index = index - 1;
                    b = true;
                    break;
                }
                x = x.next;
            }
            return b;
        }

        public Node GetNode(int no)
        {
            Node x = head;
            Node t;
            for (int i = 0; i < index; i++)
            {
                t = x.next;
                if (i == no - 1)
                {
                    return t;
                }
                x = x.next;
            }

            return null;
        }

        #endregion
    }
}
