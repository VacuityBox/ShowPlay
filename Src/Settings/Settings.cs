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

namespace ShowPlay
{
    public class Settings
    {
        public MarqueeTextViewModel Row1 { get; set; }
        public MarqueeTextViewModel Row2 { get; set; }

        public double WindowPosX   { get; set; }
        public double WindowPosY   { get; set; }
        public double WindowWidth  { get; set; }
        public double WindowHeight { get; set; }
        public string WindowTitle  { get; set; }

        public int LoggerPosX   { get; set; }
        public int LoggerPosY   { get; set; }
        public int LoggerWidth  { get; set; }
        public int LoggerHeight { get; set; }
        public int LogLevel     { get; set; }        

        public string ServerIp   { get; set; }
        public int    ServerPort { get; set; }
    }
}
