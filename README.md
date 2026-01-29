# TiketBai Kudeaketa Sistema

Proiektu hau C# (.NET) bidez garatutako kontsola-aplikazio bat da. Bere helburua saltokietako baskulen fitxategiak kudeatzea, datu-base batean gordetzea eta TicketBai sistemaren simulazio bat egitea da (XML sortu, balidatu eta Ogasunera bidali).

## Funtzionalitateak

Programa honek honako prozesu hauek automatizatzen ditu:

* Fitxategien Kudeaketa: Karpeta zehatz batean .txt fitxategiak detektatu eta prozesatzen ditu.
* Datuen Logika: Saltzaileen kodeak zuzentzeko logika inplementatzen du (Adib: 007 fitxategian -> 8 datu-basean).
* Datu-basea (MySQL): Informazio guztia (tiketak, produktuak, saltzaileak...) MySQL datu-base erlazional batean gordetzen du.
* XML TicketBai: TicketBai formatua duen XML fitxategia sortzen du eta XSD eskema baten aurka balidatzen du gordetzeko baimena eman aurretik.
* Email Bidalketa: Balidatutako XML fitxategia automatikoki bidaltzen du Ogasunera (SMTP Gmail erabiliz).
* Trazabilitatea (Log): Bidalketa bakoitza Excel/CSV fitxategi batean erregistratzen du (Data, Ordua, Tiket kopurua eta Egoera).
* Estatistikak: Salmenten datu-analisia eskaintzen du SQL kontsulten bidez (Ordu puntakoak, saltzaile onenak, sailkako salmentak...).

## Erabilitako Teknologiak

* Lengoaia: C# (.NET Framework)
* Datu-basea: MySQL
* IDE: Visual Studio 2022
* Liburutegiak:
    * MySql.Data (Konexiorako)
    * System.Xml.Linq (XML sortzeko)
    * System.Net.Mail (Emailak bidaltzeko)

## Instalazioa eta Erabilera

Proiektu hau probatzeko pauso hauek jarraitu:

1. Datu-basea inportatu:
   Exekutatu TicketBai_DB.sql fitxategia MySQL Workbenchean taulak eta hasierako datuak sortzeko.

2. Konfigurazioa:
   Program.cs fitxategian egiaztatu connectionString aldagaia zure MySQL pasahitzarekin bat datorrela.
   Egiaztatu karpetaNagusia aldagaiaren bidea zure ordenagailukoarekin bat datorrela.

3. Exekutatu:
   Ireki irtenbidea Visual Studio-n eta sakatu Start.

## Datu-basearen Egitura

Proiektuak honako taula hauek erabiltzen ditu:
* baskula: Saltokiaren sailak (Harategia, Frutategia...).
* saltzailea: Langileen informazioa.
* produktua: Saldu diren produktuak eta prezioak.
* tiketa: Eragiketa bakoitzaren erregistro nagusia.
