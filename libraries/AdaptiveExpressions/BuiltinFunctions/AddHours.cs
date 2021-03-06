﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Add a number of hours to a timestamp.
    /// </summary>
    public class AddHours : TimeTransformEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddHours"/> class.
        /// </summary>
        public AddHours()
                : base(ExpressionType.AddHours, Function)
        {
        }

        private static DateTime Function(DateTime time, int interval)
        {
            return time.AddHours(interval);
        }
    }
}
