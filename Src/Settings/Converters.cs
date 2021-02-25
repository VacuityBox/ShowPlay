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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace ShowPlay
{
    public class FontFamilyJsonConverter : JsonConverter<FontFamily>
    {
        public override FontFamily Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var fontFamilyName = reader.GetString() ?? "SegoeUI";
            return new FontFamily(fontFamilyName);
        }

        public override void Write(Utf8JsonWriter writer, FontFamily value, JsonSerializerOptions options)
        {
            var fontFamilyName = value.Source;
            writer.WriteStringValue(fontFamilyName);
        }
    }

    public class FontStretchJsonConverter : JsonConverter<FontStretch>
    {
        public override FontStretch Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var fontStretchString = reader.GetString() ?? "Normal";
            var converter = new System.Windows.FontStretchConverter();
            return (FontStretch)converter.ConvertFromString(fontStretchString);
        }

        public override void Write(Utf8JsonWriter writer, FontStretch value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public class FontStyleJsonConverter : JsonConverter<FontStyle>
    {
        public override FontStyle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var fontStyleString = reader.GetString() ?? "Normal";
            var converter = new System.Windows.FontStyleConverter();
            return (FontStyle)converter.ConvertFromString(fontStyleString);
        }

        public override void Write(Utf8JsonWriter writer, FontStyle value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public class FontWeightJsonConvereter : JsonConverter<FontWeight>
    {
        public override FontWeight Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var fontWeightString = reader.GetString() ?? "Normal";
            var converter = new System.Windows.FontWeightConverter();
            return (FontWeight)converter.ConvertFromString(fontWeightString);
        }

        public override void Write(Utf8JsonWriter writer, FontWeight value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public class SolidColorBrushJsonConverter : JsonConverter<SolidColorBrush>
    {
        public override SolidColorBrush Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var colorString = reader.GetString();
            var color = (Color)ColorConverter.ConvertFromString(colorString);
            return new SolidColorBrush(color);
        }

        public override void Write(Utf8JsonWriter writer, SolidColorBrush value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
