Windows Data and Analytics Shared Code - JSON Processing
========================================================
Are you doing simple read and write tasks with JSON strings?  
Do you want to partially process your JSON payload?  
Are you running it in a tight loop?  
Are you wondering why there's so much pressure on the garbage collector?  
Do you want something even faster?  

If so, then this package might be right for you.  

The Windows team is building it's own high-performance, low-allocation JSON API for processing data in various "big data" systems. We've managed to squeeze some pretty astonishing performance out of our implementation. Since it wasn't tightly coupled to anything either, we thought we'd share the goodness with everyone.

We're sharing our code as NuGet "recipe" packages. That means that instead of adding a DLL reference to your code, we're adding the source files themselves. Everything we add is internal and therefore scoped only to that assembly. So, you can freely add this to as many projects as you like without fear of conflicts. We do this for a few reasons:

1. In this day and age, the JIT compiler is usually smart enough to optimize away annything you don't use and codegen overhead isn't typically concerning on beefy server machines.
2. It helps avoid assembly versioning issues when several projects have a dependency on this and each other.
3. It also -- to a lesser degree -- insulates us from framework versioning issues. We try to keep the syntax simple and therefore, in many cases, we're natually compatible all the way back to .NET 3.5.
4. It gives the compiler a chance to inline our code in your project, thus giving us a boost in performance in some cases.
5. Most importantly, it gives you the freedom to tweak and tinker without even necessarily having to wait for a pull request to be approved. Although, you can still do that (and we encourage it). You just don't have to necessarily wait for all the paperwork to go through if you're in a hurry.

License
-------
This source code and artifacts are released under the terms of the [MIT License](LICENSE.txt). 

How do I install it?
--------------------
The package is available on [nuget.org](http://www.nuget.org/packages/Microsoft.Shared.Dna.Json).

How do I build it?
------------------
If you have Visual Studio 2015 installed, open a "Developer Command Prompt for VS2015" (not an MSBuild command prompt) and run:

    build.cmd

This will download the dependencies, compile the code, run unit tests, and package everything. You should end up with a file named something like Microsoft.Shared.Dna.Json.{major}.{minor}.{patch}.nupkg under the Drop folder.

How can I contribute?
---------------------
Please refer to [CONTRIBUTING.md](CONTRIBUTING.md).

Reporting Security Vulnerabilities
----------------------------------
If you believe you have found a security vulnerability in this project, please follow [these steps](https://technet.microsoft.com/en-us/security/ff852094.aspx) to report it. For more information on how vulnerabilities are disclosed, see [Coordinated Vulnerability Disclosure](https://technet.microsoft.com/en-us/security/dn467923).

Code of Conduct
---------------
This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
