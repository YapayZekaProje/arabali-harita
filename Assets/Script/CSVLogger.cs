using System;
using System.IO;
using System.Text;
using System.Globalization;

public static class CsvLogger
{
    private static string filePath = "SimulationData.csv";

    static CsvLogger()
    {
        //(Dosya yoksa yeni oluþtur)
        if (!File.Exists(filePath))
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                string header = "Tarih/Saat,Olay Turu,Olay Durumu,Olaya Mesafe (m),Hiz (Km/h),Arac Durumu,Algoritma Adı,Düğüm Sayısı,Maliyet, Gerçek Mesafe (m)";
                writer.WriteLine(header);
            }
        }
    }

    public static void Log(
    string eventType,
    string eventStatus,
    float distance = 0,
    float speed = 0,
    string vehicleStatus = "N/A",
    string algorithm = "N/A",
    int nodesVisited = 0,
    int cost = 0,
    float realDistanceToTarget = 0) // Gerçek mesafe parametresi ekledik)
{
    string timestamp = DateTime.Now.ToString("HH:mm:ss");
    string data = $"{timestamp},{eventType},{eventStatus},{distance.ToString("F2", CultureInfo.InvariantCulture)},{speed.ToString("F1", CultureInfo.InvariantCulture)},{vehicleStatus},{algorithm},{nodesVisited},{cost},{realDistanceToTarget.ToString("F2", CultureInfo.InvariantCulture)}";

    using (var writer = new StreamWriter(filePath, true, Encoding.UTF8))
    {
        writer.WriteLine(data);
    }
}
}
