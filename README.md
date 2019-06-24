# ConsoleRunner
A simple CRON scheduler using .NET Core and Quartz.NET

This is a work in progress. The goal is to build a CRON scheduler to launch console applications using .NET Core, Quartz.NET, Dependency Injection with appropriate scoping, Entity Framework for schedule persistence and run history, logging, and be able to install it as a windows service.

Features:
- [x] Service resolution of job dependencies
- [x] Per-job dependency resolution scoping
- [x] Scheduling jobs via CRON expressions
- [x] Allow jobs to run on startup
- [x] Allow jobs to skip running if they are already running
- [x] Quartz ShutdownHook plugin
- [x] Log with Microsoft.Extensions.Logging
- [x] Run console applications
- [x] Track run results (exit code, STDOUT, and STDERR)
- [x] Per-app timeouts
- [x] Long-running job warnings (configurable per job)
- [ ] Ability to install as a Windows Service
- [ ] Job queue tracking and warnings
- [ ] Long-running job warnings (automatic based on job run history)
- [ ] EntityFramework job store
- [ ] EntityFramework run-history store
- [ ] Web management interface
- [ ] API / Observables for realtime job tracking
- [ ] .NET Core 3
