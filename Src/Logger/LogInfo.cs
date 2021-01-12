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

namespace ShowPlay
{
    public class LogInfo
    {
        public LogType  Type    { get; set; }
        public DateTime Time    { get; set; }
        public string   Message { get; set; }

        public LogInfo(LogType type, DateTime time, string message)
        {
            Type = type;
            Time = time;
            Message = message;
        }

        public override string ToString()
        {
            var timeStr = Time.ToString("yyyy-MM-dd HH:mm:ss");
            return string.Format("{0} [{1,8}] {2}", timeStr, Type.ToString(), Message);
        }
    }
}
