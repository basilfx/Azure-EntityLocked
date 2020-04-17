# Azure Functions: Durable Entity Locked

## Introduction
This repository contains a sample application, to demonstrate a problem with
Azure Durable Entities. It is based on the example code in the [documentation](https://github.com/MicrosoftDocs/azure-docs/blob/master/articles/azure-functions/durable/durable-functions-entities.md#example-transfer-funds-c).

## Problem description.
Azure Durable Entities offer critical sections, to ensure exclusive access to
an entity. In this sample application, it is used to ensure that only one
process can invoce another activity, from within an orchestrator.

As far as I know, this doesn't violate any of the [rules](https://github.com/MicrosoftDocs/azure-docs/blob/master/articles/azure-functions/durable/durable-functions-entities.md#critical-section-rules).

The problem is that, when the process/host gets killed within the critical
section, the entity will stay locked forever. This also happens if an exception
is raised from within a critical section, or if you stop debugging inside a
critical section.

The result is that any future invocation will not happen, even if you restart
the function: the lock is claimed and never released because it does not
time-out. Deleting the entity does not help. The only solution that 'works for
me' is to clear the whole state using using the storage explorer.

When this problem occours, messages similar to this are logged:

```
Function 'TimerTrigger-Orchestrator (Orchestrator)' is waiting for input. Reason: WaitForLockAcquisitionCompleted
```

## Reproduction steps
* Start the function, locally.
* Verify that it is running (it should log "Performing hard work")
* Set a break point in the critical section, around line 35 in
  `src/EntityLocked/TimerTrigger.cs`.
* When hitting this breakpoint, stop debugging.
* Start the function again.
* Verify that it does not hit the breakpoint anymore.
* Stop the function.

Bonus steps:
* Clear the local storage emulator (TestHubNameHistory, TestHubNameInstances).
* Start the function again.
* Verify that the breakpoint is hit again.
