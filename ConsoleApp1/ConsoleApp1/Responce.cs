namespace ConsoleApp1
{
    public class Responce
    {
        public  uint Ttl;
        public readonly byte[] Rdata;
        public readonly byte[] DataNameOffset;
        public Responce(uint ttl, byte[] rdata, byte[] dataNameOffset)
        {
            Ttl = ttl;
            Rdata = rdata;
            DataNameOffset = dataNameOffset;
        }
    }
}