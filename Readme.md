# Nuboj.Archiver.ETL
A solution to extract data from Azure Blob and save them on a Postgresql DB

## Requirements
A docker volume named DATA_FOLDER, that will store logs and the downloaded files
```
docker create volume DATA_FOLDER
```

Mount the volume ACCOUNT_FOLDER

Both of the folders required with example data are already there in Solution.

## Start the solution

All the solution is based on docker for linux. So for start ETL Jobs run the following command:
```
docker-compose up -d
```

## Explanation

Every project continually run thanks to a while loop set to true. There are checks in order to avoid to save the same data twice.
One of the requirements of the Transferer project was to get a strart and an end datetime as env vars, but unfortunately the developer 
was not able to achieve using docker as host.     
By the way the logic to get the start and end datetime form args passed via DOTNET CLI:
```
dotnet run Nuboj.Archiver.ETL.Transferer.dll "2022-04-04T00:00:00" "2022-04-04T23:00:00"
```
or via the Enviroment Variables:
```
STARTDATETIME=
ENDDATETIME=
```
is already coded. 
If none of the previous is provided, the program will set start and end datetime to previous day from 00:00 to 23:00.    

As part of the logic to avoid data duplication every project when has finished his job will create a file name denominated {start-nameoftheproject}
(Ex: start-saver.txt) into the DATA_FOLDER.

The docker compose contains also a postgres and pgadmin service.