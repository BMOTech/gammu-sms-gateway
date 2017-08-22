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
    public interface INilaiRepository
    {
        IList<Nilai> GetByNIS(string nis);
        Nilai GetByNIS(string nis, string kode);
    }

    public class NilaiRepository : INilaiRepository
    {
        private IDapperContext _context;
        private ILog _log;

        public NilaiRepository(IDapperContext context, ILog log)
        {
            this._context = context;
            this._log = log;
        }

        public IList<Nilai> GetByNIS(string nis)
        {
            IList<Nilai> listOfNilai = new List<Nilai>();

            try
            {
                var sql = @"select * 
                            from nilai
                            where nis = @nis
                            order by kode";

                listOfNilai = _context.db.Query<Nilai>(sql, new { nis })
                                      .ToList();

            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
            }

            return listOfNilai;
        }

        public Nilai GetByNIS(string nis, string kode)
        {
            Nilai nilai = null;

            try
            {
                var sql = @"select * 
                            from nilai
                            where nis = @nis and kode = @kode";

                nilai = _context.db.QuerySingleOrDefault<Nilai>(sql, new { nis, kode });
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
            }

            return nilai;
        }
    }
}
