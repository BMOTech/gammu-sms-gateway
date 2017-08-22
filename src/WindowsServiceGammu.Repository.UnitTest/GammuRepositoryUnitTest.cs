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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using WindowsServiceGammu.Model.Gammu;
using WindowsServiceGammu.Repository;

namespace WindowsServiceGammu.Repository.UnitTest
{
    [TestClass]
    public class GammuRepositoryUnitTest
    {
        private ILog _log;
        private IDapperContext _context;
        private IGammuRepository _repo;

        [TestInitialize]
        public void Init()
        {
            _log = LogManager.GetLogger(typeof(GammuRepositoryUnitTest));
            _context = new MySqlContext();
            _repo = new GammuRepository(_context, _log);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _repo = null;
            _context.Dispose();
        }

        [TestMethod]
        public void ReadInboxTest()
        {
            var oList = _repo.ReadInbox();

            var index = 1;
            var obj = oList[index];

            Assert.IsNotNull(obj);
            Assert.AreEqual(9, obj.Id);
            Assert.AreEqual("", obj.UDH);
            Assert.AreEqual("TELKOMSEL", obj.SenderNumber);
            Assert.AreEqual("Terimakasih telah melakukan pengisian ulang dgn SN 41001621521331 senilai 10000.", obj.TextDecoded);
            Assert.AreEqual(new DateTime(2017, 8, 3, 8, 59, 50), obj.ReceivingDateTime);
        }

        [TestMethod]
        public void UpdateInboxTest()
        {
            var inboxId = 9;

            var result = _repo.UpdateInbox(inboxId);
            Assert.IsTrue(result != 0);
        }

        [TestMethod]
        public void SaveOutboxTest()
        {
            var outbox = new Outbox
            {
                DestinationNumber = "+6281381769915",
                UDH = "",
                TextDecoded = "tesss",
                MultiPart = "true"
            };

            var result = _repo.SaveOutbox(outbox);
            Assert.IsTrue(result != 0);

            if (outbox.MultiPart == "true")
            {
                var outboxMultipart = new OutboxMultipart
                {
                    Id = outbox.Id,
                    UDH = "",
                    TextDecoded = "tess #2",
                    SequencePosition = 2
                };

                result = _repo.SaveOutboxMultipart(outboxMultipart);
                Assert.IsTrue(result != 0);
            }
        }
    }
}
