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

using WindowsServiceGammu.Model;
using WindowsServiceGammu.Repository;

namespace WindowsServiceGammu.Repository.UnitTest
{
    [TestClass]
    public class NilaiRepositoryUnitTest
    {
        private ILog _log;
        private IDapperContext _context;
        private INilaiRepository _repo;

        [TestInitialize]
        public void Init()
        {
            _log = LogManager.GetLogger(typeof(MataPelajaranRepositoryUnitTest));
            _context = new SQLiteContext();
            _repo = new NilaiRepository(_context, _log);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _repo = null;
            _context.Dispose();
        }

        [TestMethod]
        public void GetByNISAndKodeTest()
        {
            var nis = "104012";
            var kode = "BING";
            var obj = _repo.GetByNIS(nis, kode);

            Assert.IsNotNull(obj);
            Assert.AreEqual("104012", obj.nis);
            Assert.AreEqual("BING", obj.kode);
            Assert.AreEqual(90, obj.nilai);
        }

        [TestMethod]
        public void GetByNISTest()
        {
            var nis = "104012";
            var oList = _repo.GetByNIS(nis);

            var index = 2;
            var obj = oList[index];

            Assert.IsNotNull(obj);
            Assert.AreEqual("104012", obj.nis);
            Assert.AreEqual("BING", obj.kode);
            Assert.AreEqual(90, obj.nilai);
        }
    }
}
