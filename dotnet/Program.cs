using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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

            bool bRet = QueryInformationJobObject(hJob, JOBOBJECTINFOCLASS.JobObjectCpuRateControlInformation, pJobpil, nLength, out _);

            Console.WriteLine("Job Object CPU rate control information:");

            if (bRet)
            {
                var info = (JOBOBJECT_CPU_RATE_CONTROL_INFORMATION)Marshal.PtrToStructure(pJobpil, typeof(JOBOBJECT_CPU_RATE_CONTROL_INFORMATION));

                Console.WriteLine($"  CPU rate: {info.CpuRate}");
                //Console.WriteLine($"  Max rate: {info.MaxRate}"); // not implemented
                //Console.WriteLine($"  Min rate: {info.MinRate}"); // not implemented
                Console.WriteLine($"  Weight: {info.Weight}");
                Console.WriteLine($"  Control flags: {info.ControlFlags}");
                Console.WriteLine("    JOB_OBJECT_CPU_RATE_CONTROL_ENABLE:       {0}", info.ControlFlags.HasFlag(CpuFlags.JOB_OBJECT_CPU_RATE_CONTROL_ENABLE));
                Console.WriteLine("    JOB_OBJECT_CPU_RATE_CONTROL_WEIGHT_BASED: {0}", info.ControlFlags.HasFlag(CpuFlags.JOB_OBJECT_CPU_RATE_CONTROL_WEIGHT_BASED));
                Console.WriteLine("    JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP:     {0}", info.ControlFlags.HasFlag(CpuFlags.JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP));
                Console.WriteLine("    JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE: {0}", info.ControlFlags.HasFlag(CpuFlags.JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE));

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
        }

        public enum CpuFlags
        {
            JOB_OBJECT_CPU_RATE_CONTROL_ENABLE = 0x00000001,
            JOB_OBJECT_CPU_RATE_CONTROL_WEIGHT_BASED = 0x00000002,
            JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP = 0x00000004,
            JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE = 0x00000010
        }
    }
}
