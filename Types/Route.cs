namespace NetInspectLib.Types
{
    public class Route
    {
        private int hopNum { get; set; }
        public List<Hop> hops { get; }

        public Route()
        {
            hopNum = 0;
            hops = new List<Hop>();
        }

        public void AddHop(string address, string time)
        {
            Hop hop = new Hop(hopNum, address, time);
            hopNum++;
            hops.Add(hop);
        }
    }
}