using System;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Device.Gpio;
using nanoFramework.Networking;
using System.Device.I2c;
using nanoFramework.Hardware.Esp32;

namespace IoT.Esp32
{
    public class Program
    {
        private const int SdaPin = 21;
        private const int SclPin = 22;
        private const int LcdI2cAddress = 0x27; // Change to 0x3F if blank

        public static void Main()
        {
            Configuration.SetPinFunction(SdaPin, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(SclPin, DeviceFunction.I2C1_CLOCK);

            var i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, LcdI2cAddress));

            // Use our custom, lightweight driver instead of the bloated library
            var lcd = new MinimalI2cLcd(i2cDevice);

            // Keep line 1 static
            lcd.SetCursor(0, 0);
            lcd.WriteText("Status:_____");
            Debug.WriteLine("starting");

            lcd.SetCursor(0, 1);
            lcd.WriteText("Starting");

            // Connect to Wi-Fi:
            //WiFi details...
            const string ssid = "HUAWEI_E5577_F75F";
            const string password = "L94RNLA3M1M";

            lcd.SetCursor(0, 1);
            lcd.WriteText($"Con-ing to Wi-Fi");


            //Helper to connect and wait for DHCP IP address
            bool connected = WifiNetworkHelper.ConnectDhcp(ssid, password, requiresDateTime: false);

            //Switch off if not connected
            if (!connected)
            {
                lcd.SetCursor(0, 0);
                lcd.WriteText("Failed - Wi-Fi");
                Thread.Sleep(Timeout.Infinite);
            }

            //Retrieve IP address
            var ipAddress = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPv4Address;

            lcd.SetCursor(0, 0);
            lcd.WriteText("IP:" + ipAddress);

            //Configure onBoard LED pin
            var gpioController = new GpioController();
            var ledPin = gpioController.OpenPin(2, PinMode.Output);
            ledPin.Write(PinValue.Low); // Turn off LED initially



            //Start Web Server
            HttpListener listener = new HttpListener("http", 80);
            
                listener.Start();
            lcd.SetCursor(0, 1);
                lcd.WriteText("Web started-await..");

                while(true)
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    lcd.SetCursor(0, 0);
                lcd.WriteText($"Incoming:       ");
                lcd.SetCursor(0, 1);
                lcd.WriteText($"{request.RawUrl}");

                // Route: /control?action=...
                if (request.RawUrl.IndexOf("/control?action=") == 0)
                    {
                        // Extract action string
                        string action = request.RawUrl.Substring("/control?action=".Length);
                        lcd.SetCursor(0, 1);
                    lcd.WriteText($"Act: {action}");

                        if (action == "FLASH_LED")
                        {
                            //Hardware Response: Flash the LED 3 times
                            for (int i = 0; i < 3; i++)
                            {
                                ledPin.Write(PinValue.High); // Turn on LED
                                Thread.Sleep(250);
                                ledPin.Write(PinValue.Low); // Turn off LED
                            }
                        }

                        if (action == "TURN_ON")
                    {
                        ledPin.Write(PinValue.High); //Turn on and leave ON
                    }else if (action == "TURN_OFF")
                    {
                        ledPin.Write(PinValue.Low); // TURN OFF 
                    }

                        //Send HTTP response 200 OK to the background worker
                        string responseString = "{\"status\":\"success\",\"action\":\"" + action + "\"}";
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                        response.ContentType = "application/json";
                        response.ContentLength64 = buffer.Length;
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                        response.OutputStream.Close();

                    }
                    else
                    {
                        //Handle invalid requests
                        lcd.SetCursor(0, 0);
                    lcd.WriteText("Invalid request.");
                    lcd.SetCursor(0, 1);
                    lcd.WriteText("XXXXXXXXXXXXXXXX");
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                        response.OutputStream.Close();
                    }
                }
            

        }
    }

    /// <summary>
    /// A bare-bones, zero-allocation driver for HD44780 LCDs via PCF8574 I2C.
    /// </summary>
    public class MinimalI2cLcd
    {
        private readonly I2cDevice _i2c;
        private const byte Backlight = 0x08; // 0x08 is ON, 0x00 is OFF
        private const byte Enable = 0x04;
        private const byte RegisterSelect = 0x01;

        public MinimalI2cLcd(I2cDevice i2c)
        {
            _i2c = i2c;

            // HD44780 Initialization Sequence
            Thread.Sleep(50);
            WriteNibble(0x30); Thread.Sleep(5);
            WriteNibble(0x30); Thread.Sleep(1);
            WriteNibble(0x30); Thread.Sleep(1);
            WriteNibble(0x20); // Set to 4-bit mode

            WriteCommand(0x28); // 2 lines, 5x8 matrix
            WriteCommand(0x0C); // Display ON, Cursor OFF
            WriteCommand(0x01); // Clear display
            Thread.Sleep(2);    // Clear takes a bit longer
        }

        private void WriteNibble(byte data)
        {
            // Send data, pulse Enable high, then low
            _i2c.Write(new byte[] { (byte)(data | Backlight) });
            _i2c.Write(new byte[] { (byte)(data | Backlight | Enable) });
            _i2c.Write(new byte[] { (byte)(data | Backlight) });
        }

        public void WriteCommand(byte cmd)
        {
            // Send higher 4 bits, then lower 4 bits (RS = 0 for commands)
            WriteNibble((byte)(cmd & 0xF0));
            WriteNibble((byte)((cmd << 4) & 0xF0));
        }

        public void WriteText(string text)
        {
            foreach (char c in text)
            {
                byte data = (byte)c;
                // Send higher 4 bits, then lower 4 bits (RS = 1 for data)
                WriteNibble((byte)((data & 0xF0) | RegisterSelect));
                WriteNibble((byte)(((data << 4) & 0xF0) | RegisterSelect));
            }
        }

        public void SetCursor(byte col, byte row)
        {
            // 0x00 is row 1 memory start, 0x40 is row 2 memory start
            byte[] rowOffsets = { 0x00, 0x40 };
            WriteCommand((byte)(0x80 | (col + rowOffsets[row])));
        }
    }
}
