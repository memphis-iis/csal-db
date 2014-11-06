csal-db - CSAL Database Access and UI
=======================================

 > IMPORTANT! All pull requests are welcome, but please work from the dev
 > branch. The master branch is for current release and release history.

This repository contains the database functionality for the current CSAL
online learning project.  This code is mainly developed in C# targeting .NET
4.0.  All projects and solutions are created and edited with Visual Studio
2013.

Current directories
----------------------

 * csaldbweb_mock - The current home of the mock-up for the Teachers' web
   interface to the database
 * CSALMongo - the actual database access library, which presents a typed,
   usable class library for access the database. Also accepts raw JSON records
   from ACE for turns in a user lesson session.  (Please see the online document
   _CSAL Data_)
 * CSALMongoUnitTest - unit tests for CSALMongo
 * CSALMongoWebAPI - the Web API wrapper around CSALMongo _and_ the host for the
   Teachers GUI (uses the Web API)
 * CSALMongoWebAPI.Tests - unit tests for CSALMongoWebAPI
 * packages - the NuGet folder managing packages for these projects
 * scripts - scripts that are useful for dealing with this project, especially
   the database.  Generally in Python.
