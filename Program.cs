using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Xml.Linq;
using System.Xml.Schema;

namespace TiketBai_Kudeaketa
{
    class Program
    {
        // ==========================================
        // ⚙️ KONFIGURAZIOA
        // ==========================================
        // Datu-basearekin konektatzeko katea (IPa, erabiltzailea, pasahitza...)
        static string connectionString = "server=127.0.0.1;user=root;database=TicketBaiDB;port=3306;password=1234;AllowPublicKeyRetrieval=True;CharSet=utf8;";
        // Fitxategiak irakurtzeko karpeta nagusia
        static string karpetaNagusia = @"C:\Users\Aritz\Desktop\BASKULAK";
        // Gmail-eko App Password berezia (Segurtasuna)
        static string emailPasahitza = "jkur jqpd jovx ijyl";

        static void Main(string[] args)
        {
            // Kontsolan karaktere bereziak (ñ, euro, azentuak) ondo ikusteko
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            bool irten = false;
            while (!irten)
            {
                // --- MENU NAGUSIA ---
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("=========================");
                Console.WriteLine("   TICKETBAI KUDEAKETA");
                Console.WriteLine("=========================");
                Console.ResetColor();
                Console.WriteLine("1. Prozesatu tiketak");
                Console.WriteLine("2. Estatistikak");
                Console.WriteLine("3. Irten");
                Console.Write("\nAukeratu: ");

                string aukera = Console.ReadLine();

                switch (aukera)
                {
                    case "1": Prozesatu(); break;
                    case "2": Estatistikak(); break;
                    case "3": irten = true; break;
                }
            }
        }

        static void Prozesatu()
        {
            // 1. ERABILTZAILEARI GALDETU
            // Saltzaileak automatikoki hartu edo eskuz zuzendu nahi dituen
            bool galdetuBananBanan = false;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Ondo jarri aldituzu saltzaileak??");
                Console.WriteLine("1. bai, prozesatu tiketak (Automatikoa: 007 -> 8)");
                Console.WriteLine("2. ez, aldatu nahi ditut (Eskuz galdetu)");
                Console.Write("\nAukeratu (1/2): ");
                string modua = Console.ReadLine();

                if (modua == "1") { galdetuBananBanan = false; break; }
                if (modua == "2") { galdetuBananBanan = true; break; }
            }

            Console.WriteLine("\n🔄 HASTEN...");

            // 2. BACKUP KARPETAK PRESTATU
            string backupXML = Path.Combine(karpetaNagusia, "BACKUP", "XML");
            string backupTXT = Path.Combine(karpetaNagusia, "BACKUP", "Tiketak");
            Directory.CreateDirectory(backupXML);
            Directory.CreateDirectory(backupTXT);

            // Fitxategi guztiak bilatu karpetan (*.txt)
            string[] fitxategiak = Directory.GetFiles(karpetaNagusia, "*.txt", SearchOption.AllDirectories);

            // XML erroa sortu
            XElement xmlErroa = new XElement("TicketBai", new XAttribute("DataOrain", DateTime.Now.ToString("s")));

            int kont = 0;
            double xmlTotalaDirua = 0; // Totala metatzeko aldagaia (Log-erako)
            Random rnd = new Random(); // Ausazko IDak sortzeko

            // 3. DATU BASEAREKIN KONEKTATU
            using (MySqlConnection konexioa = new MySqlConnection(connectionString))
            {
                try
                {
                    konexioa.Open();
                    Console.WriteLine("✅ DB konektatuta.");

                    // Saltzaileen izenak memorian kargatu (azkarrago ibiltzeko)
                    Dictionary<int, string> idToName = new Dictionary<int, string>();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT id_saltzailea, izena FROM saltzailea", konexioa))
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            if (!idToName.ContainsKey(r.GetInt32(0)))
                                idToName.Add(r.GetInt32(0), r.GetString(1));
                        }
                    }

                    // --- FITXATEGIAK BANAN-BANAN PROZESATU ---
                    foreach (string fitxategia in fitxategiak)
                    {
                        if (fitxategia.ToUpper().Contains("BACKUP")) continue; // Backup karpetakoak ignoratu
                        Console.WriteLine($"\n📄 Prozesatzen: {Path.GetFileName(fitxategia)}");

                        // Data fitxategiaren izenetik atera
                        DateTime dataTiketa = DateTime.Now;
                        try
                        {
                            string[] zatiakIzena = Path.GetFileNameWithoutExtension(fitxategia).Split('_');
                            dataTiketa = DateTime.ParseExact(zatiakIzena[zatiakIzena.Length - 1], "yyyyMMddHHmmss", null);
                        }
                        catch { }

                        // Saila identifikatu izenaren arabera
                        string saila = "Ezezaguna";
                        if (fitxategia.ToLower().Contains("txarkutegia")) saila = "Txarkutegia";
                        else if (fitxategia.ToLower().Contains("okindegia")) saila = "Okindegia";
                        else if (fitxategia.ToLower().Contains("harategia")) saila = "Harategia";
                        else if (fitxategia.ToLower().Contains("frutategia")) saila = "Frutategia";

                        // Baskula DBn bilatu (ez badago sortu)
                        int idBaskulaDB = LortuEdoSortuId(konexioa, "baskula", "baskula_id", "izena", saila);

                        // Fitxategiaren edukia irakurri
                        string[] lerroak = File.ReadAllLines(fitxategia);

                        if (lerroak.Length > 0)
                        {
                            try
                            {
                                string txtLerroa = lerroak[0];

                                if (konexioa.State != System.Data.ConnectionState.Open) konexioa.Open();

                                string[] zatiak = txtLerroa.Split('$'); // Separadorea '$' da
                                if (zatiak.Length >= 5)
                                {
                                    string prodIzena = zatiak[0].Trim();
                                    string idTestuaFile = zatiak[1].Trim();

                                    // --- LOGIKA BEREZIA: 007 -> 8 ---
                                    int finalId = 1;
                                    if (int.TryParse(idTestuaFile, out int idFitxategitik)) finalId = idFitxategitik + 1;

                                    if (!idToName.ContainsKey(finalId)) finalId = 1; // Existitzen ez bada, 1.a jarri
                                    string finalIzena = idToName[finalId];

                                    // ESKUZKO MODUA (Erabiltzaileak aldatu nahi badu)
                                    if (galdetuBananBanan)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Cyan;
                                        Console.WriteLine($"   🎫 {prodIzena} | File: {idTestuaFile} -> DB: {finalId} ({finalIzena})");
                                        Console.ResetColor();
                                        Console.Write("   Aldatu? (B/E): ");
                                        if (Console.ReadLine().ToUpper() == "B")
                                        {
                                            Console.WriteLine("\n   --- SALTZAILE AUKERAK ---");
                                            foreach (var pertsona in idToName) Console.WriteLine($"   [{pertsona.Key}] {pertsona.Value}");
                                            Console.WriteLine("   -------------------------");
                                            Console.Write("   Sartu ID berria: ");
                                            if (int.TryParse(Console.ReadLine(), out int idBerria) && idToName.ContainsKey(idBerria))
                                            {
                                                finalId = idBerria;
                                                finalIzena = idToName[idBerria];
                                                Console.WriteLine($"   ✅ Aldatuta: {finalIzena}");
                                            }
                                            else Console.WriteLine("   ⚠️ ID okerra. Jatorrizkoa mantenduko da.");
                                        }
                                    }

                                    // Datuak prestatu
                                    int idTiket = rnd.Next(100000, 9999999);
                                    double.TryParse(zatiak[3].Replace("kg", "").Trim().Replace('.', ','), out double kantitatea);
                                    double.TryParse(zatiak[4].Replace("€", "").Trim().Replace('.', ','), out double totala);

                                    xmlTotalaDirua += totala; // Metatu totala

                                    int idProdDB = LortuProduktua(konexioa, prodIzena, idBaskulaDB);

                                    // --- INSERT DATU BASEAN ---
                                    string sql = "INSERT INTO tiketa (id_tiketa, id_produktua, id_saltzailea, kantitatea_kg, prezioa_guztira, data) VALUES (@id, @ip, @is, @kant, @tot, @data)";
                                    using (MySqlCommand cmd = new MySqlCommand(sql, konexioa))
                                    {
                                        cmd.Parameters.AddWithValue("@id", idTiket);
                                        cmd.Parameters.AddWithValue("@ip", idProdDB);
                                        cmd.Parameters.AddWithValue("@is", finalId);
                                        cmd.Parameters.AddWithValue("@kant", kantitatea);
                                        cmd.Parameters.AddWithValue("@tot", totala);
                                        cmd.Parameters.AddWithValue("@data", dataTiketa);
                                        cmd.ExecuteNonQuery();
                                    }

                                    // --- XML NODOA GEHITU ---
                                    Saltzailea s = new Saltzailea(finalId, finalIzena);
                                    xmlErroa.Add(new XElement("Ticket",
                                        new XElement("ID", idTiket),
                                        new XElement("Data", dataTiketa.ToString("yyyy-MM-dd HH:mm:ss")),
                                        new XElement("Saila", saila),
                                        new XElement("Saltzailea", s.Izena),
                                        new XElement("Produktua", new XElement("Izena", prodIzena), new XElement("Kantitatea", kantitatea)),
                                        new XElement("Totala", totala)));

                                    kont++;
                                    if (!galdetuBananBanan) Console.Write(".");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"\n   ❌ ERROREA LERROAN: {ex.Message}");
                                try { konexioa.Close(); konexioa.Open(); } catch { }
                                Console.ResetColor();
                            }
                        }

                        // Fitxategia BACKUP karpetara mugitu (ez prozesatzeko berriro)
                        string helmuga = Path.Combine(backupTXT, DateTime.Now.ToString("HHmmss_") + Path.GetFileName(fitxategia));
                        if (File.Exists(helmuga)) File.Delete(helmuga);
                        File.Move(fitxategia, helmuga);
                    }
                }
                catch (Exception ex) { Console.WriteLine("❌ ERROREA NAGUSIA: " + ex.Message); }

                Console.WriteLine($"\n\nℹ️ GUZTIRA PROZESATUAK: {kont}");

                // --- 4. XSD BALIDAZIOA ETA EMAILA ---
                if (kont > 0)
                {
                    string xmlPath = Path.Combine(backupXML, $"Salmentak_{DateTime.Now:yyyyMMdd_HHmmss}.xml");
                    string xsdPath = Path.Combine(karpetaNagusia, "TicketBai.xsd");

                    Console.WriteLine("\n🛡️ XML Balidatzen...");

                    if (File.Exists(xsdPath))
                    {
                        // BalidatuXML funtzioak egiaztatzen du XSD arauak betetzen diren
                        if (BalidatuXML(xmlErroa, xsdPath))
                        {
                            xmlErroa.Save(xmlPath);
                            Console.WriteLine("✅ XML Balidatua eta Gordeta.");

                            // Ondo badago, Emaila bidali eta Log-a gorde
                            BidaliEmaila(xmlPath, kont, xmlTotalaDirua);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("❌ XML-ak ez du XSD araudia betetzen. Ez da gorde.");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"⚠️ Ez da aurkitu XSD fitxategia hemen: {xsdPath}");
                        Console.ResetColor();
                        xmlErroa.Save(xmlPath);
                    }
                }
            }
            Console.WriteLine("\nSakatu ENTER menura itzultzeko...");
            Console.ReadLine();
        }

        // =========================================================
        // FUNTZIOAK (HELPERRAK)
        // =========================================================

        static void BidaliEmaila(string bidea, int tiketKopurua, double totalaDirua)
        {
            string ogasunEmaila = "aalonso25@izarraitz.eus";
            string egoera = "ERROREA";

            try
            {
                Console.WriteLine("📧 Emaila prestatzen...");
                string nireEmaila = "erronkaprueba@gmail.com";

                // Mezua sortu
                MailMessage m = new MailMessage();
                m.From = new MailAddress(nireEmaila);
                m.To.Add(new MailAddress(ogasunEmaila));
                m.Subject = $"TicketBai XML - {DateTime.Now:yyyy/MM/dd} - {tiketKopurua} Tiket";
                m.Body = $"Kaixo,\n\nHemen atxikita bidaltzen dizut TicketBai XML fitxategia.\n\n" +
                         $"- Tiket kopurua: {tiketKopurua}\n" +
                         $"- Totala: {totalaDirua:0.00} €\n\n" +
                         $"Ondo izan.";

                // XML fitxategia erantsi (adjunto)
                if (System.IO.File.Exists(bidea)) m.Attachments.Add(new Attachment(bidea));

                // SMTP Zerbitzaria konfiguratu (Gmail)
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(nireEmaila, emailPasahitza);
                smtp.EnableSsl = true; // Segurtasuna
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Timeout = 20000;

                Console.WriteLine("🚀 Bidaltzen...");
                smtp.Send(m);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅✅ EMAILA BIDALITA! ✅✅");
                Console.ResetColor();

                egoera = "OK";
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n❌ Email Errorea: " + ex.Message);
                Console.ResetColor();
                egoera = "ERROREA: " + ex.Message.Replace(";", ",");
            }

            // Excel (CSV) Log-a gorde
            GordeLogExcel(Path.GetFileName(bidea), tiketKopurua, totalaDirua, ogasunEmaila, egoera);
        }

        static void GordeLogExcel(string fitxategia, int kopurua, double totala, string email, string egoera)
        {
            try
            {
                string logPath = Path.Combine(karpetaNagusia, "Bidalketa_Log.csv");
                bool existitzenDa = File.Exists(logPath);

                // CSV fitxategia ireki (idazteko moduan)
                using (StreamWriter sw = new StreamWriter(logPath, true))
                {
                    // Fitxategia berria bada, goiburua sortu
                    if (!existitzenDa)
                    {
                        sw.WriteLine("DATA;ORDUA;XML_FITXATEGIA;TIKET_KOP;TOTALA_EUR;NORI;EGOERA");
                    }

                    // Lerro berria gehitu
                    string lerroa = $"{DateTime.Now:yyyy-MM-dd};{DateTime.Now:HH:mm:ss};{fitxategia};{kopurua};{totala:0.00};{email};{egoera}";
                    sw.WriteLine(lerroa);
                }
                Console.WriteLine("📝 Log-a Excelen (CSV) gordeta.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Ezin izan da Log-a gorde: " + ex.Message);
            }
        }

        // XSD Balidazioa egiten duen funtzioa
        static bool BalidatuXML(XElement xml, string xsdPath)
        {
            try
            {
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add("", xsdPath);
                XDocument doc = new XDocument(xml);
                bool ondo = true;

                // Validazio prozesua
                doc.Validate(schemas, (o, e) =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"   ❌ XSD ERROREA: {e.Message}");
                    Console.ResetColor();
                    ondo = false;
                });
                return ondo;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errorea balidatzean: " + ex.Message);
                return false;
            }
        }

        // DBn IDa lortu, edo existitzen ez bada, sortu eta ID berria itzuli
        static int LortuEdoSortuId(MySqlConnection conn, string taula, string idZutabea, string izenZutabea, string balioa)
        {
            try
            {
                // Lehenengo bilatu
                string sql = $"SELECT {idZutabea} FROM {taula} WHERE {izenZutabea} = @val";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@val", balioa);
                    object res = cmd.ExecuteScalar();
                    if (res != null) return Convert.ToInt32(res);
                }
                // Ez badago, txertatu (INSERT)
                string sqlInsert = $"INSERT INTO {taula} ({izenZutabea}) VALUES (@val); SELECT LAST_INSERT_ID();";
                using (MySqlCommand cmd = new MySqlCommand(sqlInsert, conn))
                {
                    cmd.Parameters.AddWithValue("@val", balioa);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch { return 1; }
        }

        // Produktua bilatu edo sortu (Baskulari lotuta)
        static int LortuProduktua(MySqlConnection conn, string izena, int baskulaId)
        {
            try
            {
                string sql = "SELECT id_produktua FROM produktua WHERE produktuaren_izena = @val";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@val", izena);
                    object res = cmd.ExecuteScalar();
                    if (res != null) return Convert.ToInt32(res);
                }
                string sqlInsert = "INSERT INTO produktua (produktuaren_izena, prezioa_kg, baskula_id) VALUES (@val, 0, @bid); SELECT LAST_INSERT_ID();";
                using (MySqlCommand cmd = new MySqlCommand(sqlInsert, conn))
                {
                    cmd.Parameters.AddWithValue("@val", izena);
                    cmd.Parameters.AddWithValue("@bid", baskulaId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch { return 1; }
        }

        static void Estatistikak()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==========================================");
            Console.WriteLine("   📊 ESTATISTIKA AURRERATUAK");
            Console.WriteLine("==========================================");
            Console.ResetColor();

            try
            {
                using (MySqlConnection konexioa = new MySqlConnection(connectionString))
                {
                    konexioa.Open();

                    // 1. ESTATISTIKA OROKORRAK (COUNT, SUM)
                    string sqlOrokorra = "SELECT COUNT(*), SUM(prezioa_guztira), SUM(kantitatea_kg) FROM tiketa";
                    using (MySqlCommand cmd = new MySqlCommand(sqlOrokorra, konexioa))
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read() && !r.IsDBNull(0))
                        {
                            int kopurua = r.GetInt32(0);
                            double dirua = r.IsDBNull(1) ? 0 : r.GetDouble(1);
                            double kiloak = r.IsDBNull(2) ? 0 : r.GetDouble(2);
                            double media = kopurua > 0 ? dirua / kopurua : 0;

                            Console.WriteLine("\n📈 DATU OROKORRAK:");
                            Console.WriteLine($"   - Diru sarrera:    {dirua:0.00} €");
                            Console.WriteLine($"   - Tiket kopurua:   {kopurua} tiket");
                            Console.WriteLine($"   - Kiloak guztira:  {kiloak:0.000} kg");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"   - Batez beste:     {media:0.00} € / tiket");
                            Console.ResetColor();
                        }
                    }
                    Console.WriteLine("------------------------------------------");

                    // 2. TOP SALMENTAK (Helperra erabiliz)
                    ErakutsiTop(konexioa, "SALTZAILEA", "saltzailea", "izena", "id_saltzailea");
                    ErakutsiTop(konexioa, "PRODUKTUA", "produktua", "produktuaren_izena", "id_produktua");
                    Console.WriteLine("------------------------------------------");

                    // 3. SALMENTAK SAILKA (JOIN BIKOITZA)
                    Console.WriteLine("\n🏪 SALMENTAK SAILKA:");
                    string sqlSailak = @"SELECT b.izena, SUM(t.prezioa_guztira) as totala 
                                         FROM tiketa t 
                                         JOIN produktua p ON t.id_produktua = p.id_produktua 
                                         JOIN baskula b ON p.baskula_id = b.baskula_id 
                                         GROUP BY b.izena 
                                         ORDER BY totala DESC";

                    using (MySqlCommand cmd = new MySqlCommand(sqlSailak, konexioa))
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string saila = r.GetString(0);
                            double tot = r.GetDouble(1);
                            Console.WriteLine($"   🔹 {saila.PadRight(15)} : {tot:0.00} €");
                        }
                    }
                    Console.WriteLine("------------------------------------------");

                    // 4. ORDU PUNTAKOA (HOUR funtzioa erabiliz)
                    string sqlOrdua = "SELECT HOUR(data) as ordua, COUNT(*) as kop FROM tiketa GROUP BY ordua ORDER BY kop DESC LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(sqlOrdua, konexioa))
                    {
                        object emaitza = cmd.ExecuteScalar();
                        if (emaitza != null) Console.WriteLine($"⏰ ORDU PUNTAKOA: {emaitza}:00 - {emaitza}:59");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Errorea estatistiketan: " + ex.Message);
            }

            Console.WriteLine("\nSakatu ENTER menura itzultzeko...");
            Console.ReadLine();
        }

        // "TOP" estatistika bat erakusteko funtzio generikoa
        static void ErakutsiTop(MySqlConnection conn, string titulua, string taula, string eremuIzena, string idLotura)
        {
            string sqlMax = $@"SELECT t2.{eremuIzena}, SUM(t1.prezioa_guztira) as totala 
                        FROM tiketa t1 
                        JOIN {taula} t2 ON t1.{idLotura} = t2.{idLotura} 
                        GROUP BY t2.{idLotura} 
                        ORDER BY totala DESC LIMIT 1";

            ImprimatuDatuak(conn, sqlMax, $"🏆 {titulua} ONENA");
        }

        // Datuak kontsolan inprimatzeko helperra
        static void ImprimatuDatuak(MySqlConnection conn, string sql, string goiburua)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string izena = reader.GetString(0);
                        double balioa = reader.GetDouble(1);
                        Console.Write($"{goiburua}: ");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(izena);
                        Console.ResetColor();
                        Console.WriteLine($" ({balioa:0.00} €)");
                    }
                }
            }
            catch { }
        }
    }
}