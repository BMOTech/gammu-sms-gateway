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
using log4net.Appender;
using log4net.Core;
using WindowsServiceGammu.Model;

namespace WindowsServiceGammu.Repository
{
    public interface ILog4NetRepository
    {
        int Save(Log obj);
    }

    public class Log4NetRepository : ILog4NetRepository
    {
        public int Save(Log obj)
        {
            var result = 0;

            try
            {
                using (IDapperContext context = new SQLiteContext())
                {
                    var sql = @"INSERT INTO log (level, class_name, method_name, message, exception)
                                VALUES (@level, @class_name, @method_name, @message, @exception)";
                    result = context.db.Execute(sql, obj);
                }                
            }
            catch
            {
            }

            return result;            
        }
    }

    public class Log4NetAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            var log = new Log
            {
                level = loggingEvent.Level.ToString(),
                class_name = loggingEvent.LocationInformation.ClassName,
                method_name = loggingEvent.LocationInformation.MethodName,
                message = loggingEvent.RenderedMessage,
                exception = loggingEvent.GetExceptionString()
            };

            var logRepo = new Log4NetRepository();
            var result = logRepo.Save(log);
        }
    }
}
