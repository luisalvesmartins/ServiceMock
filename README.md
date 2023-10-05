# ServiceMock
Service Mock Storage Management API- small C# API project to store request and response pair and also missed requests

This project is to be used locally under your own security closed network.

Can't be simpler than this.

## Features:
- Store a request/response pair
- Return a response of a request
- Store missed responses for later fill
- Data is stored in a json file. If you need a massive data base and horizontal scalability then you should create the interface to a non relational database. Azure Storage or CosmosDB would work perfectly.
- Data is persisted after each call to the store endpoint but could be done based on a time interval
- Point DATAFILE to the location where you want to store the database.

## API:

### /retrieve - GET

return the response for a given request

``` 
GET http://(hostname)/retrieve?r=value
```

### /store - POST

store the pair request/response

``` 
POST http://(hostname)/store?r=value

content
```

### /missing - GET

returns all the missed requests for later fix. The fix is obtained by calling the store endpoint with the missing response.

``` 
GET http://(hostname)/missing
```