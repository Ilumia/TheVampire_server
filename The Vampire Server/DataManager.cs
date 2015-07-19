using System;
using System.Data.SqlClient;
using System.Data;

using MySql.Data.MySqlClient;

namespace The_Vampire_Server
{
    public class DataManager
    {
        private DataManager() { }
        private static DataManager dataManager = new DataManager();
        public static DataManager GetDataManager() {
            return dataManager;
        }
        static string connectionString = "Server=localhost; " +
            "Database=vampdb; " +
            "Uid=ilumia; " +
            "Pwd=vjpp56; " +
            "Charset=utf8;";
        /*  MS SQL Server
        static string connectionString = "Server=MOONBACKSAN-PC\\SQLEXPRESS;" +
            "Database=vampdb;" +
            "User Id=ilumia;" +
            "Password=vjpp56;";
        */

        // 결과 테이블을 반환
        public DataTable ExecuteQuery(string _sql)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = null;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MySqlDataAdapter dataAdapter = new MySqlDataAdapter(_sql, connection);
                    dataAdapter.Fill(dataSet);
                    dataTable = dataSet.Tables[0];
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                finally
                {
                    connection.Close();
                }
            }
            return dataTable;
        }
        /* Ms SQL Server ExecuteQuery()
        public DataTable ExecuteQuery(string _sql)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(_sql, connection);
                    dataAdapter.Fill(dataSet);
                    dataTable = dataSet.Tables[0];
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                finally
                {
                    connection.Close();
                }
            }
            return dataTable;
        }
        */

        // 처리된 행의 갯수를 반환 (-1: error)
        public int ExecuteUpdate(string _sql)
        {
            int result = -1;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(_sql, connection);
                    result = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
            return result;
        }
        /* Ms SQL Server ExecuteUpdate()
        public int ExecuteUpdate(string _sql)
        {
            int result = -1;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(_sql, connection);
                    result = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
            return result;
        }
        */
    }
}