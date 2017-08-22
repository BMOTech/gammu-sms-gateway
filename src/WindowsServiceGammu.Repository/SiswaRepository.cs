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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dapper;
using log4net;
using WindowsServiceGammu.Model;

namespace WindowsServiceGammu.Repository
{
    public interface ISiswaRepository
    {
        Siswa GetByNIS(string nis);
    }

    public class SiswaRepository : ISiswaRepository
    {
        private IDapperContext _context;
        private ILog _log;

        public SiswaRepository(IDapperContext context, ILog log)
        {
            this._context = context;
            this._log = log;
        }

        public Siswa GetByNIS(string nis)
        {
            Siswa siswa = null;

            try
            {
                var sql = @"select nis, nama 
                            from siswa 
                            where nis = @nis";

                siswa = _context.db.QuerySingleOrDefault<Siswa>(sql, new { nis });
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
            }

            return siswa;   
        }
    }
}
