# Market Data Importer

A C# Headless Windows Application that automates the downloading of historical market closing data for active portfolio symbols. The application fetches data from Stooq (CSV format), updates a local SQL Server database, verifies portfolio integrity, and conditionally triggers composite price calculations.

## Features

* **Automated Data Retrieval:** Downloads daily closing prices (since Jan 1, 2017) for all symbols marked `Active` in the `[dbo].[Portfolio]` table.
* **Stooq Integration:** Uses Stooq.com as the data source (no API key required).
* **Smart Upsert:** Inserts new records only if they do not already exist (Insert-If-New) to prevent duplicates.
* **Data Integrity Check (Test #1):** Verifies that the sum of the `Percent` column for each `TaxType` in the portfolio equals exactly 100%.
* **Conditional Processing:** The stored procedure `usp_CalculateCompositePrices` is executed **only if**:
    1.  New market data records were successfully inserted.
    2.  The Data Integrity Check (Test #1) passes for all Tax Types.
* **Robust Logging:** Detailed execution logs are written to daily rolling files using Serilog.

## Prerequisites

* **.NET 6.0 SDK** (or later)
* **SQL Server** (LocalDB, Express, or Standard)
* **Database Schema:**
    * Table: `[dbo].[Portfolio]` (Columns: `Symbol`, `Active`, `TaxType`, `Percent`)
    * Table: `[dbo].[ClosingPrices]` (Columns: `Closing`, `Symbol`, `Price`, `LastUpdate`)
    * Stored Procedure: `[dbo].[usp_CalculateCompositePrices]`

    * All the required database schema objects can be created using the DDL kept in this GitHub repository: [Guyton-Klinger-Withdrawals](https://github.com/CaveArnold/Guyton-Klinger-Withdrawals)

## Installation & Setup

1.  **Clone the repository:**
    ```bash
    git clone [https://github.com/your-username/market-data-importer.git](https://github.com/your-username/market-data-importer.git)
    ```

2.  **Configure Connection String:**
    Open `Program.cs` and update the `ConnectionString` variable to match your SQL Server instance:
    ```csharp
    private const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Guyton-Klinger-Withdrawals;Trusted_Connection=True;TrustServerCertificate=True;";
    ```

3.  **Install Dependencies:**
    Navigate to the project folder and run:
    ```bash
    dotnet restore
    ```

## Usage

Run the application from the command line or via a scheduled task. It requires no user interaction.

```bash
dotnet run