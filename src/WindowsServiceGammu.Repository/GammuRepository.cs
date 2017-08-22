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
using WindowsServiceGammu.Model.Gammu;

namespace WindowsServiceGammu.Repository
{
    public interface IGammuRepository
    {
        /// <summary>
        /// Method untuk membaca data sms di tabel inbox yang belum diproses
        /// </summary>
        /// <returns></returns>
        IList<Inbox> ReadInbox();

        /// <summary>
        /// Method untuk mengupdate status inbox menjadi sudah diproses
        /// </summary>
        /// <param name="inboxId"></param>
        /// <returns></returns>
        int UpdateInbox(int inboxId);

        /// <summary>
        /// Method untuk menyimpan data sms yang akan dikirim ke tabel outbox
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        int SaveOutbox(Outbox obj);

        /// <summary>
        /// Method untuk menyimpan data sms ke 2, 3, dst ke tabel outbox_multipart, jika data sms lebih dari 160 karakter
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        int SaveOutboxMultipart(OutboxMultipart obj);
    }

    public class GammuRepository : IGammuRepository
    {
        private IDapperContext _context;
        private ILog _log;

        public GammuRepository(IDapperContext context, ILog log)
        {
            this._context = context;
            this._log = log;
        }

        public IList<Inbox> ReadInbox()
        {
            IList<Inbox> listOfSMS = new List<Inbox>();

            try
            {
                var sql = @"SELECT `ID`, `UDH`, `SenderNumber`, `TextDecoded`, `ReceivingDateTime`
                            FROM inbox 
                            WHERE Processed = 'false'
                            ORDER BY ReceivingDateTime";

                listOfSMS = _context.db.Query<Inbox>(sql)
                                              .ToList();

            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
            }

            return listOfSMS;
        }

        public int UpdateInbox(int inboxId)
        {
            var result = 0;

            try
            {
                var sql = @"UPDATE inbox SET Processed = 'true' 
                            WHERE id = @inboxId";
                result = _context.db.Execute(sql, new { inboxId });
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
            }

            return result;
        }

        public int SaveOutbox(Outbox obj)
        {
            var result = 0;

            try
            {
                var sql = @"INSERT INTO outbox (DestinationNumber, UDH, TextDecoded, MultiPart, CreatorID)
                            VALUES (@DestinationNumber, @UDH, @TextDecoded, @MultiPart, 'Gammu')";
                result = _context.db.Execute(sql, obj);

                if (result > 0)
                {
                    sql = @"SELECT CONVERT(LAST_INSERT_ID(), SIGNED INTEGER) AS ID";
                    obj.Id = _context.db.QuerySingleOrDefault<int>(sql);
                }
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
            }

            return result;
        }

        public int SaveOutboxMultipart(OutboxMultipart obj)
        {
            var result = 0;

            try
            {
                var sql = @"INSERT INTO outbox_multipart(ID, UDH, TextDecoded, SequencePosition)
                            VALUES (@ID, @UDH, @TextDecoded, SequencePosition)";
                result = _context.db.Execute(sql, obj);
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
            }

            return result;
        }
    }
}
