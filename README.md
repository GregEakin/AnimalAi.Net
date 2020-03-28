# Animal AI
This is a self-learning program, that identifies animals by asking questions.
Originally developed from:
* _Creative Computing, Morristown, New Jersey_
* [BAISC Computer Games, Edited by David H. Ahl, published 1978](https://www.atariarchives.org/basicgames/index.php)
* [Animal (by Author Luehrmann, Nathan Teichholtz, Steve North)](https://www.atariarchives.org/basicgames/showpage.php?page=4)

The original program used a [Heap](https://en.wikipedia.org/wiki/Heap_(data_structure)) inside an array, for the question tree.
This version uses a database and an ORM wrapper instead. Giving us data persistence between runs.

## Setup SQL Server Local Database 
Run these steps from an administrative developer command prompt:
1. sqllocaldb create ProjectsV13
1. sqllocaldb share ProjectsV13 ProjectsV13
1. sqllocaldb start ProjectsV13
1. sqllocaldb info ProjectsV13
1. sqlcmd -S (localdb)\ProjectsV13 -Q "CREATE DATABASE AnimalDb"
1. AnimalAi.exe -setup

## Links:
- [NHibernate](https://nhibernate.info/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server)
- [Microsoft SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- [Unit Tests](https://docs.microsoft.com/en-us/visualstudio/test/unit-test-your-code)
- [SQLite](https://www.sqlite.org/index.html)
- [Community Edition of Visual Studio](https://www.visualstudio.com/vs/community/)
- [Git Extensions](http://gitextensions.github.io/)

## Author
:fire: [Greg Eakin](https://www.linkedin.com/in/gregeakin)
