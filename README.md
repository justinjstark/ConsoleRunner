# ConsoleRunner
A simple CRON scheduler using .NET Core and Quartz.NET

This is a work in progress. The goal is to build a CRON scheduler to launch console applications using .NET Core, Quartz.NET, Dependency Injection with appropriate scoping, Entity Framework for schedule persistence and run history, logging, and be able to install it as a windows service.

Features:
[X] Service resolution of job dependencies
[X] Per-job dependency resolution scoping
[X] Scheduling jobs via CRON expressions
[X] Allow jobs to run on startup
[X] Allow jobs to skip running if they are already running
[X] Quartz ShutdownHook plugin
[X] Log with Microsoft.Extensions.Logging
[ ] Run console applications
[ ] Ability to install as a Windows Service
[ ] Track run results (exit code, STDOUT, and STDERR)
[ ] Job queue tracking and warnings
[ ] Long-running job warnings (configurable per job)
[ ] Long-running job warnings (automatic based on job run history)
[ ] EntityFramework job store
[ ] EntityFramework run-history store
[ ] Web management interface
[ ] API / Observables for realtime job tracking
[ ] .NET Core 3
