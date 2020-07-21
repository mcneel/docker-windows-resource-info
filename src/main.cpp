// main.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>

//#include <SDKDDKVer.h>
//
#include <Windows.h>
//
//#include <tchar.h>

#define MILLION 1000000

//Returns the last Win32 error, in string format. Returns an empty string if there is no error.
std::string GetLastErrorAsString()
{
    //Get the error message, if any.
    DWORD errorMessageID = ::GetLastError();
    if (errorMessageID == 0)
        return std::string(); //No error message has been recorded

    LPSTR messageBuffer = nullptr;
    size_t size = FormatMessageA(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
        NULL, errorMessageID, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPSTR)&messageBuffer, 0, NULL);

    std::string message(messageBuffer, size);

    //Free the buffer.
    LocalFree(messageBuffer);

    return message;
}

int main()
{
    std::cout << "Hello World!\n\n";

    // check to see if 

    /*
    # JOBOBJECT_CPU_RATE_CONTROL_INFORMATION

    https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-jobobject_cpu_rate_control_information

    Available since Windows 8 and Windows Server 2012.

    ## CpuRate
    
    Specifies the portion of processor cycles that the threads in a job object can use during each scheduling interval,
    as the number of cycles per 10,000 cycles. If the ControlFlags member specifies JOB_OBJECT_CPU_RATE_WEIGHT_BASED or
    JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE, this member is not used.

    ## Weight

    If the ControlFlags member specifies JOB_OBJECT_CPU_RATE_WEIGHT_BASED, this member specifies the scheduling weight
    of the job object, which determines the share of processor time given to the job relative to other workloads on the
    processor.
    
    This member can be a value from 1 through 9, where 1 is the smallest share and 9 is the largest share. The default
    is 5, which should be used for most workloads.

    If the ControlFlags member specifies JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE, this member is not used.

    ## MinRate

    Specifies the minimum portion of the processor cycles that the threads in a job object can reserve during each
    scheduling interval. Specify this rate as a percentage times 100. For example, to set a minimum rate of 50%,
    specify 50 times 100, or 5,000.

    For the minimum rates to work correctly, the sum of the minimum rates for all of the job objects in the system
    cannot exceed 10,000, which is the equivalent of 100%.

    ## MaxRate

    Specifies the maximum portion of processor cycles that the threads in a job object can use during each scheduling
    interval. Specify this rate as a percentage times 100. For example, to set a maximum rate of 50%, specify 50 times
    100, or 5,000.

    After the job reaches this limit for a scheduling interval, no threads associated with the job can run until the
    next scheduling interval.
    */

    JOBOBJECT_CPU_RATE_CONTROL_INFORMATION info;
    HANDLE hJobObject = CreateJobObject(NULL, NULL);

    std::cout << "Job Object CPU rate control information:\n";

    bool failed = false;

    if (::QueryInformationJobObject(NULL, JobObjectCpuRateControlInformation, &info,
                                    sizeof(JOBOBJECT_CPU_RATE_CONTROL_INFORMATION), NULL))
    {
        printf("  CPU rate: %d\n", info.CpuRate);
        printf("  Max rate: %d\n", info.MaxRate);
        printf("  Min rate: %d\n", info.MinRate);
        printf("  Weight: %d\n", info.Weight);
        printf("  Control flags: %d\n", info.ControlFlags);
        printf("    JOB_OBJECT_CPU_RATE_CONTROL_ENABLE: %d\n", info.ControlFlags & JOB_OBJECT_CPU_RATE_CONTROL_ENABLE);
        printf("    JOB_OBJECT_CPU_RATE_CONTROL_WEIGHT_BASED: %d\n", info.ControlFlags & JOB_OBJECT_CPU_RATE_CONTROL_WEIGHT_BASED);
        printf("    JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP: %d\n", info.ControlFlags & JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP);
        printf("    JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE: %d\n", info.ControlFlags & JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE);
    }
    else
    {
        std::cout << "ERROR: " << GetLastErrorAsString();
        failed = true;
    }

    std::cout << "\n";

    // get number of logical processors

    // when run inside a docker windows container, the value relates to
    // - process isolation = host machine
    // - hyper-v isolation = virtual machine

    SYSTEM_INFO sysinfo;
    GetSystemInfo(&sysinfo);
    int numCPU = sysinfo.dwNumberOfProcessors;

    printf("Cores: %d\n", numCPU);

    std::cout << "\n";

    // just for fun, check to see if there are any limits on the memory available to this process

    JOBOBJECT_EXTENDED_LIMIT_INFORMATION extended_info;

    std::cout << "Job Object extended limit information:\n";

    if (::QueryInformationJobObject(NULL, JobObjectExtendedLimitInformation, &extended_info, sizeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION), NULL))
    {
        printf("  Job memory limit: %d\n", extended_info.JobMemoryLimit);
        printf("  Process memory limit: %d\n", extended_info.ProcessMemoryLimit);
        printf("  Peak job memory used: %d\n", extended_info.PeakJobMemoryUsed);
        printf("  Peak process memory used: %d\n", extended_info.PeakProcessMemoryUsed);
    }
    else
    {
        std::cout << "ERROR: " << GetLastErrorAsString();
    }

    std::cout << "\n";

    // determine the number of "cpu cores" avilable to this process

    bool rate_enabled = !failed and info.ControlFlags & JOB_OBJECT_CPU_RATE_CONTROL_ENABLE;
    float cpus;

    if (rate_enabled)
    {
        std::cout << "CPU rate ENABLED\n";
        cpus = (float)info.CpuRate / 10000. * numCPU;
        //printf("Docker arg: --cpus %.2f\n", cpus);
    }
    else
    {
        std::cout << "CPU rate DISABLED\n";
        cpus = (float)numCPU;
    }

    printf("\nCPU cores available: %.2f\n\n", cpus);
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
