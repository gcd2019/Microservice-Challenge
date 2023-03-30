# Microservice-Challenge

# General Information
- IDE Used: Visual Studio 2022
- Framework: .Net 7.0
- Rate Provider used: Fixer.IO
- Database used: SQLite

# Source Code
- Controllers/CurrencyExchangeController: The API controller that handles currency exchange requests.
- Data/CurrencyExchangeDbContext: A subclass of DbContext in Entity Framework Core. DbContext is responsible for managing database access and mapping database entities to C# objects.
- Models/CurrencyExchangeRequest: Represents a request to exchange a certain amount of one currency for another.
- Models/CurrencyExchangeResponse: A model that represents a response to a currency exchange request.
- Models/CurrencyExchangeTrade: Represents a currency exchange trade in the database.
- FixerApiClient: Is responsible for interacting with the Fixer API to retrieve currency exchange rates.
- FixerConversionResponse: A simple POCO (Plain Old CLR Object) that represents a response from the Fixer API for currency conversion.

# Instructions
- Open the project on Visual Studio
- Press the run button (should be set on https)
- Then you'll be able to interact with the APIs on swagger
- You can create new exchange requests, get all the exchange requests saved in the DB and delete all requests saved in the DB.
