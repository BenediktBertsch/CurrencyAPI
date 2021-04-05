# ExchangeRateAPI
API backend for currency data of the European Central Bank

## How to setup with docker
Use the dockerfile to create the container.
Neede environment variable DbString as "server=IPorHostname;database=dbName;uid=userName;pwd=userPassword"

## Endpoints
There are currently 3 available endpoints which are:
Latest:
```http
GET /latest
```
Specific date:
```http
GET /2018-03-26
```
Get historical rates for a time period:
```http
GET /history?start_at=2018-01-01&end_at=2018-09-01
```

### Every endpoint supports following parameters: 
Base:
```http
GET /latest?Base=USD
```
symbols:
```http
GET /latest?symbols=USD,HKD&Base=INR
```
