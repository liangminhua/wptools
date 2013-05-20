using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsPhone.Profiler
{
    public partial class Profiler
    {
        internal static List<EtwKernelFlag> _kernelFlags = new List<EtwKernelFlag>()
        {
            new EtwKernelFlag("PROC_THREAD", 0x3, "Process and Thread create/delete"),
            new EtwKernelFlag("LOADER", 0x4, "Kernel and user mode Image Load/Unload events"),
            new EtwKernelFlag("PROFILE", 0x1000000, "CPU Sample profile"),
            new EtwKernelFlag("CSWITCH", 0x10, "Context Switch"),
            new EtwKernelFlag("COMPACT_CSWITCH", 0x0, "Compact Context Switch"),
            new EtwKernelFlag("DISPATCHER", 0x800, "CPU Scheduler"),
            new EtwKernelFlag("DPC", 0x20, "DPC Events"),
            new EtwKernelFlag("INTERRUPT", 0x40, "Interrupt events"),
            new EtwKernelFlag("SYSCALL", 0x80, "System calls"),
            new EtwKernelFlag("ALPC", 0x100000, "Advanced Local Procedure Call"),
            new EtwKernelFlag("PERF_COUNTER", 0x8, "Process Perf Counters"),
            new EtwKernelFlag("DISK_IO", 0x300, "Disk I/O"),
            new EtwKernelFlag("DISK_IO_INIT", 0x400, "Disk I/O Initiation"),
            new EtwKernelFlag("FILE_IO", 0x2000000, "File system operation end times and results"),
            new EtwKernelFlag("FILE_IO_INIT", 0x4000000, "File system operation (create/open/close/read/write)"),
            new EtwKernelFlag("HARD_FAULTS", 0x2000, "Hard Page Faults"),
            new EtwKernelFlag("FILENAME", 0x200, "FileName (e.g., FileName create/delete/rundown)"),
            new EtwKernelFlag("SPLIT_IO", 0x200000, "Split I/O"),
            new EtwKernelFlag("REGISTRY", 0x20000, "Registry tracing"),
            new EtwKernelFlag("DRIVERS", 0x800200, "Driver events"),
            new EtwKernelFlag("NETWORKTRACE", 0x10000, "Network events (e.g., tcp/udp send/receive)"),
            new EtwKernelFlag("VIRT_ALLOC", 0x4000, "Virtual allocation reserve and release"),
            new EtwKernelFlag("ALL_FAULTS", 0x1000, "All page faults including hard, Copy on write, demand zero faults, etc."),
            new EtwKernelFlag("VAMAP", 0x8000, "MapFile info"),
            new EtwKernelFlag("MEMORY", 0x200, "Memory tracing"),

            /*
            // invalid flags
            new EtwKernelFlag("INTERRUPT_STEER", 0x0, "Interrupt Steering events"),
            new EtwKernelFlag("WDF_DPC", 0x0, "WDF DPC events"),
            new EtwKernelFlag("WDF_INTERRUPT", 0x0, "WDF Interrupt events"),
            new EtwKernelFlag("HIBERRUNDOWN", 0x0, "Rundown(s) during hibernate"),
            new EtwKernelFlag("KE_CLOCK", 0x0, "Clock Configuration events"),
            new EtwKernelFlag("PMC_PROFILE", 0x0, "PMC sampling events"),
            */

            /*
            // 0s? These need special APIs (like WPRControl API - http://msdn.microsoft.com/en-us/library/windows/desktop/hh849526.aspx)
            new EtwKernelFlag("PRIORITY", 0x0, "Priority change events"),
            new EtwKernelFlag("SPINLOCK", 0x0, "Spinlock Collisions"),
            new EtwKernelFlag("KQUEUE", 0x0, "Kernel Queue Enqueue/Dequeue"),
            new EtwKernelFlag("REG_HIVE", 0x0, "Registry hive tracing"),
            new EtwKernelFlag("POWER", 0x0, "Power management events"),
            new EtwKernelFlag("CC", 0x0, "Cache manager events"),
            new EtwKernelFlag("MEMINFO", 0x0, "Memory List Info"),
            new EtwKernelFlag("MEMORY", 0x0, "Memory tracing"),
            new EtwKernelFlag("REFSET", 0x0, "Support footprint analysis"),
            new EtwKernelFlag("MEMINFO_WS", 0x0, "Working set Info"),
            new EtwKernelFlag("CONTMEMGEN", 0x0, "Contiguous Memory Generation"),
            new EtwKernelFlag("POOL", 0x0, "Pool tracing"),
            new EtwKernelFlag("CPU_CONFIG", 0x0, "NUMA topology, Processor Group and Processor Index to Number mapping. By default it is always enabled."),
            new EtwKernelFlag("SESSION", 0x0, "Session rundown/create/delete events."),
            new EtwKernelFlag("IDLE_STATES", 0x0, "CPU Idle States"),
            new EtwKernelFlag("TIMER", 0x0, "Timer settings and its expiration"),
            new EtwKernelFlag("CLOCKINT", 0x0, "Clock Interrupt Events"),
            new EtwKernelFlag("IPI", 0x0, "Inter-processor Interrupt Events"),
            new EtwKernelFlag("OPTICAL_IO", 0x0, "Optical I/O"),
            new EtwKernelFlag("OPTICAL_IO_INIT", 0x0, "Optical I/O Initiation"),
            new EtwKernelFlag("FLT_IO_INIT", 0x0, "Minifilter callback initiation"),
            new EtwKernelFlag("FLT_IO", 0x0, "Minifilter callback completion"),
            new EtwKernelFlag("FLT_FASTIO", 0x0, "Minifilter fastio callback completion"),
            new EtwKernelFlag("FLT_IO_FAILURE", 0x0, "Minifilter callback completion with failure"),
            */
        };

        internal static List<EtwProviderType> _phoneProviders = new List<EtwProviderType>()
        {
            new EtwProviderType {Name = "3rd Party Developer Provider", GUID = "{7A68C8E0-BE90-4039-9048-2A69F64BDF3C}", Level = XmlProviderLevel.Verbose, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-Services", GUID = "{0063715b-eeda-4007-9429-ad526f62696e}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-Winsock-WS2HELP", GUID = "{d5c25f9a-4d47-493e-9184-40dd397a004d}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-Winsock-AFD", GUID = "{e53c6823-7bb8-44bb-90dc-3f86090d48a6}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-Kernel-Processor-Power", GUID = "{0f67e49f-fe51-4e9f-b490-6f2948cc6027}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-Base-Filtering-Engine-Connections", GUID = "{121d3da8-baf1-4dcb-929f-2d4c9a47f7ab}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-WindowsPhone-ProximityProvider", GUID = "{175fedee-7510-43fd-a600-a49e99594792}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-XAudio2", GUID = "{1ee3abdb-c1fc-4b43-9e56-11064abba866}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-Proximity-Common", GUID = "{28058203-d394-4afc-b2a6-2f9155a3bb95}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-MFH264Enc", GUID = "{2a49de31-8a5b-4d3a-a904-7fc7409ae90d}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-VWiFi", GUID = "{314b2b0d-81ee-4474-b6e0-c2aaec0ddbde}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-Kernel-Power", GUID = "{331c3b3a-2005-44c2-ac5e-77220c37d6b4}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-WindowsPhone-Power", GUID = "{5c103042-7e75-4629-a748-bdfa67607fac}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "TaskHost", GUID = "{5cbdf2a5-7290-4acc-b8a7-9ba285bebc39}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "LINQ2SQL", GUID = "{5f8e6c40-33c8-11e0-bc8e-0800200c9a66}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-WinHttp", GUID = "{7d44233d-3055-4b9c-ba64-0d47ca40a232}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-D3D10Level9", GUID = "{7e7d3382-023c-43cb-95d2-6f0ca6d70381}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-DxgKrnl", GUID = "{802ec45a-1e99-4b83-9920-87c98277ba9d}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-WindowsPhone-ZMediaQueue", GUID = "{8135a447-8c2f-400d-b40a-a9b9cb6e15d1}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-WindowsMobile-Compositor", GUID = "{85fffba1-cf12-402c-b9b3-29a3217bb007}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-Kernel-PnP", GUID = "{9c205a39-1250-487d-abd7-e831c6290539}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-WindowsPhone-Silverlight", GUID = "{aa087e0e-0b35-4e28-8f3a-440c3f51eef1}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "User32", GUID = "{b0aa8734-56f7-41cc-b2f4-de228e98b946}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "QCOM DX9 UMD", GUID = "{b840958e-7645-4281-aa14-611b3598640f}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "XNA", GUID = "{cd698436-a3e3-4607-bb60-0bac4d765b59}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-WindowsPhone-VideoProvider", GUID = "{d5e118bb-e2ca-42e5-8f5a-ba2f9b13660a}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-DirectWrite", GUID = "{d775f388-5a4a-474d-8726-7b255544285f}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-Direct3D11", GUID = "{db6f6ddb-ac77-4e88-8253-819df9bbf140}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Ntfs", GUID = "{dd70bc80-ef44-421b-8ac3-cd31da613a4e}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-WindowsPhone-Input", GUID = "{ed07ce1c-cee3-41e0-93e2-eeb312301848}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "DX", GUID = "{65cd4c8a-0848-4583-92a0-31c0fbaf00c0}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "UMD", GUID = "{a688ee40-d8d9-4736-b6f9-6b74935ba3b1}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-DXGI", GUID = "{ca11c036-0102-4a2d-a6ad-f03cfed5d3c9}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
            new EtwProviderType {Name = "Microsoft-Windows-MediaEngine", GUID = "{8f2048e0-f260-4f57-a8d1-932376291682}", Level = XmlProviderLevel.Warning, HexKeywords = "ffff"},
        };

        private static Dictionary<string, ProfilerSession> _predefinedScenarios = new Dictionary<string, ProfilerSession>();

        public static Dictionary<string, ProfilerSession> PredefinedScenarios { get { return _predefinedScenarios; } }

        static Profiler()
        {
            InitPredefinedScenarios();
        }

        private static void InitPredefinedScenarios()
        {
            // TODO: there has to be a better way (xml file?) to define these sessions

            _predefinedScenarios["Standard"] = new ProfilerSession();

            var standard = _predefinedScenarios["Standard"];
            standard.KernelFlags["PROC_THREAD"].IsEnabled = true;
            standard.KernelFlags["LOADER"].IsEnabled = true;
            standard.KernelFlags["DISK_IO"].IsEnabled = true;
            standard.KernelFlags["DISK_IO_INIT"].IsEnabled = true;
            standard.KernelFlags["HARD_FAULTS"].IsEnabled = true;
            standard.KernelFlags["PROFILE"].IsEnabled = true;
            standard.KernelFlags["MEMORY"].IsEnabled = true;
            standard.KernelFlags["CSWITCH"].IsEnabled = true;
            standard.KernelFlags["DISPATCHER"].IsEnabled = true;
            standard.KernelFlags["INTERRUPT"].IsEnabled = true;

            standard.KernelStackFlags["PROC_THREAD"].IsEnabled = true;
            standard.KernelStackFlags["LOADER"].IsEnabled = true;
            standard.KernelStackFlags["DISK_IO"].IsEnabled = true;
            standard.KernelStackFlags["HARD_FAULTS"].IsEnabled = true;
            standard.KernelStackFlags["PROFILE"].IsEnabled = true;
            standard.KernelStackFlags["CSWITCH"].IsEnabled = true;
            standard.KernelStackFlags["INTERRUPT"].IsEnabled = true;

            standard.Providers["3rd Party Developer Provider"].IsEnabled = true;
            standard.Providers["TaskHost"].IsEnabled = true;
            standard.Providers["Microsoft-Windows-D3D10Level9"].IsEnabled = true;
            standard.Providers["Microsoft-Windows-DirectWrite"].IsEnabled = true;
            standard.Providers["Microsoft-Windows-Direct3D11"].IsEnabled = true;

            standard.Providers["Microsoft-WindowsMobile-Compositor"].IsEnabled = true;
            standard.Providers["DX"].IsEnabled = true;
            standard.Providers["Microsoft-Windows-DXGI"].IsEnabled = true;
            standard.Providers["Microsoft-WindowsPhone-Silverlight"].IsEnabled = true;
            standard.Providers["Microsoft-Windows-WinHttp"].IsEnabled = true;
            standard.Providers["Microsoft-WindowsPhone-Input"].IsEnabled = true;

            _predefinedScenarios["Custom"] = new ProfilerSession(isCustom: true);
        }
    }
}
