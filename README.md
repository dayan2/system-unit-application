## System Unit Application – .NET Core C# Web API with MS SQL Integration

This project is a .NET Core Web API built with C#. It fetches data from a public API at [https://api.restful-api.dev/objects](https://api.restful-api.dev/objects), extracts needed fields, and stores them in a SQL Server database using raw SQL. The API has two endpoints: one to get all `SystemUnit` records, and another to get a `SystemUnit` by `ExternalId`.

Each `SystemUnit` has these fields (some may be empty depending on the data):
- ExternalId
- Name
- CPUModel
- HardDiskSize
- Year
- Price

The project includes basic error handling and is easy to run and set up.

### Build and Run the Web API Project

#### Step A: Navigate to your project folder
```bash
cd /path/to/your/project
```

#### Step B: Restore packages and build the project
```bash
dotnet restore
dotnet build --configuration Release
```

#### Step C: Run the project
```bash
dotnet run --configuration Release
```


I have also created a short video explaining the implementation and showing the app running in Swagger UI.  

👉 [Watch the video](https://youtu.be/WPMEXMUZisM)
