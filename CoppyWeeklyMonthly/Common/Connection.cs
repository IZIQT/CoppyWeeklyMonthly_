using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoppyWeeklyMonthly.Common
{
    class Connection
    {
        private static readonly OracleConnectionStringBuilder oraString = new OracleConnectionStringBuilder();
        public static string User { get; } = "51";
        public static string Pass { get; } = "15";
        public static string DB { get; } = "alpha";

        public static string GetConnectionString()
        {
            oraString.UserID = User;
            oraString.Password = Pass;
            oraString.DataSource = DB;
            oraString.IntegratedSecurity = false;

            return oraString.ConnectionString;
        }

        public static string ConectionString(string user, string pass, string db)
        {
            var oraString = new OracleConnectionStringBuilder
            {
                UserID = user,
                Password = pass,
                DataSource = db,
                IntegratedSecurity = true
            };


            return oraString.ConnectionString;
        }
    }
}
