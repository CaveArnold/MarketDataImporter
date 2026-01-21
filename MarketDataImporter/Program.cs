/* * Developer: Cave Arnold
 * AI Assistant: Gemini
 * Date: January 21, 2026
 * Version: 1.0.0
 * 
/*
 * ==============================================================================================
 * MARKDOWN README / DOCUMENTATION
 * ==============================================================================================
 * * # Market Data Importer
 *
 * A C# Headless Windows Application that automates the downloading of historical market closing data for 
 * active portfolio symbols. The application fetches data from Stooq (CSV format), updates a local 
 * SQL Server database, verifies portfolio integrity, and conditionally triggers composite price 
 * calculations.
 *
 * ## Features
 *
 * * **Automated Data Retrieval:** Downloads daily closing prices (since Jan 1, 2017) for all 
 * symbols marked `Active` in the `[dbo].[Portfolio]` table.
 * * **Stooq Integration:** Uses Stooq.com as the data source (no API key required).
 * * **Smart Upsert:** Inserts new records only if they do not already exist (Insert-If-New) 
 * to prevent duplicates.
 * * **Data Integrity Check (Test #1):** Verifies that the sum of the `Percent` column for each 
 * `TaxType` in the portfolio equals exactly 100%.
 * * **Conditional Processing:** The stored procedure `usp_CalculateCompositePrices` is executed 
 * **only if**:
 * 1. New market data records were successfully inserted.
 * 2. The Data Integrity Check (Test #1) passes for all Tax Types.
 * * **Robust Logging:** Detailed execution logs are written to daily rolling files using Serilog.
 *
 * ## Prerequisites
 *
 * * **.NET 6.0 SDK** (or later)
 * * **SQL Server** (LocalDB, Express, or Standard)
 * * **Database Schema:**
 * * Table: `[dbo].[Portfolio]` (Columns: `Symbol`, `Active`, `TaxType`, `Percent`)
 * * Table: `[dbo].[ClosingPrices]` (Columns: `Closing`, `Symbol`, `Price`, `LastUpdate`)
 * * Stored Procedure: `[dbo].[usp_CalculateCompositePrices]`
 *
 * ## Usage
 *
 * Run the application from the command line or via a scheduled task. It requires no user interaction.
 *
 * dotnet run
 *
 * The console output will be minimal. Full details are written to the `logs` directory.
 *
 * ## Logging
 *
 * Logs are stored in the `\bin\Debug\netX.X\logs\` directory. A new log file is created for 
 * each day (e.g., `market-data-20260121.txt`).
 *
 * ### Sample Log Output
 *
 * 2026-01-21 18:00:00.881 -05:00 [INF] ==================================================================
 * 2026-01-21 18:00:00.932 -05:00 [INF] Application Started at 1/21/2026 6:00:00 PM
 * 2026-01-21 18:00:01.422 -05:00 [INF] Found 9 active symbols. Starting downloads...
 * 2026-01-21 18:00:01.423 -05:00 [INF] Processing Symbol: LQD
 * 2026-01-21 18:00:07.371 -05:00 [INF]    -> Success: 1 new records inserted for LQD.
 * 2026-01-21 18:00:07.879 -05:00 [INF] Processing Symbol: SCHP
 * 2026-01-21 18:00:14.699 -05:00 [INF]    -> Success: 1 new records inserted for SCHP.
 * 2026-01-21 18:00:15.214 -05:00 [INF] Processing Symbol: VEA
 * 2026-01-21 18:00:17.688 -05:00 [INF]    -> Success: 1 new records inserted for VEA.
 * 2026-01-21 18:00:18.195 -05:00 [INF] Processing Symbol: VIG
 * 2026-01-21 18:00:26.098 -05:00 [INF]    -> Success: 1 new records inserted for VIG.
 * 2026-01-21 18:00:26.614 -05:00 [INF] Processing Symbol: VNQ
 * 2026-01-21 18:00:29.821 -05:00 [INF]    -> Success: 1 new records inserted for VNQ.
 * 2026-01-21 18:00:30.334 -05:00 [INF] Processing Symbol: VTEB
 * 2026-01-21 18:00:38.952 -05:00 [INF]    -> Success: 1 new records inserted for VTEB.
 * 2026-01-21 18:00:39.454 -05:00 [INF] Processing Symbol: VTI
 * 2026-01-21 18:00:41.080 -05:00 [INF]    -> Success: 1 new records inserted for VTI.
 * 2026-01-21 18:00:41.584 -05:00 [INF] Processing Symbol: VWO
 * 2026-01-21 18:00:45.290 -05:00 [INF]    -> Success: 1 new records inserted for VWO.
 * 2026-01-21 18:00:45.797 -05:00 [INF] Processing Symbol: VWOB
 * 2026-01-21 18:00:52.304 -05:00 [INF]    -> Success: 1 new records inserted for VWOB.
 * 2026-01-21 18:00:52.811 -05:00 [INF] Download cycle complete. Total new records inserted: 9
 * 2026-01-21 18:00:52.812 -05:00 [INF] Running Test #1: Verifying TaxType Percentages...
 * 2026-01-21 18:00:52.821 -05:00 [INF]    -> PASS: TaxType 'Tax Deferred' sums to 100.00%.
 * 2026-01-21 18:00:52.823 -05:00 [INF]    -> PASS: TaxType 'Tax Free' sums to 100.00%.
 * 2026-01-21 18:00:52.824 -05:00 [INF]    -> PASS: TaxType 'Taxable' sums to 100.00%.
 * 2026-01-21 18:00:52.824 -05:00 [INF] Validation Passed AND New Records Found. Calling 'usp_CalculateCompositePrices'...
 * 2026-01-21 18:00:52.865 -05:00 [INF] Successfully executed 'usp_CalculateCompositePrices'.
 * 2026-01-21 18:00:52.865 -05:00 [INF] Application Finished at 1/21/2026 6:00:52 PM
 *
 * ## Troubleshooting
 *
 * * **Network Error / SQL Connection Failed:** Ensure your `ConnectionString` in `Program.cs` is pointing 
 * to the correct instance (e.g., `(localdb)\MSSQLLocalDB` vs `.\SQLEXPRESS`).
 * * **No Data Inserted:**
 * * Check if the symbol is marked `Active = 1` in the database.
 * * Ensure the symbol is valid on Stooq (US tickers may need the `.US` suffix, which the app handles automatically).
 * * The database might already be up to date.
 * * **"Validation FAILED":** If the logs show Test #1 failed, check the `Portfolio` table. The sum of 
 * `Percent` for that specific `TaxType` must equal exactly 100. The stored procedure will not run until this is fixed.
 *
 * ==============================================================================================
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Serilog;

namespace MarketDataImporter
{
    class Program
    {
        // Connection String
        private const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Guyton-Klinger-Withdrawals;Trusted_Connection=True;TrustServerCertificate=True;";

        private static readonly HttpClient _httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            // 1. Setup Serilog (File Only)
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs\\market-data-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Console.WriteLine("Application Started. Check logs folder for details.");
            Log.Information("==================================================================");
            Log.Information($"Application Started at {DateTime.Now}");

            try
            {
                // 2. Get Symbols
                var symbols = GetActivePortfolioSymbols();
                int totalNewRecords = 0; // Track total insertions across all symbols

                if (symbols.Count == 0)
                {
                    Log.Warning("No active symbols found in the Portfolio table.");
                }
                else
                {
                    Log.Information($"Found {symbols.Count} active symbols. Starting downloads...");

                    // 3. Process Each Symbol
                    foreach (var symbol in symbols)
                    {
                        // Accumulate the count of inserted records
                        totalNewRecords += await ProcessSymbolAsync(symbol);

                        // Polite delay
                        await Task.Delay(500);
                    }
                }

                Log.Information($"Download cycle complete. Total new records inserted: {totalNewRecords}");

                // 4. Run Test #1 (Validate TaxType Percentages)
                bool validationPassed = await RunPortfolioValidation();

                // 5. Conditional Execution of Stored Procedure
                if (validationPassed)
                {
                    if (totalNewRecords > 0)
                    {
                        Log.Information("Validation Passed AND New Records Found. Calling 'usp_CalculateCompositePrices'...");
                        await ExecuteCompositeCalculationAsync();
                    }
                    else
                    {
                        Log.Information("Validation Passed, but no new records were loaded. Stored Procedure skipped.");
                    }
                }
                else
                {
                    Log.Error("Validation FAILED. Stored Procedure 'usp_CalculateCompositePrices' was SKIPPED.");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Critical Application Failure");
            }
            finally
            {
                Log.Information($"Application Finished at {DateTime.Now}");
                Log.CloseAndFlush();
                Console.WriteLine("Done. Press any key to exit.");
                Console.ReadKey();
            }
        }

        // --- NEW: Test #1 Logic ---
        private static async Task<bool> RunPortfolioValidation()
        {
            Log.Information("Running Test #1: Verifying TaxType Percentages...");
            bool allPassed = true;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();

                // Query: Sum 'Percent' by 'TaxType' for Active rows only
                string sql = @"
                    SELECT TaxType, SUM([Percent]) as TotalPercent
                    FROM [dbo].[Portfolio]
                    WHERE Active = 1
                    GROUP BY TaxType";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                    {
                        Log.Warning("Test #1: No Active Portfolio data found to validate.");
                        return false;
                    }

                    while (await reader.ReadAsync())
                    {
                        // Handle potential DBNulls safely
                        string taxType = reader.IsDBNull(0) ? "NULL" : reader.GetString(0);

                        // Treat percentage as decimal. 
                        // Note: If you store it as INT, change to GetInt32(1)
                        decimal totalPercent = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1));

                        if (totalPercent == 100m)
                        {
                            Log.Information($"  -> PASS: TaxType '{taxType}' sums to {totalPercent}%.");
                        }
                        else
                        {
                            Log.Error($"  -> FAIL: TaxType '{taxType}' sums to {totalPercent}% (Expected 100%).");
                            allPassed = false;
                        }
                    }
                }
            }

            return allPassed;
        }

        // --- NEW: Stored Procedure Logic ---
        private static async Task ExecuteCompositeCalculationAsync()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("usp_CalculateCompositePrices", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        // Set timeout higher if this calculation is heavy (e.g. 5 minutes)
                        cmd.CommandTimeout = 300;

                        await cmd.ExecuteNonQueryAsync();
                        Log.Information("Successfully executed 'usp_CalculateCompositePrices'.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error executing Stored Procedure 'usp_CalculateCompositePrices'.");
            }
        }

        // --- Existing Helper Methods (Slightly modified to return int) ---

        private static List<string> GetActivePortfolioSymbols()
        {
            var list = new List<string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string sql = "SELECT DISTINCT Symbol FROM [dbo].[Portfolio] WHERE Active = 1";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                                list.Add(reader.GetString(0).Trim().ToUpper());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Database Error while retrieving symbols");
                throw;
            }
            return list;
        }

        private static async Task<int> ProcessSymbolAsync(string rawSymbol)
        {
            Log.Information($"Processing Symbol: {rawSymbol}");
            string stooqSymbol = rawSymbol.EndsWith(".US") ? rawSymbol : rawSymbol + ".US";
            int insertedTotal = 0;

            try
            {
                var history = await DownloadStooqHistoryAsync(stooqSymbol);

                if (history.Count == 0)
                {
                    Log.Warning($"  -> No data found for {stooqSymbol}. Skipping.");
                    return 0;
                }

                var cutoff = new DateTime(2017, 1, 1);
                var filteredData = new List<PriceRecord>();
                foreach (var h in history)
                {
                    if (h.Date >= cutoff) filteredData.Add(h);
                }

                if (filteredData.Count > 0)
                {
                    insertedTotal = await InsertDataAsync(rawSymbol, filteredData);
                    if (insertedTotal > 0)
                        Log.Information($"  -> Success: {insertedTotal} new records inserted for {rawSymbol}.");
                    else
                        Log.Information($"  -> {rawSymbol} is up to date.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"  -> Error processing {rawSymbol}");
            }

            return insertedTotal;
        }

        private static async Task<int> InsertDataAsync(string symbol, List<PriceRecord> records)
        {
            int count = 0;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var rec in records)
                        {
                            string sql = @"
                                IF NOT EXISTS (SELECT 1 FROM [dbo].[ClosingPrices] WHERE Symbol = @Symbol AND Closing = @Closing)
                                BEGIN
                                    INSERT INTO [dbo].[ClosingPrices] ([Closing], [Symbol], [Price], [LastUpdate]) 
                                    VALUES (@Closing, @Symbol, @Price, @LastUpdate)
                                END";

                            using (SqlCommand cmd = new SqlCommand(sql, conn, trans))
                            {
                                cmd.Parameters.Add("@Closing", SqlDbType.DateTime).Value = rec.Date;
                                cmd.Parameters.Add("@Symbol", SqlDbType.NVarChar, 10).Value = symbol;
                                cmd.Parameters.Add("@Price", SqlDbType.Money).Value = rec.Close;
                                cmd.Parameters.Add("@LastUpdate", SqlDbType.DateTime).Value = DateTime.Now;

                                int rows = await cmd.ExecuteNonQueryAsync();
                                if (rows > 0) count++;
                            }
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            return count;
        }

        private static async Task<List<PriceRecord>> DownloadStooqHistoryAsync(string symbol)
        {
            string url = $"https://stooq.com/q/d/l/?s={symbol}&i=d";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string csv = await response.Content.ReadAsStringAsync();
            if (csv.Trim().StartsWith("<")) throw new Exception("Invalid Ticker");

            var list = new List<PriceRecord>();
            var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < lines.Length; i++)
            {
                var cols = lines[i].Split(',');
                if (cols.Length >= 5 &&
                    DateTime.TryParse(cols[0], out DateTime d) &&
                    decimal.TryParse(cols[4], out decimal c))
                {
                    list.Add(new PriceRecord { Date = d, Close = c });
                }
            }
            return list;
        }
    }

    public class PriceRecord
    {
        public DateTime Date { get; set; }
        public decimal Close { get; set; }
    }
}