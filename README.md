# NachoCron
A CRON scheduler using .NET Core and Quartz.NET

This project is probably abandoned. I was going to build it up to use in production but ran into a roadblock. It turns out running an application as another user from a windows service is not possible without making low-level system calls.

The goal is to build a CRON scheduler to launch console applications using .NET Core, Quartz.NET, Dependency Injection with appropriate scoping, Entity Framework for schedule persistence and run history, logging, and be able to install it as a windows service.

Features:
- [x] Service resolution of job dependencies
- [x] Per-job-run dependency resolution scoping
- [x] Scheduling jobs via CRON expressions
- [x] Allow jobs to run on startup
- [x] Allow jobs to skip running if they are already running
- [x] Graceful shutdown with Quartz ShutdownHook plugin
- [x] Log with Microsoft.Extensions.Logging
- [x] Run console applications
- [x] Track run results (exit code, STDOUT, and STDERR)
- [x] Per-app timeouts
- [x] Long-running job warnings (configurable per job)
- [x] .NET Core 3 (Currently Preview 6)
- [x] Ability to install as a Windows Service (using .NET Core 3 Service Worker)
- [ ] Run-as-user ✖╭╮✖
- [ ] Job queue tracking and warnings
- [ ] Long-running job warnings (automatic based on job run history)
- [ ] EntityFramework job store
- [ ] EntityFramework run-history store
- [ ] Web management interface
- [ ] API / Observables for realtime job tracking
