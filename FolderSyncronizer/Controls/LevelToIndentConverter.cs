﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace FolderSyncronizer.Controls
{

    public class LevelToIndentConverter : IValueConverter
    {
        private const double c_IndentSize = 19.0;
        public object Convert(object o, Type type, object parameter, CultureInfo culture)

        {
            return new Thickness((int)o * c_IndentSize, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
