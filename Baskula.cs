namespace TiketBai_Kudeaketa
{
    public class Baskula
    {
        public int Id { get; set; }
        public string Izena { get; set; }
        public string Karpeta { get; set; }

        public Baskula(int id, string izena, string karpeta)
        {
            Id = id;
            Izena = izena;
            Karpeta = karpeta;
        }
    }
}