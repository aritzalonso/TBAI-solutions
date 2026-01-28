-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: TicketBaiDB
-- ------------------------------------------------------
-- Server version	8.0.30

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `baskula`
--

DROP TABLE IF EXISTS `baskula`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `baskula` (
  `baskula_id` int NOT NULL AUTO_INCREMENT,
  `izena` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`baskula_id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `baskula`
--

LOCK TABLES `baskula` WRITE;
/*!40000 ALTER TABLE `baskula` DISABLE KEYS */;
INSERT INTO `baskula` VALUES (1,'Frutategia'),(2,'Harategia'),(3,'Okindegia'),(4,'Txarkuteria'),(5,'Txarkutegia');
/*!40000 ALTER TABLE `baskula` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `produktua`
--

DROP TABLE IF EXISTS `produktua`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `produktua` (
  `id_produktua` int NOT NULL AUTO_INCREMENT,
  `produktuaren_izena` varchar(100) NOT NULL,
  `prezioa_kg` decimal(10,2) NOT NULL,
  `baskula_id` int NOT NULL,
  PRIMARY KEY (`id_produktua`),
  KEY `baskula_id` (`baskula_id`),
  CONSTRAINT `produktua_ibfk_1` FOREIGN KEY (`baskula_id`) REFERENCES `baskula` (`baskula_id`)
) ENGINE=InnoDB AUTO_INCREMENT=25 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `produktua`
--

LOCK TABLES `produktua` WRITE;
/*!40000 ALTER TABLE `produktua` DISABLE KEYS */;
INSERT INTO `produktua` VALUES (1,'anana',8.00,1),(2,'brokolia',5.20,1),(3,'marrubia',16.00,1),(4,'gereziak',12.00,1),(5,'limoiak',20.30,1),(6,'mahatsa',9.00,1),(7,'istarrak',16.00,2),(8,'kostila',20.30,2),(9,'odolkia',18.00,2),(10,'oilaskoa',20.30,2),(11,'txorizoa',18.00,2),(12,'txuleta',18.00,2),(13,'binbo',16.00,3),(14,'croasant',20.30,3),(15,'donuts',18.00,3),(16,'ogia',20.30,3),(17,'pastela',18.00,3),(18,'sandwich',18.00,3),(19,'bacon',11.00,4),(20,'jamonyor',9.00,4),(21,'odolkia',12.00,4),(22,'salami',8.30,4),(23,'txorizoa',9.00,4),(24,'urdaiazpikoa',35.00,4);
/*!40000 ALTER TABLE `produktua` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `saltzailea`
--

DROP TABLE IF EXISTS `saltzailea`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `saltzailea` (
  `id_saltzailea` int NOT NULL,
  `izena` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id_saltzailea`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `saltzailea`
--

LOCK TABLES `saltzailea` WRITE;
/*!40000 ALTER TABLE `saltzailea` DISABLE KEYS */;
INSERT INTO `saltzailea` VALUES (1,'Autosaltzailea'),(2,'Lander'),(3,'Ander'),(4,'Mateo'),(5,'Unai'),(6,'Eber'),(7,'Igor'),(8,'Mario');
/*!40000 ALTER TABLE `saltzailea` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tiketa`
--

DROP TABLE IF EXISTS `tiketa`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tiketa` (
  `id_tiketa` int NOT NULL,
  `id_produktua` int DEFAULT NULL,
  `id_saltzailea` int DEFAULT NULL,
  `kantitatea_kg` decimal(10,3) DEFAULT NULL,
  `prezioa_guztira` decimal(10,2) DEFAULT NULL,
  `data` datetime DEFAULT NULL,
  PRIMARY KEY (`id_tiketa`),
  KEY `id_produktua` (`id_produktua`),
  KEY `id_saltzailea` (`id_saltzailea`),
  CONSTRAINT `tiketa_ibfk_1` FOREIGN KEY (`id_produktua`) REFERENCES `produktua` (`id_produktua`),
  CONSTRAINT `tiketa_ibfk_2` FOREIGN KEY (`id_saltzailea`) REFERENCES `saltzailea` (`id_saltzailea`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tiketa`
--

LOCK TABLES `tiketa` WRITE;
/*!40000 ALTER TABLE `tiketa` DISABLE KEYS */;
INSERT INTO `tiketa` VALUES (2981812,24,4,1.250,43.75,'2026-01-28 11:20:54');
/*!40000 ALTER TABLE `tiketa` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-01-28 13:14:29
