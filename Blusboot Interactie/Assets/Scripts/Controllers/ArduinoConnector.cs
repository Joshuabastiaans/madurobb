using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class ArduinoConnector : MonoBehaviour
{
    private SerialPort serialPort;
    public string portName = "COM3";  // Update to match your Arduino COM port
    public int baudRate = 115200;     // Matches the Arduino's Serial.begin(115200)

    private Thread serialThread;
    private ConcurrentQueue<string> dataQueue = new ConcurrentQueue<string>();
    private bool isRunning = false;

    // --- Events ---
    // Rotation example events (if you still use an Encoder or "S" switch)
    public event Action<int> OnRotationChanged; // +1 or -1
    public event Action OnSwitchPressed;

    // --- Pot 1 & 2 for Player 1 ---
    public event Action<int> OnPotentiometer1Changed;
    public event Action<int> OnPotentiometer2Changed;

    // --- Pot 1 & 2 for Player 2 ---
    public event Action<int> OnPotentiometer3Changed;
    public event Action<int> OnPotentiometer4Changed;

    public event Action<int> OnRotationChangedPlayer1;
    public event Action<int> OnRotationChangedPlayer2;

    void Start()
    {
        serialPort = new SerialPort(portName, baudRate);
        try
        {
            serialPort.Open();
            serialPort.ReadTimeout = 50;
            // The Arduino code uses println(), which by default is "\r\n"
            serialPort.NewLine = "\r\n";

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

    private void ReadSerial()
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
                // Ignore timeouts
            }
            catch (Exception ex)
            {
                Debug.LogError("Serial read error: " + ex.Message);
            }
        }
    }

    void Update()
    {
        // Process all queued lines from the serial thread
        while (dataQueue.TryDequeue(out string data))
        {
            // Debug.Log("Data Received: " + data);

            // --- Interpret the data format ---
            if (data == "+1")
            {
                // Example: If you still have an encoder or similar
                OnRotationChanged?.Invoke(1);
            }
            else if (data == "-1")
            {
                OnRotationChanged?.Invoke(-1);
            }
            else if (data == "S")
            {
                // Some switch press
                OnSwitchPressed?.Invoke();
            }
            else if (data.StartsWith("P1:"))
            {
                // Player 1 pot #1
                string valueString = data.Substring(3).Trim();
                if (int.TryParse(valueString, out int potValue1))
                {
                    OnPotentiometer1Changed?.Invoke(potValue1);
                }
            }
            else if (data.StartsWith("P2:"))
            {
                // Player 1 pot #2
                string valueString = data.Substring(3).Trim();
                if (int.TryParse(valueString, out int potValue2))
                {
                    OnPotentiometer2Changed?.Invoke(potValue2);
                }
            }
            else if (data.StartsWith("P3:"))
            {
                // Player 2 pot #1
                string valueString = data.Substring(3).Trim();
                if (int.TryParse(valueString, out int potValue3))
                {
                    OnPotentiometer3Changed?.Invoke(potValue3);
                }
            }
            else if (data.StartsWith("P4:"))
            {
                // Player 2 pot #2
                string valueString = data.Substring(3).Trim();
                if (int.TryParse(valueString, out int potValue4))
                {
                    OnPotentiometer4Changed?.Invoke(potValue4);
                }
            }
            else
            {
                Debug.LogWarning("Unknown data received: " + data);
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
