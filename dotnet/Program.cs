using System;
using System.Runtime.InteropServices;

namespace JobObjectInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            // get number of logical processors

            // when run inside a docker windows container, the value relates to
            // - process isolation = host machine
            // - hyper-v isolation = virtual machine (value of --cpus rounded up to nearest whole number)

            double cpus = Environment.ProcessorCount;

            // get job oject rate control information

            IntPtr hJob = IntPtr.Zero; // query job object associated with calling process
            int nLength = Marshal.SizeOf(typeof(JOBOBJECT_CPU_RATE_CONTROL_INFORMATION));
            IntPtr pJobpil = Marshal.AllocHGlobal(nLength);

            Console.WriteLine("Job Object CPU rate control information:");

            if (QueryInformationJobObject(hJob, JOBOBJECTINFOCLASS.JobObjectCpuRateControlInformation, pJobpil, nLength, out _))
            {
                var info = (JOBOBJECT_CPU_RATE_CONTROL_INFORMATION)Marshal.PtrToStructure(pJobpil, typeof(JOBOBJECT_CPU_RATE_CONTROL_INFORMATION));

                Console.WriteLine($"  CPU rate: {info.CpuRate}");
                Console.WriteLine($"  Weight: {info.Weight}");
                Console.WriteLine($"  Min rate: {info.MinMax.MinRate}"); // not implemented
                Console.WriteLine($"  Max rate: {info.MinMax.MaxRate}"); // not implemented
                Console.WriteLine($"  Control flags: {info.ControlFlags}");
                //Console.WriteLine("    JOB_OBJECT_CPU_RATE_CONTROL_ENABLE:       {0}", info.ControlFlags.HasFlag(CpuFlags.JOB_OBJECT_CPU_RATE_CONTROL_ENABLE));
                //Console.WriteLine("    JOB_OBJECT_CPU_RATE_CONTROL_WEIGHT_BASED: {0}", info.ControlFlags.HasFlag(CpuFlags.JOB_OBJECT_CPU_RATE_CONTROL_WEIGHT_BASED));
                //Console.WriteLine("    JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP:     {0}", info.ControlFlags.HasFlag(CpuFlags.JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP));
                //Console.WriteLine("    JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE: {0}", info.ControlFlags.HasFlag(CpuFlags.JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE));

                // JOBOBJECT_CPU_RATE_CONTROL_INFORMATION.CpuRate
                // 
                // Specifies the portion of processor cycles that the threads in a job object can use during each
                // scheduling interval, as the number of cycles per 10,000 cycles. If the ControlFlags member specifies
                // JOB_OBJECT_CPU_RATE_WEIGHT_BASED or JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE, this member is not
                // used.

                if (info.ControlFlags.HasFlag(CpuFlags.JOB_OBJECT_CPU_RATE_CONTROL_ENABLE)
                    && !info.ControlFlags.HasFlag(CpuFlags.JOB_OBJECT_CPU_RATE_CONTROL_WEIGHT_BASED)
                    && !info.ControlFlags.HasFlag(CpuFlags.JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE))
                {
                    cpus *= info.CpuRate / 10000f;
                }
            }
            else
            {
                //Console.WriteLine(Marshal.GetLastWin32Error()); // returns '0'?!
                var msg = "ERROR: failed to get CPU rate control information for the job object associated with the calling process\n" +
                          "       (it might not exist...)";
                Console.WriteLine(msg);
            }

            // get job object memory limit (via extended limit information)

            nLength = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            pJobpil = Marshal.AllocHGlobal(nLength);

            Console.WriteLine("\nJob Object extended limit information:");

            if (QueryInformationJobObject(hJob, JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation, pJobpil, nLength, out _))
            {
                var extended_info = (JOBOBJECT_EXTENDED_LIMIT_INFORMATION)Marshal.PtrToStructure(pJobpil, typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
                Console.WriteLine($"  Job memory limit: {extended_info.JobMemoryLimit}");
                Console.WriteLine($"  Process memory limit: {extended_info.ProcessMemoryLimit}");
            }
            else
            {
                Console.WriteLine("ERROR");
            }

            Console.WriteLine($"\nTotal cpu 'cores': {Environment.ProcessorCount}");

            Console.WriteLine($"\nCPU 'cores' available: {cpus:0.00}");
        }

        [DllImport("kernel32.dll")]
        static extern bool QueryInformationJobObject(IntPtr hJob, JOBOBJECTINFOCLASS JobObjectInfoClass, [Out, MarshalAs(UnmanagedType.SysUInt)] IntPtr lpJobObjectInfo, int cbJobObjectInfoLength, out int lpReturnLength);

        public enum JOBOBJECTINFOCLASS
        {
            JobObjectAssociateCompletionPortInformation = 7,
            JobObjectBasicLimitInformation = 2,
            JobObjectBasicUIRestrictions = 4,
            JobObjectEndOfJobTimeInformation = 6,
            JobObjectExtendedLimitInformation = 9,
            JobObjectSecurityLimitInformation = 5,
            JobObjectCpuRateControlInformation = 15
        }

        [StructLayout(LayoutKind.Explicit)]
        //[CLSCompliant(false)]
        struct JOBOBJECT_CPU_RATE_CONTROL_INFORMATION
        {
            [FieldOffset(0)]
            public CpuFlags ControlFlags;
            [FieldOffset(4)]
            public UInt32 CpuRate;
            [FieldOffset(4)]
            public UInt32 Weight;
            [FieldOffset(4)]
            public DummyStruct MinMax;
            //[FieldOffset(8)]
            //public UInt16 MinRate;
            //[FieldOffset(12)]
            //public UInt16 MaxRate;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DummyStruct
        {
            public UInt16 MinRate;
            public UInt16 MaxRate;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public Int64 PerProcessUserTimeLimit;
            public Int64 PerJobUserTimeLimit;
            public JOBOBJECTLIMIT LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public UInt32 ActiveProcessLimit;
            public Int64 Affinity;
            public UInt32 PriorityClass;
            public UInt32 SchedulingClass;
        }

        [Flags]
        public enum JOBOBJECTLIMIT : uint
        {
            // Basic Limits
            Workingset = 0x00000001,
            ProcessTime = 0x00000002,
            JobTime = 0x00000004,
            ActiveProcess = 0x00000008,
            Affinity = 0x00000010,
            PriorityClass = 0x00000020,
            PreserveJobTime = 0x00000040,
            SchedulingClass = 0x00000080,

            // Extended Limits
            ProcessMemory = 0x00000100,
            JobMemory = 0x00000200,
            DieOnUnhandledException = 0x00000400,
            BreakawayOk = 0x00000800,
            SilentBreakawayOk = 0x00001000,
            KillOnJobClose = 0x00002000,
            SubsetAffinity = 0x00004000,

            // Notification Limits
            JobReadBytes = 0x00010000,
            JobWriteBytes = 0x00020000,
            RateControl = 0x00040000,
        }

        [Flags]
        public enum CpuFlags
        {
            JOB_OBJECT_CPU_RATE_CONTROL_ENABLE = 0x00000001,
            JOB_OBJECT_CPU_RATE_CONTROL_WEIGHT_BASED = 0x00000002,
            JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP = 0x00000004,
            JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE = 0x00000010
        }
    }
}
