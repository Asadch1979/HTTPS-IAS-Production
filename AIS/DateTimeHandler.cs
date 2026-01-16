using System;
using System.Globalization;

namespace AIS
    {

    public class DateTimeHandler
        {

        public string DateTimeInDDMMYY(DateTime dt)
            {
            return dt.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
            }

        public string DateTimeInDDMMYYHHMMSS(DateTime dt)
            {
            return dt.ToString("dd'/'MM'/'yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
        }
    }
