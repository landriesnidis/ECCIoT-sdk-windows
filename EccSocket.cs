﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ECC_sdk_windows
{
    /// <summary>
    /// 异步通信的Socket客户端工具类
    /// 参考资料：https://blog.csdn.net/mss359681091/article/details/51790931
    /// </summary>
    public class EccSocket : IDisposable
    {
        //Socket
        public Socket Socket { get;private set; }   
        private IPEndPoint ipep;
        private int maxCacheSize = 2048;
        //回调接口
        public IEccReceiptListener EccReceiptListener { private get; set; }
        public IEccDataReceiveListener EccDataReceiveListener { private get; set; }
        public IEccExceptionListener EccExceptionListener { private get; set; }
        //字符编码
        private Encoding encoding = Encoding.UTF8;
        public Encoding Encoding { set { encoding = value; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipep">网络终结点</param>
        public EccSocket(IPEndPoint ipep)
        {
            this.ipep = ipep;
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        public void Connect(IEccReceiptListener listener)
        {
            //端口及IP  
            //IPEndPoint ipe = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6065);
            //创建套接字  
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                //开始对一个远程主机建立异步的连接请求
                client.BeginConnect(ipep, asyncResult =>
                {
                    //结束挂起的异步连接请求
                    client.EndConnect(asyncResult);
                    //连接完成回调
                    EccReceiptListener.Ecc_Connection(listener,true);
                    //接受消息  
                    Recive();
                }, null);
            }
            catch (SocketException ex) {
                //与服务器连接失败
                EccReceiptListener.Ecc_Connection(listener,false);
            }
            
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="message">消息字符串</param>
        public void Send(IEccReceiptListener listener, string message)
        {
            if (Socket == null || message == string.Empty) return;
            //编码  
            byte[] data = encoding.GetBytes(message);
            try
            {
                //异步发送数据
                Socket.BeginSend(data, 0, data.Length, SocketFlags.None, asyncResult =>
                {
                    //完成发送消息  
                    int length = Socket.EndSend(asyncResult);
                    //消息发送成功
                    EccReceiptListener.Ecc_Sent(listener,message, true);
                }, null);
            }
            catch (SocketException ex)
            {
                //消息发送失败
                EccReceiptListener.Ecc_Sent(listener,message, false);
                //异常回调
                EccExceptionListener.Ecc_BreakOff(ex);
            }
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        private void Recive()
        {
            //缓存区
            byte[] data = new byte[maxCacheSize];
            try
            {
                //开始接收数据  
                Socket.BeginReceive(data, 0, data.Length, SocketFlags.None,
                asyncResult =>
                {
                    int length = Socket.EndReceive(asyncResult);
                    //消息接收回调
                    EccDataReceiveListener.Ecc_Received(encoding.GetString(data), length);
                    //重启异步接收数据
                    Recive();
                }, null);
            }
            catch (SocketException ex)
            {
                EccExceptionListener.Ecc_BreakOff(ex);
            }
        }

        public void Dispose()
        {
            Socket.Close();
            Socket.Dispose();
        }
    }
}