using System;

#if NETFX_CORE
using Windows.Devices.Gpio;
#endif

namespace UWPAPA102
{
    public sealed class APA102Controller : IDisposable
    {
#if NETFX_CORE
        private GpioPin dataGpioPin;
        private GpioPin clockGpioPin;
#endif

        private IAPA102FrameBuffer frameBuffer;

        public APA102Controller(int dataPinNumber, int clockPinNumber, int numberOfLEDs)
        {
#if NETFX_CORE
            GpioController gpioController = GpioController.GetDefault();

            dataGpioPin = gpioController.OpenPin(dataPinNumber);
            dataGpioPin.Write(GpioPinValue.Low);
            dataGpioPin.SetDriveMode(GpioPinDriveMode.Output);

            clockGpioPin = gpioController.OpenPin(clockPinNumber);
            clockGpioPin.Write(GpioPinValue.Low);
            clockGpioPin.SetDriveMode(GpioPinDriveMode.Output);
#endif
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

#if NETFX_CORE
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
            sendFrame(0x00000000);

            for (int i = 0; i < frameBuffer.NumberOfLEDs; i++)
            {
                sendFrame(frameBuffer.GetLEDFrame(i));
            }

            sendFrame(0xffffffff);
        }
#endif

        private void FrameBuffer_Changed(object sender)
        {
#if NETFX_CORE
            sendFrames();
#endif
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
#if NETFX_CORE
                    if (dataGpioPin != null) dataGpioPin.Dispose();
                    if (clockGpioPin != null) clockGpioPin.Dispose();
#endif
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
