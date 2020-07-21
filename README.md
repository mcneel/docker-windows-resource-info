# Docker Windows Resource Info

A sample c++ project that uses Windows' Job Object API to determine the proportion of CPU available to a process running inside a Windows Docker container under process isolation.

```commandline
> docker build -t buildtools2019:latest -m 2GB --no-cache .

...
Successfully tagged buildtools2019:latest

> docker run -v D:\dev\tmp\BuildTools\src:C:\src --cpus=5.34 --isolation=process buildtools2019

Hello World!

Job Object CPU rate control information:
  CPU rate: 6675
  Max rate: 0
  Min rate: 6675
  Weight: 6675
  Control flags: 5
    JOB_OBJECT_CPU_RATE_CONTROL_ENABLE: 1
    JOB_OBJECT_CPU_RATE_CONTROL_WEIGHT_BASED: 0
    JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP: 4
    JOB_OBJECT_CPU_RATE_CONTROL_MIN_MAX_RATE: 0

Cores: 8

Job Object extended limit information:
  Job memory limit: 0
  Process memory limit: 0
  Peak job memory used: 33247232
  Peak process memory used: 4419584

CPU rate ENABLED

CPU cores available: 5.34
```