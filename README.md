# Docker Windows Resource Info

A sample c# project that uses Windows' Job Object API to determine the proportion of CPU available to a process running inside a Windows Docker container under process isolation.

```commandline
> docker build -t jobj_dotnet .

...
Successfully tagged jobj_dotnet:latest

> docker run --rm --cpus=3.53 --isolation=process jobj_dotnet

Job Object CPU rate control information:
  CPU rate: 4412
  Weight: 4412
  Control flags: 5
    JOB_OBJECT_CPU_RATE_CONTROL_ENABLE:       True
    JOB_OBJECT_CPU_RATE_CONTROL_WEIGHT_BASED: False
    JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP:     True
    JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE: False

Total cpu 'cores': 8

CPU 'cores' available: 3.53
```