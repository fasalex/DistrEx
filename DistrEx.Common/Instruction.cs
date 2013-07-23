﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DistrEx.Common
{
    public delegate TResult Instruction<in TArgument, out TResult>(CancellationToken cancellationToken, Action reportProgress, TArgument argument);
}