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
    public interface IMataPelajaranRepository
    {
        MataPelajaran GetByKode(string kode);
        IList<MataPelajaran> GetAll();
    }

    public class MataPelajaranRepository : IMataPelajaranRepository
    {
        private IDapperContext _context;
        private ILog _log;

        public MataPelajaranRepository(IDapperContext context, ILog log)
        {
            this._context = context;
            this._log = log;
        }

        public MataPelajaran GetByKode(string kode)
        {
            MataPelajaran mataPelajaran = null;

            try
            {
                var sql = @"select kode, deskripsi 
                            from matapelajaran 
                            where kode = @kode";

                mataPelajaran = _context.db.QuerySingleOrDefault<MataPelajaran>(sql, new { kode });
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
            }

            return mataPelajaran; 
        }

        public IList<MataPelajaran> GetAll()
        {
            IList<MataPelajaran> listOfMataPelajaran = new List<MataPelajaran>();

            try
            {
                var sql = @"select kode, deskripsi 
                            from matapelajaran
                            order by deskripsi";

                listOfMataPelajaran = _context.db.Query<MataPelajaran>(sql)
                                              .ToList();
                                              
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
            }

            return listOfMataPelajaran;
        }
    }
}
