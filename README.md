# C++ Source Manager
### Overview
`C++ Source Manager` is a Visual Studio plugin that allows you to easily manage files and folders in a C++ project. The main advantage of this tool
comparing to others or a default Visual Studio file manager is that it makes it possible to separate source files from project files.

For example, we can have the following directory structure
```
source\
    myproject\
      systems\
        \*.cpp
        \*.h
      main\
        application.cpp
projects\
    myproject\
    myproject.sln
```
which makes it easier to use any build system, for example **Make** or **CMake**, to genarate project files for Visual Studio or similar IDEs.

### Features
Currently, `C++ Source Manager` supports
  - Creating a new source file (Automatically creates .cpp and .h files with very basic class template)
  - Adding a new directory to the project (which also generates filters for VS)

### Credits
* Many thanks to [Mads Kristensen](https://github.com/madskristensen/AddAnyFile) for providing some of the base code for creating VS Extensions.
