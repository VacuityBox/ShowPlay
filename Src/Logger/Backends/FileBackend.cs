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
using System.IO;

namespace ShowPlay
{
    public class FileBackend : ILoggerBackend
    {
        #region Private Proporties

        private StreamWriter mFile { get; set; } = null;

        #endregion

        #region Public Proporties

        public int    LogLevel { get; set; }         = (int)LogType.Info;
        public string FileName { get; private set; } = "";

        #endregion

        #region Constructor

        public FileBackend(string fileName)
        {
            FileName = fileName;

            try
            {
                mFile = File.CreateText(FileName);
            }
            catch (Exception e)
            {
                Log.Error("Failed to create log file '{0}', {1}", FileName, e);
                mFile = null;
            }
            finally
            {
                mFile.AutoFlush = true;                
            }
        }

        #endregion

        #region ILoggerBackend Implementation

        public bool IsInitialized()
        {
            return mFile is not null;
        }

        public void LogMessage(LogInfo info)
        {
            if (mFile is null)
                return;

            try
            {
                mFile.WriteLine(info.ToString());
            }
            catch
            {
            }
        }

        #endregion
    }
}
