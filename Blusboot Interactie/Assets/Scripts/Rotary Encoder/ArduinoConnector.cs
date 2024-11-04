using System;
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

    // Events to notify other scripts
    public event Action<int> OnRotationChanged; // int value can be +1 or -1
    public event Action OnSwitchPressed;

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
        catch (Exception e)
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
            catch (TimeoutException)
            {
                // Ignore timeout exceptions and continue reading
            }
            catch (Exception ex)
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
            if (data == "+1")
            {
                // Notify subscribers of rotation change
                OnRotationChanged?.Invoke(1);
            }
            else if (data == "-1")
            {
                OnRotationChanged?.Invoke(-1);
            }
            else if (data == "S")
            {
                // Notify subscribers of switch press
                OnSwitchPressed?.Invoke();
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
