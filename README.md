# Animal AI for dot.Net
This is a [self-learning program](https://en.wikipedia.org/wiki/Decision_tree_learning), that identifies animals by asking questions.
Originally developed from:
* [Creative Computing, Morristown, New Jersey](https://en.wikipedia.org/wiki/Creative_Computing_(magazine))
* [BAISC Computer Games, Edited by David H. Ahl, published 1978](https://www.atariarchives.org/basicgames/index.php)
* [Animal (by Author Luehrmann, Nathan Teichholtz, Steve North)](https://www.atariarchives.org/basicgames/showpage.php?page=4)

The original program used a [Heap](https://en.wikipedia.org/wiki/Heap_(data_structure)) inside an array, for the question tree.
This version uses a database and an ORM wrapper instead. Giving us data persistence between runs.

## Setup SQL Server Local Database 
Run these steps from an administrative developer command prompt:
1. sqllocaldb create AnimalAI
1. sqllocaldb share AnimalAI AnimalAI
1. sqllocaldb start AnimalAI
1. sqllocaldb info AnimalAI
1. sqlcmd -S (localdb)\AnimalAI -Q "CREATE DATABASE AnimalDb"
1. AnimalAi.exe -setup

### There's a new Andriod version. It's not running yet.
See the [Android version](https://github.com/GregEakin/AnimalAi.android).

## Tools:
- [NHibernate](https://nhibernate.info/)
- [SQL Server Express LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [SQL Server Management Sdutio](https://docs.microsoft.com/en-us/sql/ssms/sql-server-management-studio-ssms)
- [SQLite](https://www.sqlite.org/index.html)
- [Unit Tests](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest)
- [Visual Studio](https://visualstudio.microsoft.com/)
- [ReSharper](https://www.jetbrains.com/resharper/)
- [Git Extensions](http://gitextensions.github.io/)

## Author
:fire: [Greg Eakin](https://www.linkedin.com/in/gregeakin)
