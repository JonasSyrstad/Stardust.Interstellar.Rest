using System;
using System.Collections.Generic;
using Stardust.Particles;

namespace Stardust.Continuum
{
    public static class DateTimeHelper
    {
        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime;
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }
        public static double GetSizeIn(long lengthInByte, out FileSizeTypes sizeQualifier)
        {
            try
            {
                List<FileSizeTypes> sizes = EnumHelper.EnumToList<FileSizeTypes>();
                double len = lengthInByte;
                var order = 0;
                while (len >= 1024 && ++order < 5)
                {
                    len = len / 1024;
                }

                sizeQualifier = (FileSizeTypes)order;
                return len;
            }
            catch (Exception ex)
            {
                ex.Log();
                sizeQualifier = FileSizeTypes.MB;
                double len = lengthInByte;
                return len / 1024 / 1024;
            }
        }
    }
}