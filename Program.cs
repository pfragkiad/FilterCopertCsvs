

using System.Diagnostics;
using System.Text;
using CsvReaderAdvanced;
using CsvReaderAdvanced.Files;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FilterCopertCsvs;

public class Program
{
    public static void Main(string[] args)
    {

        var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddCsvReader(hostContext.Configuration);
        })
        .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();


        List<string> vehicles = [
        "Rigid truck 3.5-7.5 t GVW",
        "Rigid truck 7.5-12 t GVW",
        "Rigid truck 12-20 t GVW",
        "Rigid truck 20-26 t GVW",
        "Rigid truck 26-32 t GVW",
        "Artic truck up to 34 t GVW",
        "Artic truck up to 40 t GVW",
        "Artic truck up to 44 t GVW",
        "Artic truck up to 60 t GVW",
        "Artic truck up to 72 t GVW",
        ];

        List<string> segments = [
        "Rigid <=7,5 t",
        "Rigid 7,5 - 12 t",
        "Rigid 12 - 14 t",
        "Rigid 14 - 20 t",
        "Rigid 20 - 26 t",
        "Rigid 26 - 28 t",
        "Rigid 28 - 32 t",
        "Rigid >32 t",
        "Articulated 14 - 20 t",
        "Articulated 20 - 28 t",
        "Articulated 28 - 34 t",
        "Articulated 34 - 40 t",
        "Articulated 40 - 50 t",
        "Articulated 50 - 60 t",
        ];


        Dictionary<string, List<string>> vehicleSegments = new()
        {
            ["Rigid truck 3.5-7.5 t GVW"] = ["Rigid <=7,5 t"],
            ["Rigid truck 7.5-12 t GVW"] = ["Rigid 7,5 - 12 t"],
            ["Rigid truck 12-20 t GVW"] = ["Rigid 12 - 14 t", "Rigid 14 - 20 t"],
            ["Rigid truck 20-26 t GVW"] = ["Rigid 20 - 26 t"],
            ["Rigid truck 26-32 t GVW"] = ["Rigid 26 - 28 t", "Rigid 28 - 32 t", "Rigid >32 t"],
            ["Artic truck up to 34 t GVW"] = ["Articulated 14 - 20 t", "Articulated 20 - 28 t", "Articulated 28 - 34 t"],
            ["Artic truck up to 40 t GVW"] = ["Articulated 34 - 40 t"],
            ["Artic truck up to 44 t GVW"] = ["Articulated 40 - 50 t"],
            ["Artic truck up to 60 t GVW"] = ["Articulated 40 - 50 t", "Articulated 50 - 60 t"],
            ["Artic truck up to 72 t GVW"] = ["Articulated 50 - 60 t"],
        };
        string ecPath = @"data\HDV_EC.csv", noxPath = @"data\HDV_NOX.csv";
        if (!File.Exists(ecPath))
        {
            logger.LogError("EC file does not exist: {0}", ecPath);
            return;
        }
        if (!File.Exists(noxPath))
        {
            logger.LogError("NOX file does not exist: {0}", noxPath);
            return;
        }


        string basePath = @"D:\od\OneDrive - EMISIA SA\408 - Vecto web application - Methodology\Development\Vehicles\GLEC Generic";

        void SaveVehicleCopertFile(string vehicle, string sourceCopertFile)
        {
            string vehiclePath = Path.Combine(basePath, vehicle);
            string copertFolder = Path.Combine(vehiclePath, "COPERT");
            if(!Directory.Exists(copertFolder)) Directory.CreateDirectory(copertFolder);

            string targetCopertFile = Path.Combine(copertFolder, Path.GetFileName(sourceCopertFile));

            //vehicle segments
            List<string> vehicleSegment = vehicleSegments[vehicle];

            using(StreamWriter writer = new StreamWriter(targetCopertFile))
            {
                using(StreamReader reader = new StreamReader(sourceCopertFile))
                {
                    string header = reader.ReadLine();
                    writer.WriteLine(header);

                    while(!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        string[] values = line.Split(',');
                        string segment = values[2];

                        if(vehicleSegment.Contains(segment))
                            writer.WriteLine(line);
                    }
                }
            }
        }


        logger.LogInformation("Checking directories for vehicles...");

        foreach (string vehicle in vehicles)
        {
            string vehiclePath = Path.Combine(basePath, vehicle);

            if (!Directory.Exists(vehiclePath))
            {
                logger.LogError("Directory does not exist: {0}", vehiclePath);
                continue;
            }

            SaveVehicleCopertFile(vehicle, ecPath);
            SaveVehicleCopertFile(vehicle, noxPath);
        }


        logger.LogInformation("Done");

    }


}
