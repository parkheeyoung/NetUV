﻿// Copyright (c) Johnny Z. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NetUV.Core.Common
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Configuration;

    static class Configuration
    {
        const string ConfigurationFileName = "appsettings.json";

        static Configuration()
        {
            IConfigurationBuilder builder = 
                new ConfigurationBuilder()
                .SetBasePath(ProcessDirectory);

            if (File.Exists(Path.Combine(ProcessDirectory, ConfigurationFileName)))
            {
                builder.AddJsonFile(ConfigurationFileName);
            }

            Current = builder.Build();
        }

        public static string ProcessDirectory => AppContext.BaseDirectory;

        internal static IConfigurationRoot Current { get; }

        public static bool TryGetValue(string name, out int value)
        {
            value = 0;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            string currentValue = Current[name];
            return !string.IsNullOrEmpty(currentValue) 
                && int.TryParse(currentValue, out value);
        }

        public static bool TryGetValue(string name, out bool value)
        {
            value = false;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            string currentValue = Current[name];
            return !string.IsNullOrEmpty(currentValue)
                && bool.TryParse(currentValue, out value);
        }
    }
}
