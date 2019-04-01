// Copyright (c) Brock Allen, Dominick Baier, Michele Leroux Bustamante. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace DataService.Models
{
    /// <summary>
    /// Models an application role
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the subjects.
        /// </summary>
        /// <value>
        /// The subjects.
        /// </value>
        public List<string> Permissions { get; set; }
        
    }
}