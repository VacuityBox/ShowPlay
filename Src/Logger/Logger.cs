// ShowPlay - Show what song is playing
//
// Copyright (C) 2020-2021 VacuityBox
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
// SPDX-License-Identifier: GPL-3.0-only 

using System;
using System.Collections.Concurrent;

namespace ShowPlay
{
    using BackendDictionary = ConcurrentDictionary<string, ILoggerBackend>;

    public class Logger
    {
        #region Private Proporties

        private object mLogLock { get; set; } = new object();
        private BackendDictionary mBackends { get; set; } = new BackendDictionary();

        #endregion

        #region Public Methods

        public bool AddBackend(string name, ILoggerBackend backend)
        {
            return mBackends.TryAdd(name, backend);
        }

        public void Log(LogType logType, string str, params object[] args)
        {
            var formatedString = string.Format(str, args);
            var time = DateTime.Now;
            var info = new LogInfo(logType, time, formatedString);

            lock (mLogLock)
            {
                foreach (var backend in mBackends.Values)
                {
                    if (!backend.IsInitialized())
                        continue;

                    if (backend.LogLevel > (int)logType)
                        continue;

                    backend.LogMessage(info);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Global Logger
    /// </summary>
    public static class Log
    {
        #region Public Proporties

        public static Logger LoggerInstance { get; private set; } = new Logger();

        #endregion

        #region Log Methods

        public static void Debug(string str, params object[] args)
        {
            LoggerInstance.Log(LogType.Debug, str, args);
        }

        public static void Info(string str, params object[] args)
        {
            LoggerInstance.Log(LogType.Info, str, args);
        }

        public static void Success(string str, params object[] args)
        {
            LoggerInstance.Log(LogType.Success, str, args);
        }

        public static void Warning(string str, params object[] args)
        {
            LoggerInstance.Log(LogType.Warning, str, args);
        }

        public static void Error(string str, params object[] args)
        {
            LoggerInstance.Log(LogType.Error, str, args);
        }

        public static void Fatal(string str, params object[] args)
        {
            LoggerInstance.Log(LogType.Fatal, str, args);
        }

        #endregion
    }
}
