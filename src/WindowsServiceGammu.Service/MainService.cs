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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using log4net;

namespace WindowsServiceGammu.Service
{
    public partial class MainService : ServiceBase
    {
        private readonly List<TaskBase> _listOfTask;
        private readonly ILog _log;
        private const int RefreshInterval = 1000; // In milliseconds

        public MainService()
        {
            InitializeComponent();
            _log = Program.log;

            // Add in this list the tasks to run periodically.
            // Tasks frequencies are set in the corresponding classes.
            _listOfTask = new List<TaskBase>
            {
                new SMSGatewayTask(RefreshInterval, _log)
            };
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _log.Info("Services started ...");
                _listOfTask.ForEach(t => t.StartService());
            }
            catch (Exception ex)
            {
                _log.Error("Error:", ex);
                Stop();
            }
        }

        protected override void OnStop()
        {
            _log.Info("Services stoped ...");
            _listOfTask.ForEach(t => t.StopService());
        }
    }
}
