# com-port-to-database
The app allows reading and writing serial port data on Windows with MS SQL database.

## Requirements

- [SQL Server 2019 Express](https://www.microsoft.com/en-us/Download/details.aspx?id=101064) - Microsoft SQL Server Express is a version of Microsoft's SQL Server relational database management system that is free to download.
- [SSMS](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver15) - SSMS is an integrated environment for managing SQL Server infrastructure.
- [.NET Memory Profiler](https://memprofiler.com/download) - .NET Memory Profiler is a powerful tool for finding memory leaks and optimizing the memory usage in programs written in C#.
- [Visual Studio](https://visualstudio.microsoft.com/vs/community/) - A fully-featured, extensible, free IDE for creating modern applications for Android, iOS, Windows, as well as web applications and cloud services.
- [git](https://github.com/git/git) - Git is a fast, scalable, distributed revision control system.

## Common setup

Clone the repo and create and setup the app database 

        $ git clone https://github.com/yovchenko/com-port-to-database.git
        $ cd com-port-to-database
        $ database.sql
        
Create database using SSMS and database.sql
Set up a Microsoft SQL Server ODBC Data Source. For further information, please see the following link:
https://docs.microsoft.com/en-us/sql/odbc/admin/odbc-data-source-administrator?view=sql-server-ver15

DSN connection string attribute is specified in the connection string which you can find in *.config file  

## Description of the database 

| PK  | FK  |   Field 	|    Constraints       |    Type     | NN  |  Default |	      Description               | 
| --- | --- | --- | --- | --- | --- | --- | --- |
|  +  |     | port_name | CK__port_conf__port  | varchar(31) |	+  |          | Communications port name        |
|     |     | baud_rate |       	       | int	     |  +  |          | Serial baud rate                |
|     |     | data_bits | CK__port_conf__data  | smallint    |	+  |          | Data bits length                |
|     |     | stop_bits | CK__port_conf__stop  | varchar(31) |	+  |          | Number of stopbits per byte     |
|     |     | parity   	| CK__port_conf__parit | varchar(31) |	+  |          | Values of parity-checking       |
|     |     | handshake | CK__port_conf__hands | varchar(31) |	+  |          | One of the Handshake values     |
|     |     | timeout  	|     	               | int 	     |  +  |          | Milliseconds before a time-out  |

port_data table

| PK  | FK  |   Field 	|    Constraints       |    Type     | NN  |  Default |	      Description               | 
| --- | --- | --- | --- | --- | --- | --- | --- |
|  +  |     | id       	|                      | int         |	+  |          | Command queue number            |
|     |  +  | port_name |                      | varchar(31) |	+  |          | Communications port name        |
|     |     | send_data |      	               | varchar(MAX)|     |          | Command ASCII                   |
|     |     | res_data 	|    	               | varchar(MAX)|	   |          | Response ASCII                  |
|     |     | res_date 	|    	               | datetime    |	   |          | Response data and time          |

## NuGet packages 

- [Topshelf](https://www.nuget.org/packages/Topshelf/4.3.0?_src=template) - Topshelf is an open source project for hosting services without friction. 
- [log4net](https://www.nuget.org/packages/log4net/2.0.12?_src=template) - log4net is a tool to help the programmer output log statements to a variety of output targets.
- [RuntimeInformation](https://www.nuget.org/packages/System.Runtime.InteropServices.RuntimeInformation/4.3.0?_src=template) - Provides APIs to query about runtime and OS information.

## Getting started 

Open your project using Visual Studio and build the app.

To install the service, run the following 

        $ com-port-to-database.exe install

To uninstall the service, run the following 

        $ com-port-to-database.exe uninstall

## Application Structure

```
   .                 
    ├── com-port-to-database-unit-tests     # Unit test project (.NET Framework)
    │    ├── properties
    │    ├── ComPort.spec.cs 
    │    ├── Service.spec.cs 
    │    ├── app.config
    │    ├── com-port-to-database-unit-tests.csproj
    │    └── packages.config 
    ├── com-port-to-database                # Console app project (.NET Framework)
    │    ├── properties
    │    ├── app.config
    │    ├── Attributes.cs
    │    ├── ComPort.cs 
    │    ├── Program.cs 
    │    ├── Service.cs 
    │    ├── SqlData.cs 
    │    ├── app.config
    │    ├── com-port-to-database.csproj
    │    └── packages.config  
    ├── .gitignore   
    ├── database.sql                        # SQL file contains Structured Query Language
    ├── LICENSE
    ├── com-port-to-database.prfsession     # The prfsession file stores data for .NET memory profiler.
    └── com-port-to-database.sln
```

### Author and Contributor list 
---------------------------
Volodymyr M. Yovchenko

All bugs and fixes can be sent to volodymyr.yovchenko@gmail.com
