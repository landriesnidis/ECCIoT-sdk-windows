﻿using System;

namespace ECC_sdk_windows.Listener
{
    /// <summary>
    /// 异常错误回调接口
    /// </summary>
    public interface IEccExceptionListener
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        void Ecc_BreakOff(Exception e);
    }
}
