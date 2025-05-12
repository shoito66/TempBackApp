using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;//DB接続用ライブラリ
using Newtonsoft.Json;
using System.Net.Mail;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Collections.Generic;

namespace FunctionAPIApp
{
    public static class Function1
    {
        //関数名を「SELECT」に
        [FunctionName("SELECT")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            //レスポンス用文字列
            string responseMessage;
            // 電話番号を取得
            string phoneNumber = req.Query["PhoneNumber"];


            try
            {
                //接続文字列の設定
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "m3h-ito-sqldb0428.database.windows.net";
                builder.UserID = "sqladmin";
                builder.Password = "Abcd1234";
                builder.InitialCatalog = "m3h-ito-sqldb0428";

                //接続用オブジェクトの初期化
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    Console.WriteLine("\nQuery data example:");
                    Console.WriteLine("=========================================\n");

                    //実行するクエリ
                    String sql = "SELECT ID,Name,PhoneNumber," +
                        "MailAddress,Number_of_Tickets,SeatType FROM Reservations";
                    // 電話番号が指定されている場合はWHERE句を追加
                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        sql += " WHERE PhoneNumber = @PhoneNumber";
                    }


                    //SQL実行オブジェクトの初期化
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        // 電話番号が指定されている場合はパラメータを追加
                        if (!string.IsNullOrEmpty(phoneNumber))
                        {
                            command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        }

                    
                        //DBと接続
                        connection.Open();

                        //SQLを実行し、結果をオブジェクトに格納
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            //結果を格納するためのオブジェクトを初期化
                            ReservationsList resultList = new ReservationsList();

                            //結果を1行ずつ処理
                            while (reader.Read())
                            {
                                //オブジェクトに結果を格納
                                resultList.List.Add(new ReservationsRow {
                                    ID = reader.GetInt32("id"), 
                                    Name = reader.GetString("Name"),
                                    PhoneNumber = reader.GetString("PhoneNumber"),
                                    MailAddress= reader.GetString("MailAddress"),
                                    Number_of_Tickets = reader.GetInt32("Number_of_Tickets"),
                                    SeatType = reader.GetString("SeatType") });
                            }
                            //JSONオブジェクトを文字列に変換
                            responseMessage = JsonConvert.SerializeObject(resultList);
                        
                        }
                    }
                }
            }
            //DB操作でエラーが発生した場合はここでキャッチ
            catch (SqlException e)
            {
                //エラーをコンソールに出力
                Console.WriteLine(e.ToString());
                responseMessage = "Error accessing database.";

            }
            //結果文字列を返却
            return new OkObjectResult(responseMessage);
        }
        [FunctionName("INSERT")]
        public static async Task<IActionResult> RunInsert(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            //HTTPレスポンスで返す文字列を定義
            string responseMessage = "INSERT RESULT:";

            //インサート用のパラメーター取得（GETメソッド用）
            string Name = req.Query["Name"];
            string PhoneNumber = req.Query["PhoneNumber"];
            string MailAddress = req.Query["MailAddress"];
            string Number_of_Tickets = req.Query["Number_of_Tickets"];
            string SeatType = req.Query["SeatType"];




            //インサート用のパラメーター取得（POSTメソッド用）
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            Name = Name ?? data?.Name;
            PhoneNumber = PhoneNumber ?? data?.PhoneNumber;
            MailAddress = MailAddress ?? data?.MailAddress;
            Number_of_Tickets = Number_of_Tickets ?? data?.Number_of_Tickets;
            SeatType = SeatType ?? data?.SeatType;

            //両パラメーターを取得できた場合のみ処理
            if ( 
                !string.IsNullOrWhiteSpace(Name) && 
                !string.IsNullOrWhiteSpace(PhoneNumber)&&
                !string.IsNullOrWhiteSpace(MailAddress) &&
                !string.IsNullOrWhiteSpace(Number_of_Tickets)&&
                !string.IsNullOrWhiteSpace(SeatType))
            {
                try
                {
                    //DB接続設定（接続文字列の構築）
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                    builder.DataSource = "m3h-ito-sqldb0428.database.windows.net";
                    builder.UserID = "sqladmin";
                    builder.Password = "Abcd1234";
                    builder.InitialCatalog = "m3h-ito-sqldb0428";

                    //SQLコネクションを初期化
                    using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                    {

                        //実行するSQL（パラメーター付き）
                        String sql = "INSERT INTO Reservations( Name, PhoneNumber, MailAddress, Number_of_Tickets, SeatType) VALUES (@Name, @PhoneNumber, @MailAddress, @Number_of_Tickets, @SeatType)";
                        //SQLコマンドを初期化
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            //パラメーターを設定
                            
                            command.Parameters.AddWithValue("@Name", Name);
                            command.Parameters.AddWithValue("@PhoneNumber", PhoneNumber);
                            command.Parameters.AddWithValue("@MailAddress", MailAddress);
                            command.Parameters.AddWithValue("@Number_of_Tickets", int.Parse(Number_of_Tickets));
                            command.Parameters.AddWithValue("@SeatType", SeatType);

                            //コネクションオープン（＝　SQLDatabaseに接続）
                            connection.Open();

                            //SQLコマンドを実行し結果行数を取得
                            int result = command.ExecuteNonQuery();

                            //レスポンス用にJSONオブジェクトに格納
                            JObject jsonObj = new JObject { ["result"] = $"{result}行更新されました" };

                            //JSONオブジェクトを文字列に変換
                            responseMessage = JsonConvert.SerializeObject(jsonObj, Formatting.None);

                        }
                    }
                }
                //DB処理でエラーが発生した場合
                catch (SqlException e)
                {
                    //コンソールにエラーを出力
                    Console.WriteLine(e.ToString());
                }

            }
            else
            {
                responseMessage = "パラメーターが設定されていません";
            }

            //HTTPレスポンスを返却
            return new OkObjectResult(responseMessage);
        }

        // UPDATE 関数
        [FunctionName("UPDATE")]
        public static async Task<IActionResult> RunUpdate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string responseMessage = "UPDATE RESULT:";

            string id = req.Query["ID"];
            string Name = req.Query["Name"];
            string PhoneNumber = req.Query["PhoneNumber"];
            string MailAddress = req.Query["MailAddress"];
            string Number_of_Tickets = req.Query["Number_of_Tickets"];
            string SeatType = req.Query["SeatType"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            id = id ?? data?.ID;
            Name = Name ?? data?.Name;
            PhoneNumber = PhoneNumber ?? data?.PhoneNumber;
            MailAddress = MailAddress ?? data?.MailAddress;
            Number_of_Tickets = Number_of_Tickets ?? data?.Number_of_Tickets;
            SeatType = SeatType ?? data?.SeatType;

            if (!string.IsNullOrWhiteSpace(id) &&
                (!string.IsNullOrWhiteSpace(Name) ||
                 !string.IsNullOrWhiteSpace(PhoneNumber) ||
                 !string.IsNullOrWhiteSpace(MailAddress) ||
                 !string.IsNullOrWhiteSpace(Number_of_Tickets) ||
                 !string.IsNullOrWhiteSpace(SeatType)))
            {
                try
                {
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                    {
                        DataSource = "m3h-ito-sqldb0428.database.windows.net",
                        UserID = "sqladmin",
                        Password = "Abcd1234",
                        InitialCatalog = "m3h-ito-sqldb0428"
                    };

                    using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                    {
                        string sql = @"
                            UPDATE Reservations
                            SET Name = COALESCE(@Name, Name),
                                PhoneNumber = COALESCE(@PhoneNumber, PhoneNumber),
                                MailAddress = COALESCE(@MailAddress, MailAddress),
                                Number_of_Tickets = COALESCE(@Number_of_Tickets, Number_of_Tickets),
                                SeatType = COALESCE(@SeatType, SeatType)
                            WHERE ID = @ID";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@ID", int.Parse(id));
                            command.Parameters.AddWithValue("@Name", (object)Name ?? DBNull.Value);
                            command.Parameters.AddWithValue("@PhoneNumber", (object)PhoneNumber ?? DBNull.Value);
                            command.Parameters.AddWithValue("@MailAddress", (object)MailAddress ?? DBNull.Value);
                            command.Parameters.AddWithValue("@Number_of_Tickets", string.IsNullOrWhiteSpace(Number_of_Tickets) ? (object)DBNull.Value : int.Parse(Number_of_Tickets));
                            command.Parameters.AddWithValue("@SeatType", (object)SeatType ?? DBNull.Value );

                            connection.Open();

                            int result = command.ExecuteNonQuery();

                            JObject jsonObj = new JObject { ["result"] = $"{result}行更新されました" };
                            responseMessage = JsonConvert.SerializeObject(jsonObj, Formatting.None);
                        }
                    }
                }
                catch (SqlException e)
                {
                    log.LogError(e.ToString());
                }
            }
            else
            {
                responseMessage = "ID または更新するパラメーターが設定されていません";
            }

            return new OkObjectResult(responseMessage);
        }

        // DELETE 関数
        [FunctionName("DELETE")]
        public static async Task<IActionResult> RunDelete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string responseMessage = "DELETE RESULT:";

            string phoneNumber = req.Query["phoneNumber"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            phoneNumber = phoneNumber ?? data?.PhoneNumber;


            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                try
                {
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                    {
                        DataSource = "m3h-ito-sqldb0428.database.windows.net",
                        UserID = "sqladmin",
                        Password = "Abcd1234",
                        InitialCatalog = "m3h-ito-sqldb0428"
                    };

                    using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                    {
                        string sql = "DELETE FROM Reservations WHERE PhoneNumber = @PhoneNumber";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                            connection.Open();

                            int result = command.ExecuteNonQuery();

                            JObject jsonObj = new JObject { ["result"] = $"{result}行削除されました" };
                            responseMessage = JsonConvert.SerializeObject(jsonObj, Formatting.None);
                        }
                    }
                }
                catch (SqlException e)
                {
                    log.LogError(e.ToString());
                }
            }
            else
            {
                responseMessage = "電話番号が設定されていません";
            }

            return new OkObjectResult(responseMessage);
        }
    }

    
    }



   
