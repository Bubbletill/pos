using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT_POS.Integrations.Hardware;

public class SerialLineDisplay : IDisposable
{
    private readonly SerialPort _port;

    // Common POS character bytes (CP437 / Epson-compatible)
    private const byte PoundSymbol = 0x9C; // £

    public SerialLineDisplay(string portName, int baudRate = 9600)
    {
        _port = new SerialPort(portName, baudRate)
        {
            Parity = Parity.None,
            DataBits = 8,
            StopBits = StopBits.One,
            Encoding = Encoding.ASCII,
            Handshake = Handshake.None
        };

        _port.Open();
        Clear();
    }

    /// <summary>
    /// Clears the line display
    /// </summary>
    public void Clear()
    {
        // Form Feed (FF) – clear display
        _port.Write(new byte[] { 0x0C }, 0, 1);
    }

    /// <summary>
    /// Displays two lines of text.
    /// Automatically converts £ to the correct POS byte.
    /// </summary>
    public void Show(string line1, string line2)
    {
        Clear();
        WriteText(line1);
        _port.Write("\r\n");
        WriteText(line2);
    }

    /// <summary>
    /// Writes text to the display, translating unsupported characters.
    /// </summary>
    private void WriteText(string text)
    {
        foreach (char c in text)
        {
            if (c == '£')
            {
                _port.Write(new[] { PoundSymbol }, 0, 1);
            }
            else
            {
                _port.Write(c.ToString());
            }
        }
    }

    public void Dispose()
    {
        if (_port.IsOpen)
            _port.Close();

        _port.Dispose();
    }
}
