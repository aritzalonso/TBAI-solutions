using System;
using System.Collections.Generic;

namespace TiketBai_Kudeaketa
{
    public class Tiketa
    {
        public int Id { get; set; }
        public Saltzailea Saltzailea { get; set; }
        public Baskula Baskula { get; set; }
        public List<TiketLerroa> Lerroak { get; set; } = new List<TiketLerroa>();

        public Tiketa(int id, Saltzailea s, Baskula b)
        {
            Id = id;
            Saltzailea = s;
            Baskula = b;
        }

        public double LortuGuztira()
        {
            double totala = 0;
            foreach (var l in Lerroak) totala += l.Subtotala;
            return totala;
        }
    }
}