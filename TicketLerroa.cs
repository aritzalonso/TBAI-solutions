using TiketBai_Kudeaketa;

namespace TiketBai_Kudeaketa
{
    public class TiketLerroa
    {
        public int Id { get; set; }
        public double Kantitatea { get; set; }
        public Produktua Produktua { get; set; }
        public double Subtotala { get; set; }

        public TiketLerroa(int id, double kantitatea, Produktua p)
        {
            Id = id;
            Kantitatea = kantitatea;
            Produktua = p;
            Subtotala = kantitatea * p.Prezioa;
        }
    }
}