using System;
using System.Runtime.InteropServices;
using Iceoryx2;

namespace ComplexDataTypesExample;

/// <summary>
/// Example struct demonstrating complex data type support.
/// Uses StructLayout(LayoutKind.Sequential) to ensure memory layout compatibility with C/Rust.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[Iox2Type("TransmissionData")]
public struct TransmissionData
{
    public int X;
    public int Y;
    public double Funky;

    public TransmissionData(int x, int y, double funky)
    {
        X = x;
        Y = y;
        Funky = funky;
    }

    public override string ToString()
    {
        return $"TransmissionData {{ x: {X}, y: {Y}, funky: {Funky:F2} }}";
    }
}

/// <summary>
/// More complex example with nested data and arrays.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[Iox2Type("SensorData")]
public struct SensorData
{
    public long Timestamp;
    public float Temperature;
    public float Humidity;
    public int SensorId;

    public SensorData(long timestamp, float temperature, float humidity, int sensorId)
    {
        Timestamp = timestamp;
        Temperature = temperature;
        Humidity = humidity;
        SensorId = sensorId;
    }

    public override string ToString()
    {
        return $"SensorData {{ ts: {Timestamp}, temp: {Temperature:F1}, hum: {Humidity:F1}, id: {SensorId} }}";
    }
}

/// <summary>
/// Example with fixed-size array embedded in the struct.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[Iox2Type("Point3D")]
public unsafe struct Point3D
{
    public fixed float Coordinates[3];
    public int Id;

    public Point3D(float x, float y, float z, int id)
    {
        Coordinates[0] = x;
        Coordinates[1] = y;
        Coordinates[2] = z;
        Id = id;
    }

    public override string ToString()
    {
        return $"Point3D {{ id: {Id}, coords: [{Coordinates[0]:F1}, {Coordinates[1]:F1}, {Coordinates[2]:F1}] }}";
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("iceoryx2 C# Complex Data Types Example");
        Console.WriteLine("======================================\n");

        if (args.Length < 2)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run publisher <type>");
            Console.WriteLine("  dotnet run subscriber <type>");
            Console.WriteLine("Available types: TransmissionData, SensorData, Point3D");
            return;
        }

        var mode = args[0].ToLower();
        var type = args[1];
        var serviceName = $"csharp-{type}-service";

        try
        {
            switch (mode)
            {
                case "publisher":
                    switch (type)
                    {
                        case nameof(TransmissionData):
                            RunPublisher<TransmissionData>(serviceName);
                            break;
                        case nameof(SensorData):
                            RunPublisher<SensorData>(serviceName);
                            break;
                        case nameof(Point3D):
                            RunPublisher<Point3D>(serviceName);
                            break;
                        default:
                            Console.WriteLine($"Unknown type: {type}");
                            break;
                    }
                    break;
                case "subscriber":
                    switch (type)
                    {
                        case nameof(TransmissionData):
                            RunSubscriber<TransmissionData>(serviceName);
                            break;
                        case nameof(SensorData):
                            RunSubscriber<SensorData>(serviceName);
                            break;
                        case nameof(Point3D):
                            RunSubscriber<Point3D>(serviceName);
                            break;
                        default:
                            Console.WriteLine($"Unknown type: {type}");
                            break;
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown mode: {mode}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static unsafe void RunPublisher<T>(string serviceName) where T : unmanaged
    {
        Console.WriteLine($"[Publisher] Starting with type: {typeof(T).Name}");
        Console.WriteLine($"[Publisher] Type size: {sizeof(T)} bytes");
        Console.WriteLine($"[Publisher] Service: {serviceName}");

        // Create a node
        using var node = NodeBuilder.New()
            .Name("csharp-complex-publisher")
            .Create()
            .Expect("Failed to create node");

        // Open or create the service
        using var service = node.ServiceBuilder()
            .PublishSubscribe<T>()
            .Open(serviceName)
            .Expect("Failed to open service");

        // Create a publisher
        using var publisher = service.CreatePublisher()
            .Expect("Failed to create publisher");

        Console.WriteLine("Publisher created successfully. Press Ctrl+C to stop.");

        var counter = 0;
        while (true)
        {
            // For now use SendCopy fallback to avoid loan/send lifecycle bug
            switch (typeof(T).Name)
            {
                case nameof(TransmissionData):
                    var tData = new TransmissionData(counter, counter * 3, counter * 812.12);
                    Console.WriteLine($"Sending(copy): {tData}");
                    publisher.SendCopy(tData).Expect("Failed to send sample via SendCopy");
                    break;

                case nameof(SensorData):
                    var sData = new SensorData(
                        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        20.0f + counter % 10,
                        45.0f + counter % 30,
                        counter % 5);
                    Console.WriteLine($"Sending(copy): {sData}");
                    publisher.SendCopy(sData).Expect("Failed to send sample via SendCopy");
                    break;

                case nameof(Point3D):
                    var pData = CreatePoint3D(counter, counter * 2.0f, counter * 3.0f, counter);
                    Console.WriteLine($"Sending(copy): {pData}");
                    publisher.SendCopy(pData).Expect("Failed to send sample via SendCopy");
                    break;

                default:
                    throw new InvalidOperationException($"Unknown type: {typeof(T).Name}");
            }

            counter++;
            System.Threading.Thread.Sleep(1000);
        }
    }

    static unsafe void RunSubscriber<T>(string serviceName) where T : unmanaged
    {
        Console.WriteLine($"[Subscriber] Starting with type: {typeof(T).Name}");
        Console.WriteLine($"[Subscriber] Service: {serviceName}");

        // Create a node
        using var node = NodeBuilder.New()
            .Name("csharp-complex-subscriber")
            .Create()
            .Expect("Failed to create node");

        // Open the service
        using var service = node.ServiceBuilder()
            .PublishSubscribe<T>()
            .Open(serviceName)
            .Expect("Failed to open service");

        // Create a subscriber
        using var subscriber = service.CreateSubscriber()
            .Expect("Failed to create subscriber");

        Console.WriteLine("Subscriber created. Waiting for samples...\n");

        // Receive data
        while (true)
        {
            var receiveResult = subscriber.Receive<T>();

            if (receiveResult.IsOk)
            {
                var sample = receiveResult.Unwrap();
                if (sample != null)
                {
                    using (sample)
                    {
                        var payload = sample.Payload;
                        Console.WriteLine($"Received: {payload}");
                    }
                }
                // else, no new sample available which is a normal condition
            }
            else
            {
                break;
            }

            System.Threading.Thread.Sleep(100);
        }
    }

    static unsafe Point3D CreatePoint3D(float x, float y, float z, int id)
    {
        var point = new Point3D();
        point.Coordinates[0] = x;
        point.Coordinates[1] = y;
        point.Coordinates[2] = z;
        point.Id = id;
        return point;
    }
}