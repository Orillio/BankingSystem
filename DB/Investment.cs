using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;


[Serializable]
[Microsoft.SqlServer.Server.SqlUserDefinedType(Format.Native)]
public struct Investment: INullable
{
    // Это поле элемента-заполнителя
    public int _var1;
 
    // Закрытый член
    private bool _null;

    public override string ToString() =>
         string.Empty;
    
    public bool IsNull
    {
        get => _null;
    }
    
    public static Investment Null
    {
        get
        {
            Investment h = new Investment();
            h._null = true;
            return h;
        }
    }
    
    public static Investment Parse(SqlString s)
    {
        if (s.IsNull)
            return Null;
        Investment u = new Investment();
        // Введите здесь код
        return u;
    }
    
    // Это метод-заполнитель
    public string Method1()
    {
        // Введите здесь код
        return string.Empty;
    }
    
    // Это статический метод заполнителя
    public static SqlString Method2()
    {
        // Введите здесь код
        return new SqlString("");
    }
    
}