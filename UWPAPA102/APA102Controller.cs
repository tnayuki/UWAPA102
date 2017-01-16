using System;

using Windows.Devices.Gpio;

namespace UWPAPA102
{
    public sealed class APA102Controller : IDisposable
    {
        private GpioPin dataGpioPin;
        private GpioPin clockGpioPin;

        private IAPA102FrameBuffer frameBuffer;

        public APA102Controller(int dataPinNumber, int clockPinNumber, int numberOfLEDs)
        {
            GpioController gpioController = GpioController.GetDefault();

            dataGpioPin = gpioController.OpenPin(dataPinNumber);
            dataGpioPin.Write(GpioPinValue.Low);
            dataGpioPin.SetDriveMode(GpioPinDriveMode.Output);

            clockGpioPin = gpioController.OpenPin(clockPinNumber);
            clockGpioPin.Write(GpioPinValue.Low);
            clockGpioPin.SetDriveMode(GpioPinDriveMode.Output);

            frameBuffer = new APA102ArrayFrameBuffer(numberOfLEDs);
            frameBuffer.Changed += FrameBuffer_Changed;
        }

        public IAPA102FrameBuffer FrameBuffer
        {
            get
            {
                return frameBuffer;
            }
            set
            {
                frameBuffer.Changed -= FrameBuffer_Changed;

                frameBuffer = value;
                frameBuffer.Changed += FrameBuffer_Changed;
            }
        } 

        private void sendFrame(uint frame)
        {
            for (int i = 0; i < 32; i++)
            {
                dataGpioPin.Write((frame & 0x80000000) != 0 ? GpioPinValue.High : GpioPinValue.Low);
                clockGpioPin.Write(GpioPinValue.High);
                clockGpioPin.Write(GpioPinValue.Low);

                frame <<= 1;
            }
        }

        private void sendFrames()
        {
            dataGpioPin.Write(GpioPinValue.Low);
            for (int i = 0; i < 32; i++)
            {
                clockGpioPin.Write(GpioPinValue.High);
                clockGpioPin.Write(GpioPinValue.Low);
            }

            for (int i = 0; i < frameBuffer.NumberOfLEDs; i++)
            {
                sendFrame(frameBuffer.GetLEDFrame(i));
            }

            dataGpioPin.Write(GpioPinValue.High);
            for (int i = 0; i < 36; i++)
            {
                clockGpioPin.Write(GpioPinValue.High);
                clockGpioPin.Write(GpioPinValue.Low);
            }
        }

        private void FrameBuffer_Changed(object sender)
        {
            sendFrames();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (dataGpioPin != null) dataGpioPin.Dispose();
                    if (clockGpioPin != null) clockGpioPin.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

    }
}
