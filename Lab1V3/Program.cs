using System;
using System.Collections.Generic;
using System.Numerics;
using static System.Math;

//..................ЭЛЕМЕНТ ДАННЫХ ИЗМЕРЕНИЯ (СОДЕРЖИТ КОРДНИАТЫ ПО ОСИ X И Y И ЗНАЧЕНИЕ ВЕКТОРА ПОЛЯ В ЗАДАННОЙ ТОЧКЕ)
public struct DataItem
{
    public double X { get; }
    public double Y { get; }
    public Vector2 VecVal { get; }

    public DataItem(double x, double y, Vector2 v)
    { X = x; Y = y; VecVal = v; }

    public string ToLongString(string format)
    { return "X=" + String.Format(format, X) + ";  Y=" + String.Format(format, Y) + "\n"; }

    public override string ToString() { return "\n"; }
}

//..................ДЕЛЕГАТ ВЕКТОРА ПОЛЯ
public delegate Vector2 FdblVector2(double x, double y);

//..................ЭЛЕМЕНТ ДАННЫХ (СОДЕРЖИТ НАЗВАНИЕ ТИПА И ДАТУ)
public abstract class V3Data
{
    public string ObjectId { get; }
    public DateTime Date { get; }

    public V3Data(string ObjectId, DateTime Date)
    { this.ObjectId = ObjectId; this.Date = Date; }

    public abstract int Count { get; }

    public abstract double MaxDistance { get; }

    public abstract string ToLongString(string format);

    public override string ToString()
    { return "ID: " + ObjectId + "\nDate: " + Convert.ToString(Date) + "\n"; }
}

//..................СПИСОК
public class V3DataList: V3Data
{
    public List<DataItem> DataList { get; }

    public V3DataList(string str, DateTime date): base(str, date)
    { DataList = new List<DataItem>(); }

    public bool Add(DataItem NewItem)
    {
        bool val = true;
        foreach(DataItem Item in DataList)
        {
            if(Item.X == NewItem.X && Item.Y == NewItem.Y)
                val = false;
        }
        if (val)
            DataList.Add(NewItem);
        return val;
    }

    public int AddDefaults(int nItems, FdblVector2 F)
    {
        int num = 0;
        Random rnd = new Random();
        int x, y;
        Vector2 result;
        bool Item_is_added;

        for(int i=0; i<nItems; i++)
        {
            x = rnd.Next(-50, 50);
            y = rnd.Next(-50, 50);
            result = F(x, y);
            DataItem Item = new DataItem(x, y, result);
            Item_is_added = Add(Item);
            if(Item_is_added)
                num++;
        }
        return num;
    }

    public override int Count
    { get { return DataList.Count; } }

    public override double MaxDistance
    {
        get
        {
            double Dis, MaxDis = 0;
            foreach(DataItem A in DataList)
                foreach(DataItem B in DataList)
                {
                    Dis = Sqrt(Pow((A.X-B.X), 2) + Pow((A.Y-B.Y), 2));
                    if(Dis > MaxDis)
                        MaxDis = Dis;
                }
            return MaxDis;
        }
    }

    public override string ToString()
    { return base.ToString() + "\nAmount of elements in the list: " + Count + "\n"; }

    public override string ToLongString(string format)
    {
        string info = null;
        int n = 1;
//        Vector2 abs;
        foreach(DataItem Item in DataList)
        {
            info += Convert.ToString(n) + ")  X=" + String.Format(format, Item.X) +"   Y=" + 
            String.Format(format, Item.Y) + "   Vector Value: " + String.Format(format, Item.VecVal) + "\n";
            n++;
        }
        return ToString() + "List Info:\n" + info;
    }
}

//..................ПРЯМОУГОЛЬНАЯ СЕТКА
public class V3DataArray: V3Data
{
    public int Xnum { get; }
    public int Ynum { get; }
    public double Xstep { get; }
    public double Ystep { get; }
    public Vector2[,] InfoVec { get; }
    
    public V3DataArray(string str, DateTime date): base(str, date)
    { Xnum = 0; Ynum = 0; Xstep = 0; Ystep = 0; InfoVec = new Vector2[0,0]; }

    public V3DataArray(string str, DateTime date, int xnum, int ynum, double xstep, double ystep, FdblVector2 Vec): base(str, date)
    {
        Xnum = xnum;
        Ynum = ynum;
        Xstep = xstep;
        Ystep = ystep;
        InfoVec = new Vector2[Xnum, Ynum];
        
        double Xval = 0;
        double Yval = 0;
        for(int i=0; i<Xnum; i++)
            for(int j=0; j<Ynum; j++)
            {
                InfoVec[i,j] = Vec(Xval, Yval);
                Xval += Xstep;
                Yval += Ystep;
            }
    }

    public override int Count { get { return InfoVec.Length; } }

    public override double MaxDistance
    { 
        get
        {
            double x1, y1, x2, y2, Dis, MaxDis = 0;
            for(int i1=0; i1<Xnum; i1++)
                for(int j1=0; j1<Ynum; j1++)
                {
                    x1 = i1*Xstep;
                    y1 = j1*Ystep;
                    for(int i2=0; i2<Xnum; i2++)
                        for(int j2=0; j2<Ynum; j2++)
                        {
                            x2 = i2*Xstep;
                            y2 = j2*Ystep;    
                            if(x1!=x2 | y1!=y2)
                            {
                                Dis = Sqrt(Pow((x2-x1), 2) + Pow((y2-y1), 2));
                                if(Dis > MaxDis)
                                    MaxDis = Dis;
                            }
                        }
                }
            return MaxDis;
        }
    }

    public override string ToString()
    { return base.ToString() + "\nAmount of nodes:\nOx: " + Convert.ToString(Xnum) + "     Oy: " + Convert.ToString(Ynum) + "\n"; }

    public override string ToLongString(string format)
    {
        string str = this.ToString() + "\nNodes info:\n";
        int n = 1;
        for(int i=0; i<Xnum; i++)
            for(int j=0; j<Ynum; j++)
            {
                str += Convert.ToString(n) + ".  X=" + String.Format(format, i*Xstep) + 
                "   Y=" + String.Format(format, i*Ystep) + "  Vector Value: " + String.Format(format, InfoVec[i, j]) + "\n";
                n++;
            }
        return str;
    }

    public static implicit operator V3DataList(V3DataArray DataArray)
    {
        V3DataList DataList = new V3DataList(DataArray.ObjectId, DataArray.Date);
        for(int i=0; i<DataArray.Xnum; i++)
            for(int j=0; j<DataArray.Ynum; j++)
            {
                DataItem Item = new DataItem(i*DataArray.Xstep, j*DataArray.Ystep, DataArray.InfoVec[i,j]);
                DataList.Add(Item);
            }
        return DataList;
    }
}

//..................КОЛЛЕКЦИЯ ЭЛЕМЕНТОВ ДАННЫХ 
public class V3MainCollection
{
    private List<V3Data> DataList;

    public V3MainCollection()
    { DataList = new List<V3Data>(); }

    public V3Data this[int index] { get { return DataList[index]; } }

    public int Count { get { return DataList.Count; } }

    public bool Contains(string ID)
    {
        bool contains = false;
        foreach(V3Data Data in DataList)
            if(Data.ObjectId == ID)
            { contains = true; return contains; }
        return contains;
    }

    public bool Add(V3Data NewItem)
    {
        bool contains = false;
        bool Item_is_added = false;
        
        foreach(V3Data Data in DataList)
            if(Data.ObjectId == NewItem.ObjectId)
                contains = true;
        
        if (contains == false)
        { DataList.Add(NewItem); Item_is_added = true; }
        return Item_is_added;
    }

    public string ToLongString(string format)
    {
        string str = "\n...............COLLECTION ITEMS:\n";
        foreach(V3Data Item in DataList)
            str += Item.ToLongString(format) + "\n";
        return str + "...............END OF COLLECTION.\n";
    }

    public override string ToString()
    {
        string str = null;
        foreach(V3Data Item in DataList)
            str += Item.ToString();
        return str;
    }  
}

//..................КЛАСС МЕТОДОВ ВЫЧИСЛЕНИЯ ВЕКТОРА ПОЛЯ (в=использую вместо вычислений простейшние операции)
public static class VecCalculator
{
    public static Vector2 Sum(double x, double y)
    { return new Vector2((float)(x+y)); }

    public static Vector2 Sub(double x, double y)
    { return new Vector2((float)(x-y)); }

    public static Vector2 Com(double x, double y)
    { return new Vector2((float)(x*y)); }
}

class Test
{
    static void Main()
    {
        FdblVector2 V = new FdblVector2(VecCalculator.Sum);
        DateTime date1 = new DateTime(2021, 10, 11, 14, 55, 59);
        DateTime date2 = new DateTime(2021, 10, 12, 12, 30, 31);
        
        V3DataArray Ar1 = new V3DataArray("Entry #1", date1, 5, 5, 3, 2, V);
        V3DataArray Ar2 = new V3DataArray("Entry #2", date2, 5, 5, 3, 2, V);
        V3DataList Lst1 = Ar1;
        V3DataList Lst2 = Ar2;

        string format = "{0:F4}";

//.....1)
        Console.WriteLine(Ar1.ToLongString(format));
        Console.WriteLine("List representation:");
        Console.WriteLine(Lst1.ToLongString(format));

//.....2)
        V3MainCollection Collection = new V3MainCollection();
        Collection.Add(Ar1); Collection.Add(Ar2);
        Collection.Add(Lst1); Collection.Add(Lst2);
        Console.WriteLine(Collection.ToLongString(format));

//.....3)
        for(int index=0; index<Collection.Count; index++)
        {
            Console.Write(index+1);
            Console.Write(")  Count=");
            Console.Write(Collection[index].Count);
            Console.Write("   MaxDistance=");
            Console.WriteLine(Collection[index].MaxDistance);
        }
    }
}
