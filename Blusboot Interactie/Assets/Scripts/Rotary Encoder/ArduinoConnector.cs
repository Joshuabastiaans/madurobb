using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class ArduinoConnector : MonoBehaviour
{
    private SerialPort serialPort;
    public string portName = "COM3";  // Update this to your specific port
    public int baudRate = 115200;     // Ensure this matches the Arduino baud rate

    private Thread serialThread;
    private ConcurrentQueue<string> dataQueue = new ConcurrentQueue<string>();
    private bool isRunning = false;

    private long encoderPosition = 0;   // Stores the latest encoder position
    private bool switchPressed = false; // Indicates if the switch was pressed

    void Start()
    {
        serialPort = new SerialPort(portName, baudRate);
        try
        {
            serialPort.Open();
            serialPort.ReadTimeout = 50;
            serialPort.NewLine = "\r\n"; // Matches Arduino's Serial.println() line endings
            Debug.Log("Serial Port Opened");

            isRunning = true;
            serialThread = new Thread(ReadSerial);
            serialThread.Start();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error opening serial port: " + e.Message);
        }
    }

    void ReadSerial()
    {
        while (isRunning)
        {
            try
            {
                string data = serialPort.ReadLine();
                dataQueue.Enqueue(data);
            }
            catch (System.TimeoutException)
            {
                // Ignore timeout exceptions and continue reading
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Serial read error: " + ex.Message);
            }
        }
    }

    void Update()
    {
        while (dataQueue.TryDequeue(out string data))
        {
            Debug.Log("Data Received: " + data);

            // Parse the incoming data
            if (data.StartsWith("P"))
            {
                string positionString = data.Substring(1).Trim();
                if (long.TryParse(positionString, out long position))
                {
                    // Update the encoder position
                    encoderPosition = position;
                }
            }
            else if (data.StartsWith("S"))
            {
                // Set the flag to change color
                switchPressed = true;
            }
        }

        // Apply rotation based on the encoder position
        float degreesPerStep = 1.0f; // Adjust this scaling factor as needed
        float rotationAngle = (encoderPosition * degreesPerStep) % 360f; // Wraps around at 360 degrees

        transform.rotation = Quaternion.Euler(0, rotationAngle, 0);

        // Change color if the switch was pressed
        if (switchPressed)
        {
            switchPressed = false; // Reset the flag

            // Get the Renderer component
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                // Assign a random color
                renderer.material.color = new Color(Random.value, Random.value, Random.value);
            }
            else
            {
                Debug.LogWarning("Renderer component not found on the game object.");
            }
        }
    }

    void OnDestroy()
    {
        isRunning = false;

        if (serialThread != null && serialThread.IsAlive)
        {
            serialThread.Join();
        }

        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }
}
