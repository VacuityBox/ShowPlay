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
using System.Text.Json;

namespace ShowPlay
{
    public static class SettingsReader
    {
        public static Settings Load(string fileName)
        {
            Log.Info("Loading settings file '{0}'", fileName);
            var settings = new Settings();

            if (!File.Exists(fileName))
            {
                Log.Warning("Settings file '{0}' don't exists, using default values", fileName);
                throw new Exception();
            }
            else
            {
                try
                {                    
                    var data = File.ReadAllBytes(fileName);
                    var reader = new Utf8JsonReader(data);
                    var options = new JsonSerializerOptions()
                    {
                        Converters =
                        {
                            new FontFamilyJsonConverter(),
                            new FontStretchJsonConverter(),
                            new FontStyleJsonConverter(),
                            new FontWeightJsonConvereter(),
                            new SolidColorBrushJsonConverter()
                        }

                    };
                    settings = JsonSerializer.Deserialize<Settings>(ref reader, options);

                    Log.Success("Successfully loaded settings");
                }
                catch (Exception e)
                {
                    Log.Error("Failed to load settings {0}", e.ToString());
                    throw;
                }
            }

            return settings;
        }
    }
}
