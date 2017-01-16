namespace UWPAPA102
{
    public delegate void APA102ChangedEvent(object sender);

    public struct LED
    {
        public byte brightness;
        public byte r;
        public byte g;
        public byte b;
    }

    public interface IAPA102FrameBuffer
    {
        event APA102ChangedEvent Changed;

        uint GetLEDFrame(int index);

        LED GetLED(int index);
        void SetLED(int index, LED led);

        int NumberOfLEDs
        {
            get;
        }
    }

    public sealed class APA102ArrayFrameBuffer : IAPA102FrameBuffer
    {
        public event APA102ChangedEvent Changed;
        
        private LED[] leds;

        public APA102ArrayFrameBuffer(int numberOfLEDs)
        {
            leds = new LED[numberOfLEDs];
        }

        public uint GetLEDFrame(int index)
        {
            return leds[index].b * (uint)0x10000 | leds[index].g * (uint)0x100 | (uint)leds[index].r | ((uint)leds[index].brightness & 0x1f) * (uint)0x1000000;
        }

        public int NumberOfLEDs
        {
            get
            {
                return leds.Length;
            }
        }

        public LED GetLED(int index)
        {
            return leds[index];
        }

        public void SetLED(int index, LED led)
        {
            leds[index] = led;

            Changed(this);
        }
    }
}
