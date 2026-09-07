# Library Management System

A backend-focused Library Management System built with **C# and .NET**.

## Overview

This project manages library members, books, transactions, and book holds. It also includes file-based data persistence, audit logging, background services, automatic saving, daily backups, log cleanup, search suggestions, idle reminders, and concurrent access protection.

## Features

* Register new library members
* Add and remove books
* Check out books
* Return books
* Renew books
* Place and remove book holds
* View member transactions by date or date range
* Notify members when held books become available
* Save and load data using JSON files
* Continuous asynchronous audit logging
* Save unsaved data before exiting
* Automatic data saving every 2 minutes
* Daily data backups
* Background book search suggestions
* Automatic deletion of logs older than 30 days
* Idle reminder for unsaved changes
* Thread-safe operations for concurrent clerks

## Technologies

* **C#**
* **.NET 10**
* **System.Text.Json**
* **Serilog**
* **Serilog.Sinks.File**
* **Serilog.Sinks.Async**

## Project Structure

```text
LibraryManagementSystem
│
├── Models
│   ├── User.cs
│   ├── Book.cs
│   ├── Transaction.cs
│   └── Hold.cs
│
├── Services
│   ├── LibraryService.cs
│   ├── FileStorageService.cs
│   └── LogService.cs
│
└── Program.cs
```

## Data Storage

The system stores library data in JSON files:

* `users.json`
* `books.json`
* `transactions.json`
* `holds.json`

Daily backups are stored in the `Backups` folder.

Audit logs are stored in the `Logs` folder.

## Background Services

The system uses background timers for:

* Automatic saving every 2 minutes
* Daily backups at midnight
* Log cleanup
* Idle detection

Book search suggestions are also processed using background tasks.

## Concurrency

The system uses `lock` to protect shared library data when multiple clerks perform operations at the same time.

## Purpose

This project was developed as a backend training project to practice:

* Object-Oriented Programming
* Data Structures
* File Persistence
* JSON Serialization
* Logging
* Background Tasks
* Timers
* Thread Safety
* Basic Backend System Design

## Author

**Eman Ahmad**
