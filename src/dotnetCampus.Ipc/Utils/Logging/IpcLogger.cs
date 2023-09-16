﻿using System;
using System.Diagnostics;

namespace dotnetCampus.Ipc.Utils.Logging
{
    /// <summary>
    /// 为 IPC 提供日志输出。
    /// </summary>
    public class IpcLogger : ILogger
    {
        /// <summary>
        /// 创建为 IPC 提供的日志
        /// </summary>
        /// <param name="name">当前的 Ipc 名。等同于管道名</param>
        public IpcLogger(string name)
        {
            Name = name;
        }

        private string Name { get; }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Log(logLevel, state, exception, formatter);
        }

        protected virtual void Log<TState>(LogLevel logLevel, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel >= LogLevel.Debug)
            {
                Debug.WriteLine(formatter(state, exception));
            }
        }

        /// <summary>
        /// 返回此日志的名字。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[{Name}]";
        }
    }
}
