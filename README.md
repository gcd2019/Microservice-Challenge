# Microservice-Challenge

# General Information
- IDE Used: Visual Studio 2022
- Framework: .Net 7.0
- Rate Provider used: Fixer.IO
- Database used: SQLite

# Source Code
- CurrencyExchangeController: The API controller that handles currency exchange requests.
- CurrencyExchangeDbContext: A subclass of DbContext in Entity Framework Core. DbContext is responsible for managing database access and mapping database entities to C# objects.
- CurrencyExchangeRequest: Represents a request to exchange a certain amount of one currency for another.
- CurrencyExchangeResponse: A model that represents a response to a currency exchange request.
- CurrencyExchangeTrade: Represents a currency exchange trade in the database.
- FixerApiClient: Is responsible for interacting with the Fixer API to retrieve currency exchange rates.
- FixerConversionResponse: A simple POCO (Plain Old CLR Object) that represents a response from the Fixer API for currency conversion.
