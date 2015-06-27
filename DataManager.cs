using System;
using System.Text;
using System.Data.SqlClient;

public class DataManager
{
    SqlConnection connection;
    public DataManager()
	{
        connection = new SqlConnection();
	}

}
