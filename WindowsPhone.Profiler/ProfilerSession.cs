using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsPhone.Tools;

namespace WindowsPhone.Profiler
{
    public class ProfilerSession
    {

        private Dictionary<string, EtwKernelFlag> _kernelFlags = new Dictionary<string, EtwKernelFlag>();
        public Dictionary<string, EtwKernelFlag> KernelFlags { get { return _kernelFlags; } }

        private Dictionary<string, EtwKernelFlag> _kernelStackFlags = new Dictionary<string, EtwKernelFlag>();
        public Dictionary<string, EtwKernelFlag> KernelStackFlags { get { return _kernelStackFlags; } }

        private Dictionary<string, EtwProviderType> _providers = new Dictionary<string, EtwProviderType>();
        public Dictionary<string, EtwProviderType> Providers { get { return _providers; } }

        public RemoteApplicationEx TargetApp { get; set; }

        public bool IsValid
        {
            get
            {
                if (TargetApp == null)
                {
                    return false;
                }

                bool foundEnabledProvider = false;

                // TODO: remove the code repetition below
                foreach (var v in KernelFlags.Values)
                {
                    if (v.IsEnabled)
                    {
                        foundEnabledProvider = true;
                        break;
                    }
                }

                if (!foundEnabledProvider)
                {
                    foreach (var v in KernelStackFlags.Values)
                    {
                        if (v.IsEnabled)
                        {
                            foundEnabledProvider = true;
                            break;
                        }
                    }
                }

                if (!foundEnabledProvider)
                {
                    foreach (var v in Providers.Values)
                    {
                        if (v.IsEnabled)
                        {
                            foundEnabledProvider = true;
                            break;
                        }
                    }
                }

                return foundEnabledProvider;
            }
        }

        public ProfilerSession()
        {
            CopyKernelFlags(ref _kernelFlags);
            CopyKernelFlags(ref _kernelStackFlags);
            CopyProviders();
        }

        private void CopyKernelFlags(ref Dictionary<string, EtwKernelFlag> target)
        {
            foreach (var flag in Profiler._kernelFlags)
            {
                target.Add(flag.Name, flag.Copy());
            }
        }

        private void CopyProviders()
        {
            foreach (var provider in Profiler._phoneProviders)
            {
                _providers.Add(provider.Name, provider.Copy());
            }
        }

        internal ProfilingTasksType GenerateTasks()
        {
            // get the kernel flags first
            EtwProviderType kernelProvider = GenerateKernelProvider();

            // get the regular providers
            List<EtwProviderType> etwProviders = GenerateEtwProviderList();

            // add the kernel provider to the list of providers we will be profiling
            etwProviders.Insert(0, kernelProvider);

            // now generate the correct XML object sequence tree
            ProfilingTasksType tasks = new ProfilingTasksType();
            tasks.Task = new TaskType[] { new TaskType() { Name = "WPProfiler" }};

            // EtwProvider is an array, go figure
            tasks.Task[0].EtwProvider = etwProviders.ToArray();

            return tasks;
        }

        private EtwProviderType GenerateKernelProvider()
        {
            EtwProviderType provider = new EtwProviderType()
            {
                Name = "KernelModeProvider",
                KernelMode = true,
                CLR = true,
                HexKeywords = GenerateKernelKeywords(_kernelFlags),
                KernelStackHexKeywords = GenerateKernelKeywords(_kernelStackFlags),
            };

            return provider;
        }

        private string GenerateKernelKeywords(Dictionary<string, EtwKernelFlag> flags)
        {
            int keyword = 0;

            foreach (var flag in flags.Values)
            {
                if (flag.IsEnabled)
                {
                    keyword |= flag.Keyword;
                }
            }

            return keyword.ToString();
        }

        private List<EtwProviderType> GenerateEtwProviderList()
        {
            var providers = new List<EtwProviderType>();

            foreach (var provider in _providers.Values)
            {
                if (provider.IsEnabled)
                {
                    // we don't add a copy since it is assumed that nothing will change
                    // while we're actually profiling
                    providers.Add(provider);
                }
            }

            return providers;
        }
    }
}
