// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Hosting;

namespace Interop.Microsoft.Owin.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || args[0] != "--server.urls")
            {
                throw new ArgumentException("Expected --server.urls parameter");
            }

            using (WebApp.Start<Startup>(args[1]))
            {
                Console.WriteLine("Now listening on: " + args[1]);
                Console.WriteLine("Application started. Press Ctrl+C to shut down.");
                Console.ReadKey();
            }
        }
    }
}
