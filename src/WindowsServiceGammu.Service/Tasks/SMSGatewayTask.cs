
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

using log4net;
using WindowsServiceGammu.Model;
using WindowsServiceGammu.Model.Gammu;
using WindowsServiceGammu.Repository;

namespace WindowsServiceGammu.Service
{
    public class SMSGatewayTask : TaskBase
    {
        private ILog _log;

        public SMSGatewayTask(int refreshInterval, ILog log)
            : base(refreshInterval) // In milliseconds
        {
            _log = log;
        }        

        protected override void ExecTask()
        {
            using (IDapperContext mysqlContext = new MySqlContext())
            {
                IGammuRepository gammuRepo = new GammuRepository(mysqlContext, _log);
                var listOfInbox = gammuRepo.ReadInbox();

                foreach (var inbox in listOfInbox)
                {                    
                    var phoneNumber = inbox.SenderNumber;

                    if (phoneNumber.Substring(0, 3) == "+62")
                    {
                        var keyword = inbox.TextDecoded;
                        var prefix = keyword;
                        var msg = string.Empty;

                        if (keyword.IndexOf("#") >= 0) // karakter # -> separator keyword
                        {
                            var nis = string.Empty;
                            var kodeMP = string.Empty;

                            var arrKeyword = keyword.Split('#');
                            prefix = arrKeyword[0];

                            switch (prefix.ToUpper())
                            {
                                case "CEKSISWA": // FORMAT PERINTAH: CEKSISWA#NIS
                                    nis = arrKeyword[1]; // nis di ambil dari parameter pertama
                                    msg = GetBalasanCekSiswa(nis);

                                    break;

                                case "CEKNILAI": // FORMAT PERINTAH: CEKNILAI#NIS#<OPTIONAL KODE MP>
                                    nis = arrKeyword[1]; // nis di ambil dari parameter pertama
                                    kodeMP = arrKeyword.Count() > 2 ? arrKeyword[2] : string.Empty;

                                    msg = GetBalasanCekNilai(nis, kodeMP);
                                    break;

                                default:
                                    break;
                            }
                        }
                        else
                        {
                            // FORMAT PERINTAH: CEKMP
                            if (keyword.ToUpper() == "CEKMP")
                            {
                                msg = GetBalasanCekMP();
                            }
                            else // keyword tidak valid
                            {
                                msg = string.Format("Keyword {0} tidak terdaftar", keyword.ToUpper());
                            }
                        }

                        SaveOutbox(msg, inbox, gammuRepo);
                    }                    
                }
            }
        }

        /// <summary>
        /// Method untuk menyimpan pesan yang akan dikirim ke tabel outbox
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="inbox"></param>
        /// <param name="gammuRepo"></param>
        private void SaveOutbox(string msg, Inbox inbox, IGammuRepository gammuRepo)
        {
            var result = 0;

            // insert ke tabel outbox
            var jumlahSMS = (int)Math.Ceiling((double)msg.Length / 160);

            if (jumlahSMS > 1) // balasan sms > 160 karakter, sms dipecah sebelum dikirim
            {
                var listSms = msg.SplitByLength(153)
                                 .ToList();

                var smsKe = 1;
                var outboxID = 0;
                foreach (var sms in listSms)
                {
                    var udh = inbox.UDH;

                    if (udh.Length == 0)
                    {
                        udh = string.Format("050003A7{0:00}{1:00}", listSms.Count, smsKe);
                    }
                    else
                    {
                        udh = inbox.UDH.Substring(0, inbox.UDH.Length - 4);
                        udh = string.Format("{0}{1:00}{2:00}", udh, listSms.Count, smsKe);
                    }

                    if (smsKe == 1)
                    {
                        var outbox = new Outbox
                        {
                            DestinationNumber = inbox.SenderNumber,
                            UDH = udh,
                            TextDecoded = sms,
                            MultiPart = "true"
                        };

                        result = gammuRepo.SaveOutbox(outbox);
                        if (result > 0)
                        {
                            outboxID = outbox.Id;
                        }
                    }
                    else // sms ke 2, 3, dst, simpan ke tabel outbox_multipart
                    {
                        var outboxMultipart = new OutboxMultipart
                        {
                            Id = outboxID,
                            UDH = udh,
                            TextDecoded = sms,
                            SequencePosition = smsKe
                        };

                        result = gammuRepo.SaveOutboxMultipart(outboxMultipart);
                    }

                    smsKe++;
                }
            }
            else // balasan sms <= 160 karakter
            {
                var outbox = new Outbox
                {
                    DestinationNumber = inbox.SenderNumber,
                    UDH = string.Empty,
                    TextDecoded = msg,
                    MultiPart = "false"
                };

                result = gammuRepo.SaveOutbox(outbox);
            }

            if (result > 0)
            {
                // update status pesan di inbox menjadi sudah diproses
                result = gammuRepo.UpdateInbox(inbox.Id);
            }
        }

        /// <summary>
        /// Method untuk mengenerate pesan balasan untuk keyword: CEKSISWA#NIS
        /// </summary>
        /// <param name="nis"></param>
        /// <returns></returns>
        private string GetBalasanCekSiswa(string nis)
        {
            var msg = string.Empty;

            using (IDapperContext sqliteContext = new SQLiteContext())
            {
                ISiswaRepository siswaRepo = new SiswaRepository(sqliteContext, _log);
                var siswa = siswaRepo.GetByNIS(nis);

                if (siswa == null)
                {
                    msg = string.Format("NIS: {0} tidak ditemukan", nis);
                }
                else
                {
                    msg = string.Format("NIS: {0}\nNAMA: {1}", siswa.nis, siswa.nama);
                }
            }

            return msg;
        }

        /// <summary>
        /// Method untuk mengenerate pesan balasan untuk keyword: CEKNILAI#NIS#<OPTIONAL KODE MP>
        /// </summary>
        /// <param name="nis"></param>
        /// <param name="kodeMP"></param>
        /// <returns></returns>
        private string GetBalasanCekNilai(string nis, string kodeMP)
        {
            var msg = string.Empty;

            IList<Nilai> listOfNilai = new List<Nilai>();

            using (IDapperContext sqliteContext = new SQLiteContext())
            {
                ISiswaRepository siswaRepo = new SiswaRepository(sqliteContext, _log);
                var siswa = siswaRepo.GetByNIS(nis);

                if (siswa == null)
                {
                    msg = string.Format("NIS: {0} tidak ditemukan", nis);
                }
                else
                {
                    INilaiRepository nilaiRepo = new NilaiRepository(sqliteContext, _log);

                    if (nis.Length > 0 && kodeMP.Length > 0)
                    {
                        var nilai = nilaiRepo.GetByNIS(nis, kodeMP);
                        listOfNilai.Add(nilai);
                    }
                    else
                    {
                        listOfNilai = nilaiRepo.GetByNIS(nis);
                    }

                    msg = string.Format("NIS: {0}\nNAMA: {1}\n", siswa.nis, siswa.nama);
                    msg += "Nilai:\n";

                    foreach (var nilai in listOfNilai)
                    {
                        msg += string.Format("{0}: {1}\n", nilai.kode, nilai.nilai);
                    }
                }
            }

            return msg;
        }

        /// <summary>
        /// Method untuk mengenerate pesan balasan untuk keyword: CEKMP
        /// </summary>
        /// <returns></returns>
        private string GetBalasanCekMP()
        {
            var msg = string.Empty;

            using (IDapperContext sqliteContext = new SQLiteContext())
            {
                IMataPelajaranRepository mataPelajaranRepo = new MataPelajaranRepository(sqliteContext, _log);
                var listOfMataPelajaran = mataPelajaranRepo.GetAll();

                msg = string.Empty;
                msg = "kode mata pelajaran:\n";

                foreach (var mataPelajaran in listOfMataPelajaran)
                {
                    msg += string.Format("{0}: {1}\n", mataPelajaran.kode, mataPelajaran.deskripsi);
                }
            }

            return msg;
        }
    }
}
