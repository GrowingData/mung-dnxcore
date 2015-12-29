using System;
using Microsoft.AspNet.Mvc.Razor.Precompilation;
using Microsoft.Extensions.PlatformAbstractions;

namespace GrowingData.Mung.Web {
    public class RazorPreCompilation : RazorPreCompileModule
    {
        public RazorPreCompilation(IApplicationEnvironment applicationEnvironment)
        {
            GenerateSymbols = string.Equals(applicationEnvironment.Configuration,
                                            "debug",
                                            StringComparison.OrdinalIgnoreCase);
        }
    }
}