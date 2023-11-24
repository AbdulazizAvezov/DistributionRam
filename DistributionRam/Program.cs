using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        try
        {
            Console.WriteLine("Введите размер раздела в ГБ");
            int partitionSize = Convert.ToInt16(Console.ReadLine());
            // Call to function which will create partition.
            CreatePartition(partitionSize);
            Console.WriteLine("Форматирование завершено!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        Console.ReadKey();

    }
    private static int CreatePartition(int partitionSizeInGB)
    {
        string sd = string.Empty;
        int result = 0;
        // Получите все приводы, кроме приводов компакт-дисков и сетевых дисков.
        List<DriveInfo> allDrivesInfo = DriveInfo.GetDrives().Where(x => x.DriveType != DriveType.Network).OrderBy(c => c.Name).ToList();
        // Получите новое имя диска на основе существующих дисков.
        char newDriveName = allDrivesInfo.LastOrDefault()?.Name.FirstOrDefault() ?? 'A';
        newDriveName = (char)(Convert.ToUInt16(newDriveName) + 1);
        //Получите информацию о фиксированных дисках.
        List<DriveInfo> allFixedDrives = DriveInfo.GetDrives().Where(c => c.DriveType == DriveType.Fixed).ToList();

        try
        {
            string scriptFilePath = System.IO.Path.GetTempPath() + @"\dpScript.txt";
            string driveName = allFixedDrives.FirstOrDefault()?.Name;
            if (File.Exists(scriptFilePath))
            {
                File.Delete(scriptFilePath); // Удалите скрипт, если он существует.
            }
            // Создайте сценарий для изменения размера и форматирования раздела.
            File.AppendAllText(scriptFilePath,
                string.Format(
                    "SELECT DISK=0\n" +       // Выберите первый диск.
                    "SELECT VOLUME={0}\n" +   // Выберите диск.
                    "SHRINK DESIRED={1} MINIMUM={1}\n" + // Уменьшите изображение до половины исходного размера.
                    "CREATE PARTITION PRIMARY\n" +       // Создайте раздел диска.
                    "ASSIGN LETTER={2}\n" +             // Назначьте его букву.
                    "FORMAT FS=FAT32 QUICK\n" +         // Отформатируйте его.
                    "EXIT",
                    driveName, partitionSizeInGB * 1000, newDriveName)); // Выход.
            int exitCode = 0;
            string resultStr = ExecuteCmdCommand("DiskPart.exe" + " /s " + scriptFilePath, ref exitCode);
            File.Delete(scriptFilePath); // Удалите файл скрипт.
            if (exitCode > 0)
            {
                result = exitCode;
            }
            return result;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    private static string ExecuteCmdCommand(string command, ref int exitCode)
    {
        ProcessStartInfo processInfo;
        Process process = new Process();
        string output = string.Empty;
        processInfo = new ProcessStartInfo("cmd.exe", "/C " + command);
        processInfo.CreateNoWindow = false;
        processInfo.WindowStyle = ProcessWindowStyle.Normal;
        processInfo.UseShellExecute = false;
        processInfo.RedirectStandardOutput = true;
        process.StartInfo = processInfo;
        process = Process.Start(processInfo);
        StreamReader streamReader = process.StandardOutput;
        output = streamReader.ReadToEnd();
        exitCode = process.ExitCode;
        process.Close();
        return output;
    }
}
