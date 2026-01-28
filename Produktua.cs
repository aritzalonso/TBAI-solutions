using System;

namespace TiketBai_Kudeaketa // <--- KONTUZ IZENAREKIN
{
    public class Produktua // <--- PUBLIC jarri behar du
    {
        public int Id { get; set; }
        public string Izena { get; set; } // <--- PUBLIC
        public double Prezioa { get; set; } // <--- PUBLIC

        public Produktua(int id, string izena, double prezioa)
        {
            this.Id = id;
            this.Izena = izena;
            this.Prezioa = prezioa;
        }
    }
}