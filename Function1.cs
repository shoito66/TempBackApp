using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;//DB�ڑ��p���C�u����
using Newtonsoft.Json;
using System.Net.Mail;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Collections.Generic;

namespace FunctionAPIApp
{
    public static class Function1
    {
        //�֐������uSELECT�v��
        [FunctionName("SELECT")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            //���X�|���X�p������
            string responseMessage;
            // �d�b�ԍ����擾
            string phoneNumber = req.Query["PhoneNumber"];


            try
            {
                //�ڑ�������̐ݒ�
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "m3h-ito-sqldb0428.database.windows.net";
                builder.UserID = "sqladmin";
                builder.Password = "Abcd1234";
                builder.InitialCatalog = "m3h-ito-sqldb0428";

                //�ڑ��p�I�u�W�F�N�g�̏�����
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    Console.WriteLine("\nQuery data example:");
                    Console.WriteLine("=========================================\n");

                    //���s����N�G��
                    String sql = "SELECT ID,Name,PhoneNumber," +
                        "MailAddress,Number_of_Tickets,SeatType FROM Reservations";
                    // �d�b�ԍ����w�肳��Ă���ꍇ��WHERE���ǉ�
                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        sql += " WHERE PhoneNumber = @PhoneNumber";
                    }


                    //SQL���s�I�u�W�F�N�g�̏�����
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        // �d�b�ԍ����w�肳��Ă���ꍇ�̓p�����[�^��ǉ�
                        if (!string.IsNullOrEmpty(phoneNumber))
                        {
                            command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        }

                    
                        //DB�Ɛڑ�
                        connection.Open();

                        //SQL�����s���A���ʂ��I�u�W�F�N�g�Ɋi�[
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            //���ʂ��i�[���邽�߂̃I�u�W�F�N�g��������
                            ReservationsList resultList = new ReservationsList();

                            //���ʂ�1�s������
                            while (reader.Read())
                            {
                                //�I�u�W�F�N�g�Ɍ��ʂ��i�[
                                resultList.List.Add(new ReservationsRow {
                                    ID = reader.GetInt32("id"), 
                                    Name = reader.GetString("Name"),
                                    PhoneNumber = reader.GetString("PhoneNumber"),
                                    MailAddress= reader.GetString("MailAddress"),
                                    Number_of_Tickets = reader.GetInt32("Number_of_Tickets"),
                                    SeatType = reader.GetString("SeatType") });
                            }
                            //JSON�I�u�W�F�N�g�𕶎���ɕϊ�
                            responseMessage = JsonConvert.SerializeObject(resultList);
                        
                        }
                    }
                }
            }
            //DB����ŃG���[�����������ꍇ�͂����ŃL���b�`
            catch (SqlException e)
            {
                //�G���[���R���\�[���ɏo��
                Console.WriteLine(e.ToString());
                responseMessage = "Error accessing database.";

            }
            //���ʕ������ԋp
            return new OkObjectResult(responseMessage);
        }
        [FunctionName("INSERT")]
        public static async Task<IActionResult> RunInsert(
[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            //HTTP���X�|���X�ŕԂ���������`
            string responseMessage = "INSERT RESULT:";

            //�C���T�[�g�p�̃p�����[�^�[�擾�iGET���\�b�h�p�j
            string Name = req.Query["Name"];
            string PhoneNumber = req.Query["PhoneNumber"];
            string MailAddress = req.Query["MailAddress"];
            string Number_of_Tickets = req.Query["Number_of_Tickets"];
            string SeatType = req.Query["SeatType"];




            //�C���T�[�g�p�̃p�����[�^�[�擾�iPOST���\�b�h�p�j
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            Name = Name ?? data?.Name;
            PhoneNumber = PhoneNumber ?? data?.PhoneNumber;
            MailAddress = MailAddress ?? data?.MailAddress;
            Number_of_Tickets = Number_of_Tickets ?? data?.Number_of_Tickets;
            SeatType = SeatType ?? data?.SeatType;

            //���p�����[�^�[���擾�ł����ꍇ�̂ݏ���
            if ( 
                !string.IsNullOrWhiteSpace(Name) && 
                !string.IsNullOrWhiteSpace(PhoneNumber)&&
                !string.IsNullOrWhiteSpace(MailAddress) &&
                !string.IsNullOrWhiteSpace(Number_of_Tickets)&&
                !string.IsNullOrWhiteSpace(SeatType))
            {
                try
                {
                    //DB�ڑ��ݒ�i�ڑ�������̍\�z�j
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                    builder.DataSource = "m3h-ito-sqldb0428.database.windows.net";
                    builder.UserID = "sqladmin";
                    builder.Password = "Abcd1234";
                    builder.InitialCatalog = "m3h-ito-sqldb0428";

                    //SQL�R�l�N�V������������
                    using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                    {

                        //���s����SQL�i�p�����[�^�[�t���j
                        String sql = "INSERT INTO Reservations( Name, PhoneNumber, MailAddress, Number_of_Tickets, SeatType) VALUES (@Name, @PhoneNumber, @MailAddress, @Number_of_Tickets, @SeatType)";
                        //SQL�R�}���h��������
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            //�p�����[�^�[��ݒ�
                            
                            command.Parameters.AddWithValue("@Name", Name);
                            command.Parameters.AddWithValue("@PhoneNumber", PhoneNumber);
                            command.Parameters.AddWithValue("@MailAddress", MailAddress);
                            command.Parameters.AddWithValue("@Number_of_Tickets", int.Parse(Number_of_Tickets));
                            command.Parameters.AddWithValue("@SeatType", SeatType);

                            //�R�l�N�V�����I�[�v���i���@SQLDatabase�ɐڑ��j
                            connection.Open();

                            //SQL�R�}���h�����s�����ʍs�����擾
                            int result = command.ExecuteNonQuery();

                            //���X�|���X�p��JSON�I�u�W�F�N�g�Ɋi�[
                            JObject jsonObj = new JObject { ["result"] = $"{result}�s�X�V����܂���" };

                            //JSON�I�u�W�F�N�g�𕶎���ɕϊ�
                            responseMessage = JsonConvert.SerializeObject(jsonObj, Formatting.None);

                        }
                    }
                }
                //DB�����ŃG���[�����������ꍇ
                catch (SqlException e)
                {
                    //�R���\�[���ɃG���[���o��
                    Console.WriteLine(e.ToString());
                }

            }
            else
            {
                responseMessage = "�p�����[�^�[���ݒ肳��Ă��܂���";
            }

            //HTTP���X�|���X��ԋp
            return new OkObjectResult(responseMessage);
        }

        // UPDATE �֐�
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

                            JObject jsonObj = new JObject { ["result"] = $"{result}�s�X�V����܂���" };
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
                responseMessage = "ID �܂��͍X�V����p�����[�^�[���ݒ肳��Ă��܂���";
            }

            return new OkObjectResult(responseMessage);
        }

        // DELETE �֐�
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

                            JObject jsonObj = new JObject { ["result"] = $"{result}�s�폜����܂���" };
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
                responseMessage = "�d�b�ԍ����ݒ肳��Ă��܂���";
            }

            return new OkObjectResult(responseMessage);
        }
    }

    
    }



   
