﻿using System.Diagnostics;
using System.IO;
using Microsoft.Test.ApplicationControl;

namespace DistrEx.Coordinator.Test.Util
{
    internal static class ProcessHelper
    {
        /// <summary>
        /// blocks until started
        /// </summary>
        /// <param name="exeFile">path relative to App base path</param>
        /// <returns></returns>
        internal static AutomatedApplication Start(string exeFile)
        {
            var psi = new ProcessStartInfo(exeFile);
            psi.WorkingDirectory = Path.GetFullPath(Path.GetDirectoryName(exeFile));
            var result = new OutOfProcessApplication(new OutOfProcessApplicationSettings
            {
                ProcessStartInfo = psi,
                ApplicationImplementationFactory = new UIAutomationOutOfProcessApplicationFactory()
            });
            result.Start();

            return result;
        }

        /// <summary>
        ///     blocks until closed
        /// </summary>
        /// <param name="process"></param>
        internal static void Stop(AutomatedApplication process)
        {
            process.Close();
        }
    }
}
