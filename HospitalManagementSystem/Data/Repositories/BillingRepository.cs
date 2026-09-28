using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalManagementSystem.Models;
using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data.Repositories
{
    public class BillingRepository : AdoNetRepositoryBase
    {
        public BillingRepository(DbConnectionHelper dbConnectionHelper)
            : base(dbConnectionHelper)
        {
        }

        public async Task<int> AddAsync(Bill bill, IEnumerable<BillItem> items)
        {
            const string insertBillQuery = @"
INSERT INTO Bills
(PatientId, AppointmentId, DoctorId, BillDate, SubTotal, Tax, Discount, Total, PaymentStatus, PaymentMethod, Notes)
OUTPUT INSERTED.BillId
VALUES
(@PatientId, @AppointmentId, @DoctorId, @BillDate, @SubTotal, @Tax, @Discount, @Total, @PaymentStatus, @PaymentMethod, @Notes);";

            const string insertItemQuery = @"
INSERT INTO BillItems
(BillId, Description, Quantity, UnitPrice, Amount)
VALUES
(@BillId, @Description, @Quantity, @UnitPrice, @Amount);";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                await using SqlCommand billCmd =
                    new SqlCommand(insertBillQuery, connection, transaction);

                billCmd.Parameters.AddWithValue("@PatientId", bill.PatientId);
                billCmd.Parameters.AddWithValue(
                    "@AppointmentId",
                    (object?)bill.AppointmentId ?? DBNull.Value);
                billCmd.Parameters.AddWithValue(
                    "@DoctorId",
                    (object?)bill.DoctorId ?? DBNull.Value);
                billCmd.Parameters.AddWithValue(
                    "@BillDate",
                    bill.BillDate == default ? DateTime.UtcNow : bill.BillDate);
                billCmd.Parameters.AddWithValue("@SubTotal", bill.SubTotal);
                billCmd.Parameters.AddWithValue("@Tax", bill.Tax);
                billCmd.Parameters.AddWithValue("@Discount", bill.Discount);
                billCmd.Parameters.AddWithValue("@Total", bill.Total);
                billCmd.Parameters.AddWithValue(
                    "@PaymentStatus",
                    (object?)bill.PaymentStatus ?? "Unpaid");
                billCmd.Parameters.AddWithValue(
                    "@PaymentMethod",
                    DbValue(bill.PaymentMethod));
                billCmd.Parameters.AddWithValue(
                    "@Notes",
                    DbValue(bill.Notes));

                object? result = await billCmd.ExecuteScalarAsync();
                int billId = Convert.ToInt32(result);

                foreach (BillItem item in items)
                {
                    decimal amount = item.Amount;

                    if (amount <= 0)
                    {
                        amount = item.UnitPrice * item.Quantity;
                    }

                    await using SqlCommand itemCmd =
                        new SqlCommand(insertItemQuery, connection, transaction);

                    itemCmd.Parameters.AddWithValue("@BillId", billId);
                    itemCmd.Parameters.AddWithValue(
                        "@Description",
                        item.Description.Trim());
                    itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                    itemCmd.Parameters.AddWithValue("@Amount", amount);

                    await itemCmd.ExecuteNonQueryAsync();
                }

                transaction.Commit();
                return billId;
            }
            catch
            {
                try
                {
                    transaction.Rollback();
                }
                catch
                {
                }

                throw;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public async Task<Bill?> GetByIdAsync(int billId)
        {
            const string billQuery = @"
SELECT 
    b.BillId,
    b.PatientId,
    b.AppointmentId,
    b.DoctorId,
    b.BillDate,
    b.SubTotal,
    b.Tax,
    b.Discount,
    b.Total,
    b.PaymentStatus,
    b.PaymentMethod,
    b.Notes,
    b.CreatedAt,
    CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
    d.FullName AS DoctorName
FROM Bills b
LEFT JOIN Patients p ON b.PatientId = p.PatientId
LEFT JOIN Doctors d ON b.DoctorId = d.DoctorId
WHERE b.BillId = @BillId;";

            const string itemsQuery = @"
SELECT 
    BillItemId,
    BillId,
    Description,
    Quantity,
    UnitPrice,
    Amount,
    CreatedAt
FROM BillItems
WHERE BillId = @BillId
ORDER BY BillItemId ASC;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand billCmd =
                new SqlCommand(billQuery, connection);

            billCmd.Parameters.AddWithValue("@BillId", billId);

            await using SqlDataReader reader =
                await billCmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            Bill bill = new Bill
            {
                BillId = GetInt32(reader, "BillId"),
                PatientId = GetInt32(reader, "PatientId"),

                AppointmentId =
                    reader.IsDBNull(reader.GetOrdinal("AppointmentId"))
                        ? null
                        : (int?)reader.GetInt32(
                            reader.GetOrdinal("AppointmentId")),

                DoctorId =
                    reader.IsDBNull(reader.GetOrdinal("DoctorId"))
                        ? null
                        : (int?)reader.GetInt32(
                            reader.GetOrdinal("DoctorId")),

                BillDate = GetDateTime(reader, "BillDate"),
                SubTotal = GetDecimal(reader, "SubTotal"),
                Tax = GetDecimal(reader, "Tax"),
                Discount = GetDecimal(reader, "Discount"),
                Total = GetDecimal(reader, "Total"),

                PaymentStatus =
                    GetNullableString(reader, "PaymentStatus")
                    ?? "Unpaid",

                PaymentMethod =
                    GetNullableString(reader, "PaymentMethod"),

                Notes =
                    GetNullableString(reader, "Notes"),

                CreatedAt =
                    GetDateTime(reader, "CreatedAt"),

                PatientName =
                    GetNullableString(reader, "PatientName"),

                DoctorName =
                    GetNullableString(reader, "DoctorName")
            };

            await reader.CloseAsync();

            await using SqlCommand itemsCmd =
                new SqlCommand(itemsQuery, connection);

            itemsCmd.Parameters.AddWithValue("@BillId", billId);

            await using SqlDataReader itemsReader =
                await itemsCmd.ExecuteReaderAsync();

            List<BillItem> items = new List<BillItem>();

            while (await itemsReader.ReadAsync())
            {
                items.Add(new BillItem
                {
                    BillItemId =
                        GetInt32(itemsReader, "BillItemId"),

                    BillId =
                        GetInt32(itemsReader, "BillId"),

                    Description =
                        GetNullableString(
                            itemsReader,
                            "Description")
                        ?? string.Empty,

                    Quantity =
                        GetInt32(itemsReader, "Quantity"),

                    UnitPrice =
                        GetDecimal(itemsReader, "UnitPrice"),

                    Amount =
                        GetDecimal(itemsReader, "Amount"),

                    CreatedAt =
                        GetDateTime(itemsReader, "CreatedAt")
                });
            }

            bill.Items = items;

            return bill;
        }

        public async Task<IReadOnlyList<Bill>> GetAllAsync()
        {
            const string query = @"
SELECT 
    BillId,
    PatientId,
    AppointmentId,
    DoctorId,
    BillDate,
    SubTotal,
    Tax,
    Discount,
    Total,
    PaymentStatus,
    PaymentMethod,
    Notes,
    CreatedAt
FROM Bills
ORDER BY BillDate DESC;";

            List<Bill> bills = new List<Bill>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand(query, connection);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                bills.Add(MapBillHeader(reader));
            }

            return bills;
        }

        public async Task<IReadOnlyList<Bill>> GetByPatientAsync(
            int patientId)
        {
            const string query = @"
SELECT 
    BillId,
    PatientId,
    AppointmentId,
    DoctorId,
    BillDate,
    SubTotal,
    Tax,
    Discount,
    Total,
    PaymentStatus,
    PaymentMethod,
    Notes,
    CreatedAt
FROM Bills
WHERE PatientId = @PatientId
ORDER BY BillDate DESC;";

            List<Bill> bills = new List<Bill>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@PatientId", patientId);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                bills.Add(MapBillHeader(reader));
            }

            return bills;
        }

        public async Task<IReadOnlyList<Bill>> GetByDateRangeAsync(
            DateTime from,
            DateTime to)
        {
            const string query = @"
SELECT 
    BillId,
    PatientId,
    AppointmentId,
    DoctorId,
    BillDate,
    SubTotal,
    Tax,
    Discount,
    Total,
    PaymentStatus,
    PaymentMethod,
    Notes,
    CreatedAt
FROM Bills
WHERE CAST(BillDate AS date)
      BETWEEN CAST(@FromDate AS date)
      AND CAST(@ToDate AS date)
ORDER BY BillDate DESC;";

            List<Bill> bills = new List<Bill>();

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@FromDate", from);
            command.Parameters.AddWithValue("@ToDate", to);

            await using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                bills.Add(MapBillHeader(reader));
            }

            return bills;
        }

        public async Task<bool> UpdatePaymentStatusAsync(
            int billId,
            string paymentStatus,
            string? paymentMethod = null)
        {
            const string query = @"
UPDATE Bills
SET 
    PaymentStatus = @PaymentStatus,
    PaymentMethod = @PaymentMethod
WHERE BillId = @BillId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@BillId", billId);

            command.Parameters.AddWithValue(
                "@PaymentStatus",
                paymentStatus.Trim());

            command.Parameters.AddWithValue(
                "@PaymentMethod",
                DbValue(paymentMethod));

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int billId)
        {
            const string deleteItemsQuery =
                @"DELETE FROM BillItems WHERE BillId = @BillId;";

            const string deleteBillQuery =
                @"DELETE FROM Bills WHERE BillId = @BillId;";

            await using SqlConnection connection = CreateConnection();
            await connection.OpenAsync();

            using SqlTransaction transaction =
                connection.BeginTransaction();

            try
            {
                await using SqlCommand itemsCmd =
                    new SqlCommand(
                        deleteItemsQuery,
                        connection,
                        transaction);

                itemsCmd.Parameters.AddWithValue("@BillId", billId);

                await itemsCmd.ExecuteNonQueryAsync();

                await using SqlCommand billCmd =
                    new SqlCommand(
                        deleteBillQuery,
                        connection,
                        transaction);

                billCmd.Parameters.AddWithValue("@BillId", billId);

                int affected =
                    await billCmd.ExecuteNonQueryAsync();

                transaction.Commit();

                return affected > 0;
            }
            catch
            {
                try
                {
                    transaction.Rollback();
                }
                catch
                {
                }

                throw;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        private static Bill MapBillHeader(SqlDataReader reader)
        {
            return new Bill
            {
                BillId =
                    GetInt32(reader, "BillId"),

                PatientId =
                    GetInt32(reader, "PatientId"),

                AppointmentId =
                    reader.IsDBNull(
                        reader.GetOrdinal("AppointmentId"))
                        ? null
                        : (int?)reader.GetInt32(
                            reader.GetOrdinal("AppointmentId")),

                DoctorId =
                    reader.IsDBNull(
                        reader.GetOrdinal("DoctorId"))
                        ? null
                        : (int?)reader.GetInt32(
                            reader.GetOrdinal("DoctorId")),

                BillDate =
                    GetDateTime(reader, "BillDate"),

                SubTotal =
                    GetDecimal(reader, "SubTotal"),

                Tax =
                    GetDecimal(reader, "Tax"),

                Discount =
                    GetDecimal(reader, "Discount"),

                Total =
                    GetDecimal(reader, "Total"),

                PaymentStatus =
                    GetNullableString(
                        reader,
                        "PaymentStatus")
                    ?? "Unpaid",

                PaymentMethod =
                    GetNullableString(
                        reader,
                        "PaymentMethod"),

                Notes =
                    GetNullableString(
                        reader,
                        "Notes"),

                CreatedAt =
                    GetDateTime(reader, "CreatedAt")
            };
        }
    }
}