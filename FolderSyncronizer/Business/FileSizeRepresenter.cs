using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FolderSyncronizer.Business
{
    public class FileSizeRepresenter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.GetType() == typeof(long))
            {
                long sizeInBytes = (long)value;
                if (sizeInBytes < 1024)
                    return sizeInBytes + " Bytes";
                else if (sizeInBytes < 1048576)
                    return sizeInBytes / 1024 + " KB";
                else
                    return sizeInBytes / (1024 * 1024) + " MB"; 
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
