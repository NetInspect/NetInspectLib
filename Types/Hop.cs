namespace NetInspectLib.Types
{
    public class Hop
    {
        public int number { get; }
        public string address { get; }
        public string time { get; }

        public Hop(int number, string address, string time)
        {
            this.number = number;
            this.address = address;
            this.time = time;
        }
    }
}