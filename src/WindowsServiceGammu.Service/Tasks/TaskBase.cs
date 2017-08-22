
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
using System.Timers;

namespace WindowsServiceGammu.Service
{
    /// <summary>
    /// Referensi: https://stackoverflow.com/questions/31988775/windows-service-can-it-run-two-separate-tasks-scheduled-at-different-intervals
    /// </summary>
    public abstract class TaskBase
    {
        /// <summary>
        /// Task timer
        /// </summary>
        private Timer _timer;

        public TaskBase(double refreshInterval)
        {
            _timer = new Timer(refreshInterval)
            {
                AutoReset = true
            };
        }

        /// <summary>
        /// This method is executed each time the task's timer has reached the interval specified in the constructor.
        /// Time counters are automatically updated.
        /// </summary>
        protected abstract void ExecTask();

        public void StartService()
        {
            _timer.Elapsed += _timer_Elapsed;

            ResetTimer();

            // Run the task once when starting instead of waiting for a full interval.
            ExecTask();
        }

        public void StopService()
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
                _timer.Elapsed -= _timer_Elapsed;
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ExecTask();
        }

        private void ResetTimer()
        {
            if (_timer.Enabled) _timer.Stop();
            _timer.Start();
        }        
    }
}
