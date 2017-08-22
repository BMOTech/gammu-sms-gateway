/**
 * Copyright (C) 2017 Kamarudin (http://coding4ever.net/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */

using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace WindowsServiceGammu.Repository
{
    public class MySqlContext : IDapperContext
    {
        private readonly string _providerName;
        private readonly string _connectionString;
        private IDbConnection _db;

        public MySqlContext()
        {
            var server = ConfigurationManager.AppSettings["server"];
            var dbName = ConfigurationManager.AppSettings["dbName"];
            var dbUser = ConfigurationManager.AppSettings["dbUser"];
            var dbUserPass = ConfigurationManager.AppSettings["dbUserPass"];

            _providerName = "MySql.Data.MySqlClient";
            _connectionString = string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3}", server, dbName, dbUser, dbUserPass);
        }

        public IDbConnection db
        {
            get { return _db ?? (_db = DbConnectionHelper.GetOpenConnection(_providerName, _connectionString)); }
        }

        public void Dispose()
        {
            if (_db != null)
            {
                try
                {
                    if (_db.State != ConnectionState.Closed)
                        _db.Close();
                }
                finally
                {
                    _db.Dispose();
                }
            }

            GC.SuppressFinalize(this);
        }
    }
}
