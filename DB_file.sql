-- MySQL dump 10.13  Distrib 8.0.36, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: sportscheduler
-- ------------------------------------------------------
-- Server version	8.0.26

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
-- Table structure for table `items`
--

DROP TABLE IF EXISTS `items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `items` (
  `id` varchar(10) NOT NULL,
  `Klub_id` int NOT NULL,
  `name` varchar(50) NOT NULL,
  `price` decimal(10,2) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_Items_Klub1_idx` (`Klub_id`),
  CONSTRAINT `fk_Items_Klub1` FOREIGN KEY (`Klub_id`) REFERENCES `klub` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `items`
--

LOCK TABLES `items` WRITE;
/*!40000 ALTER TABLE `items` DISABLE KEYS */;
INSERT INTO `items` VALUES ('00000001',1,'item1',100.00),('00000002',1,'item2',200.00),('00000003',1,'item3',100.00);
/*!40000 ALTER TABLE `items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `klub`
--

DROP TABLE IF EXISTS `klub`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `klub` (
  `id` int NOT NULL AUTO_INCREMENT,
  `danaValute` int NOT NULL DEFAULT '30',
  `pib` varchar(15) NOT NULL,
  `name` varchar(100) NOT NULL,
  `address` varchar(45) NOT NULL,
  `city` varchar(45) NOT NULL,
  `number` varchar(45) DEFAULT NULL,
  `email` varchar(60) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `klub`
--

LOCK TABLES `klub` WRITE;
/*!40000 ALTER TABLE `klub` DISABLE KEYS */;
INSERT INTO `klub` VALUES (1,30,'123456789','Klub proba','Njegoseva 30','Sremska Mitrovica','0644420296','babic95@gmail.com'),(2,0,'133456789','TK Sirmium','adr','ci',NULL,NULL);
/*!40000 ALTER TABLE `klub` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `liga`
--

DROP TABLE IF EXISTS `liga`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `liga` (
  `id` varchar(36) NOT NULL,
  `Klub_id` int NOT NULL,
  `name` varchar(45) NOT NULL,
  `startDate` datetime NOT NULL,
  `endDate` datetime NOT NULL,
  `imagesFolderName` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_Liga_Klub1_idx` (`Klub_id`),
  CONSTRAINT `fk_Liga_Klub1` FOREIGN KEY (`Klub_id`) REFERENCES `klub` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `liga`
--

LOCK TABLES `liga` WRITE;
/*!40000 ALTER TABLE `liga` DISABLE KEYS */;
/*!40000 ALTER TABLE `liga` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `naplatatermina`
--

DROP TABLE IF EXISTS `naplatatermina`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `naplatatermina` (
  `id` int NOT NULL,
  `Klub_id` int NOT NULL,
  `name` varchar(45) NOT NULL,
  `price` decimal(10,2) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_NaplataTermina_Klub1_idx` (`Klub_id`),
  CONSTRAINT `fk_NaplataTermina_Klub1` FOREIGN KEY (`Klub_id`) REFERENCES `klub` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `naplatatermina`
--

LOCK TABLES `naplatatermina` WRITE;
/*!40000 ALTER TABLE `naplatatermina` DISABLE KEYS */;
INSERT INTO `naplatatermina` VALUES (0,1,'Fiksni',500.00),(1,1,'Plivajuci',600.00),(2,1,'Trenerski',700.00),(3,1,'Vanredni',800.00),(4,1,'Neclanski',900.00),(5,1,'Klupski',1000.00);
/*!40000 ALTER TABLE `naplatatermina` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `obavestenja`
--

DROP TABLE IF EXISTS `obavestenja`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `obavestenja` (
  `id` varchar(36) NOT NULL,
  `User_id` int NOT NULL,
  `Termin_id` varchar(36) DEFAULT NULL,
  `description` blob NOT NULL,
  `date` datetime NOT NULL,
  `seen` int NOT NULL DEFAULT '0',
  `prvoSlanje` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_Obavestenja_User1_idx` (`User_id`),
  KEY `fk_Obavestenja_Termin1_idx` (`Termin_id`),
  CONSTRAINT `fk_Obavestenja_Termin1` FOREIGN KEY (`Termin_id`) REFERENCES `termin` (`id`),
  CONSTRAINT `fk_Obavestenja_User1` FOREIGN KEY (`User_id`) REFERENCES `user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `obavestenja`
--

LOCK TABLES `obavestenja` WRITE;
/*!40000 ALTER TABLE `obavestenja` DISABLE KEYS */;
INSERT INTO `obavestenja` VALUES ('02b98903-a397-4aa7-b9e2-94b4b3b92160',1,'05678b25-6b36-44d5-835f-d2b759a1ab62',_binary 'Poštovani korisniče, Vaš termin počinje za 3h. Ukoliko želite da otkažete termin bez naknade molimo Vas da to učinite najkasnije za 30min.','2024-12-12 17:00:00',0,0),('1b6b1f89-80c6-40b1-8033-e4ec0aa16713',1,'d9a39735-9d1b-464f-b894-a73fd5337113',_binary 'Poštovani korisniče, Vaš termin počinje za 3h. Ukoliko želite da otkažete termin bez naknade molimo Vas da to učinite najkasnije za 30min.','2024-12-12 16:00:00',1,0),('222c7ea0-c74b-439a-a365-12bcadfcbd68',1,'a7f4f6da-5155-4502-b0c7-fd718ae15956',_binary 'Poštovani korisniče, danas imate termin u 15:0. Ukoliko želite da otkažete termin bez naknade molimo Vas da to učinite najkasnije do 12:-30.','2024-12-13 06:59:59',0,1),('42893523-a622-4dc5-8014-8fa28ba86b15',1,'a7f4f6da-5155-4502-b0c7-fd718ae15956',_binary 'Poštovani korisniče, Vaš termin počinje za 3h. Ukoliko želite da otkažete termin bez naknade molimo Vas da to učinite najkasnije za 30min.','2024-12-13 12:59:58',0,0),('4cd143b3-577e-4f63-9808-5c4944acf99c',1,'68324eb1-f10c-4209-980d-b1053f981fb4',_binary 'Poštovani korisniče, Vaš termin počinje za 3h. Ukoliko želite da otkažete termin bez naknade molimo Vas da to učinite najkasnije za 30min.','2024-12-12 16:00:00',0,0),('70731761-8ee3-4775-be00-40097563f3df',1,'881b588a-e47a-4cbd-b9aa-c94366130b9d',_binary 'Poštovani korisniče, Vaš termin počinje za 3h. Ukoliko želite da otkažete termin bez naknade molimo Vas da to učinite najkasnije za 30min.','2024-12-13 17:59:58',0,0),('7390c02c-1057-4cd4-b9c6-dc2c9e4db187',1,'864904f5-49b6-4d3d-9361-17820819b1aa',_binary 'Poštovani korisniče, danas imate termin u 18:0. Ukoliko želite da otkažete termin bez naknade molimo Vas da to učinite najkasnije do 15:-30.','2024-12-13 06:59:59',0,1),('910fe336-eca7-4961-9101-2e82269b637a',1,'0ac8c3e9-e612-42cf-b19a-69aacad99943',_binary 'Poštovani korisniče, Vaš termin počinje za 3h. Ukoliko želite da otkažete termin bez naknade molimo Vas da to učinite najkasnije za 30min.','2024-12-13 18:59:58',0,0),('aa1ab0d4-bd26-4d01-ab8e-9f56344394f3',1,'0ac8c3e9-e612-42cf-b19a-69aacad99943',_binary 'Poštovani korisniče, danas imate termin u 21:0. Ukoliko želite da otkažete termin bez naknade molimo Vas da to učinite najkasnije do 18:-30.','2024-12-13 06:59:59',0,1),('c3bd9500-99ca-49c9-a982-07963df2dd60',1,'881b588a-e47a-4cbd-b9aa-c94366130b9d',_binary 'Poštovani korisniče, danas imate termin u 20:0. Ukoliko želite da otkažete termin bez naknade molimo Vas da to učinite najkasnije do 17:-30.','2024-12-13 06:59:59',0,1),('edd2686e-253d-42ba-b1d5-8579487bc895',1,'864904f5-49b6-4d3d-9361-17820819b1aa',_binary 'Poštovani korisniče, Vaš termin počinje za 3h. Ukoliko želite da otkažete termin bez naknade molimo Vas da to učinite najkasnije za 30min.','2024-12-13 15:59:58',1,0);
/*!40000 ALTER TABLE `obavestenja` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `popustitermina`
--

DROP TABLE IF EXISTS `popustitermina`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `popustitermina` (
  `id` int NOT NULL,
  `Klub_id` int NOT NULL,
  `name` varchar(45) NOT NULL,
  `popust` decimal(10,2) NOT NULL,
  `typeUser` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_PopustiTermina_Klub1_idx` (`Klub_id`),
  CONSTRAINT `fk_PopustiTermina_Klub1` FOREIGN KEY (`Klub_id`) REFERENCES `klub` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `popustitermina`
--

LOCK TABLES `popustitermina` WRITE;
/*!40000 ALTER TABLE `popustitermina` DISABLE KEYS */;
/*!40000 ALTER TABLE `popustitermina` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `racun`
--

DROP TABLE IF EXISTS `racun`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `racun` (
  `id` varchar(36) NOT NULL,
  `User_id` int NOT NULL,
  `invoiceNumber` varchar(100) NOT NULL,
  `date` datetime NOT NULL,
  `totalAmount` decimal(15,2) NOT NULL,
  `placeno` decimal(15,2) NOT NULL DEFAULT '0.00',
  `otpis` decimal(15,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`id`),
  KEY `fk_Racun_User1_idx` (`User_id`),
  CONSTRAINT `fk_Racun_User1` FOREIGN KEY (`User_id`) REFERENCES `user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `racun`
--

LOCK TABLES `racun` WRITE;
/*!40000 ALTER TABLE `racun` DISABLE KEYS */;
INSERT INTO `racun` VALUES ('3f920be2-ba85-4bfc-af9a-ba2048fdf212',1,'1234567890123-1-1','2024-12-05 14:32:31',500.00,500.00,0.00),('e756932d-9ed2-4f93-8a99-2bd088123dd9',1,'1234567890123-1-2','2024-12-05 14:34:00',600.00,600.00,0.00);
/*!40000 ALTER TABLE `racun` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `racunitem`
--

DROP TABLE IF EXISTS `racunitem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `racunitem` (
  `Racun_id` varchar(36) NOT NULL,
  `Items_id` varchar(10) NOT NULL,
  `name` varchar(100) NOT NULL,
  `quantity` decimal(10,3) NOT NULL,
  `unitPrice` decimal(10,2) NOT NULL,
  `totalAmount` decimal(10,2) NOT NULL,
  PRIMARY KEY (`Racun_id`,`Items_id`),
  KEY `fk_RacunItem_Items1_idx` (`Items_id`),
  CONSTRAINT `fk_RacunItem_Items1` FOREIGN KEY (`Items_id`) REFERENCES `items` (`id`),
  CONSTRAINT `fk_RacunItem_Racun1` FOREIGN KEY (`Racun_id`) REFERENCES `racun` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `racunitem`
--

LOCK TABLES `racunitem` WRITE;
/*!40000 ALTER TABLE `racunitem` DISABLE KEYS */;
INSERT INTO `racunitem` VALUES ('3f920be2-ba85-4bfc-af9a-ba2048fdf212','00000001','item1',1.000,100.00,100.00),('3f920be2-ba85-4bfc-af9a-ba2048fdf212','00000002','item2',1.000,200.00,200.00),('3f920be2-ba85-4bfc-af9a-ba2048fdf212','00000003','item3',2.000,100.00,200.00),('e756932d-9ed2-4f93-8a99-2bd088123dd9','00000001','item1',2.000,100.00,200.00),('e756932d-9ed2-4f93-8a99-2bd088123dd9','00000002','item2',1.000,200.00,200.00),('e756932d-9ed2-4f93-8a99-2bd088123dd9','00000003','item3',2.000,100.00,200.00);
/*!40000 ALTER TABLE `racunitem` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `teren`
--

DROP TABLE IF EXISTS `teren`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `teren` (
  `id` int NOT NULL AUTO_INCREMENT,
  `Klub_id` int NOT NULL,
  `name` varchar(45) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_Teren_Klub1_idx` (`Klub_id`),
  CONSTRAINT `fk_Teren_Klub1` FOREIGN KEY (`Klub_id`) REFERENCES `klub` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `teren`
--

LOCK TABLES `teren` WRITE;
/*!40000 ALTER TABLE `teren` DISABLE KEYS */;
INSERT INTO `teren` VALUES (1,1,'Teren 1');
/*!40000 ALTER TABLE `teren` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `termin`
--

DROP TABLE IF EXISTS `termin`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `termin` (
  `id` varchar(36) NOT NULL,
  `Teren_id` int NOT NULL,
  `User_id` int DEFAULT NULL,
  `TerminLiga_id` varchar(36) DEFAULT NULL,
  `TerminTurnir_id` varchar(36) DEFAULT NULL,
  `startDateTime` datetime NOT NULL,
  `endDateTime` datetime DEFAULT NULL,
  `price` decimal(15,2) NOT NULL,
  `placeno` decimal(15,2) NOT NULL DEFAULT '0.00',
  `dateRezervacije` datetime NOT NULL,
  `otpis` decimal(15,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`id`),
  KEY `fk_Termin_Teren1_idx` (`Teren_id`),
  KEY `fk_Termin_User1_idx` (`User_id`),
  KEY `fk_Termin_TerminLiga1_idx` (`TerminLiga_id`),
  KEY `fk_Termin_TerminTurnir1_idx` (`TerminTurnir_id`),
  CONSTRAINT `fk_Termin_Teren1` FOREIGN KEY (`Teren_id`) REFERENCES `teren` (`id`),
  CONSTRAINT `fk_Termin_TerminLiga1` FOREIGN KEY (`TerminLiga_id`) REFERENCES `terminliga` (`id`),
  CONSTRAINT `fk_Termin_TerminTurnir1` FOREIGN KEY (`TerminTurnir_id`) REFERENCES `terminturnir` (`id`),
  CONSTRAINT `fk_Termin_User1` FOREIGN KEY (`User_id`) REFERENCES `user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `termin`
--

LOCK TABLES `termin` WRITE;
/*!40000 ALTER TABLE `termin` DISABLE KEYS */;
INSERT INTO `termin` VALUES ('02dfe3b9-e472-4323-a609-87f415d5a559',1,6,NULL,NULL,'2025-02-17 15:00:00','2025-02-17 16:00:00',700.00,0.00,'2025-02-17 11:28:49',0.00),('0408e377-dfbf-4f38-961a-34be899a206b',1,6,NULL,NULL,'2025-02-18 14:00:00','2025-02-18 15:00:00',300.00,0.00,'2025-02-17 09:53:30',0.00),('05678b25-6b36-44d5-835f-d2b759a1ab62',1,1,NULL,NULL,'2025-01-12 20:00:00','2025-01-12 21:00:00',700.00,100.00,'2024-12-08 01:17:57',100.00),('05a63f6e-98b0-4bbf-8e54-34e93a54014a',1,2,NULL,NULL,'2025-02-10 11:00:00','2025-02-10 12:00:00',300.00,0.00,'2025-02-04 13:12:11',0.00),('0ac8c3e9-e612-42cf-b19a-69aacad99943',1,1,NULL,NULL,'2025-01-13 21:00:00','2025-01-13 22:00:00',700.00,0.00,'2024-12-08 01:21:14',0.00),('0d0dd6c2-9891-46d1-9ddd-69e81614993a',1,1,NULL,NULL,'2025-01-07 18:00:00','2025-01-07 19:00:00',500.00,500.00,'2024-12-06 16:26:09',0.00),('127badb3-019e-4bab-8437-6fe8146bbbf4',1,2,NULL,NULL,'2025-02-06 15:00:00','2025-02-06 16:00:00',500.00,0.00,'2025-02-03 13:51:29',0.00),('169525c9-da76-442d-b2c8-703a7e6e68ec',1,2,NULL,NULL,'2025-02-09 11:00:00','2025-02-09 12:00:00',300.00,0.00,'2025-02-04 13:58:56',0.00),('21b25095-7295-4fba-9787-5cde939b039f',1,1,NULL,NULL,'2025-01-08 19:00:00','2025-01-08 20:00:00',700.00,700.00,'2024-12-08 00:28:34',0.00),('330a37c5-a5c2-48b5-806f-8e658a36d647',1,2,NULL,NULL,'2025-02-09 21:00:00','2025-02-09 22:00:00',700.00,0.00,'2025-02-04 13:25:26',0.00),('47ff522e-b70c-466d-be45-93c8bf4a5a98',1,1,NULL,NULL,'2025-01-11 18:00:00','2025-01-11 19:00:00',500.00,500.00,'2024-12-09 19:13:53',0.00),('4f9c9bbf-ab72-478b-bf77-8cadba802688',1,1,NULL,NULL,'2025-01-16 17:00:00','2025-01-16 18:00:00',500.00,0.00,'2024-12-12 18:30:39',0.00),('508c4cfc-9e41-47b6-9b45-0a18a89a5183',1,1,NULL,NULL,'2025-01-09 19:00:00','2025-01-09 20:00:00',700.00,700.00,'2024-12-08 01:02:56',0.00),('550b5762-133a-4a2b-9e27-15f9524435f6',1,2,NULL,NULL,'2025-02-06 17:00:00','2025-02-06 18:00:00',500.00,0.00,'2025-02-03 11:45:04',0.00),('55885e6c-cf0e-4f3b-8f90-69df28f56ed1',1,1,NULL,NULL,'2024-12-19 17:00:00','2024-12-19 18:00:00',500.00,0.00,'2024-12-13 13:17:04',0.00),('5cad8f1f-9687-4e56-8dfe-6c731e36c628',1,2,NULL,NULL,'2025-02-09 12:00:00','2025-02-09 13:00:00',300.00,0.00,'2025-02-04 13:22:37',0.00),('6386f037-3e2b-4feb-8e18-dd4bed16b293',1,1,NULL,NULL,'2024-12-09 21:00:00','2024-12-09 22:00:00',700.00,700.00,'2024-12-08 01:04:36',0.00),('640b76cf-d6cf-4f5f-9132-4cc35981e275',1,2,NULL,NULL,'2025-02-09 10:00:00','2025-02-09 11:00:00',300.00,0.00,'2025-02-04 13:56:39',0.00),('68324eb1-f10c-4209-980d-b1053f981fb4',1,1,NULL,NULL,'2024-12-12 16:00:00','2024-12-12 17:00:00',500.00,500.00,'2024-12-10 12:18:38',500.00),('6decb6b6-c9b9-46d2-a517-4d4bd1135556',1,1,NULL,NULL,'2024-12-07 16:00:00','2024-12-07 17:00:00',500.00,500.00,'2024-12-05 17:27:19',0.00),('6fae7d65-bd62-4e58-bc32-e2d967b01eeb',1,2,NULL,NULL,'2025-02-09 13:00:00','2025-02-09 14:00:00',300.00,0.00,'2025-02-04 13:25:09',0.00),('73a6c4fd-aead-4052-86c4-26abccfd6f82',1,1,NULL,NULL,'2024-12-10 15:00:00','2024-12-10 16:00:00',500.00,500.00,'2024-12-09 14:51:45',0.00),('8025dd26-9ba7-42cf-9802-8c977058b99e',1,1,NULL,NULL,'2024-12-18 17:00:00','2024-12-18 18:00:00',500.00,0.00,'2024-12-12 18:33:47',0.00),('864904f5-49b6-4d3d-9361-17820819b1aa',1,1,NULL,NULL,'2024-12-13 18:00:00','2024-12-13 19:00:00',500.00,0.00,'2024-12-09 19:02:02',0.00),('881b588a-e47a-4cbd-b9aa-c94366130b9d',1,1,NULL,NULL,'2024-12-13 20:00:00','2024-12-13 21:00:00',700.00,0.00,'2024-12-10 12:02:54',0.00),('88af231b-d50b-44eb-a4b4-894974d722d2',1,2,NULL,NULL,'2025-02-07 11:00:00','2025-02-07 12:00:00',300.00,0.00,'2025-02-04 14:07:13',0.00),('8ed769ef-1ff4-4001-937d-6b1379e09df3',1,1,NULL,NULL,'2024-12-17 19:00:00','2024-12-17 20:00:00',700.00,0.00,'2024-12-12 18:39:42',0.00),('8fd99aca-9252-435d-bab8-77043e489e9f',1,1,NULL,NULL,'2024-12-11 19:00:00','2024-12-11 20:00:00',700.00,700.00,'2024-12-10 12:25:15',0.00),('9a6e67cb-7f20-4849-865a-9d4a0d1d6752',1,2,NULL,NULL,'2025-02-05 11:00:00','2025-02-05 12:00:00',300.00,0.00,'2025-02-03 18:04:46',0.00),('9e148114-69a0-4265-9d46-d80d3522ea89',1,1,NULL,NULL,'2024-12-11 21:00:00','2024-12-11 22:00:00',700.00,700.00,'2024-12-09 19:18:49',400.00),('a2e8bbf3-1c17-4d51-99eb-ac89b046fe49',1,8,NULL,NULL,'2025-02-18 12:00:00','2025-02-18 13:00:00',900.00,0.00,'2025-02-17 11:40:12',0.00),('a421c37c-c6c7-4e13-a3b2-11efa94f2968',1,2,NULL,NULL,'2025-02-06 12:00:00','2025-02-06 13:00:00',300.00,0.00,'2025-02-04 13:06:24',0.00),('a45d24fb-995e-47ac-9c09-7f976b306ef8',1,1,NULL,NULL,'2024-12-05 16:00:00','2024-12-05 17:00:00',500.00,500.00,'2024-12-05 17:24:22',0.00),('a558869e-75c7-4834-a554-4ce89013cd1c',1,2,NULL,NULL,'2025-02-08 13:00:00','2025-02-08 14:00:00',300.00,0.00,'2025-02-03 18:02:52',0.00),('a7f4f6da-5155-4502-b0c7-fd718ae15956',1,1,NULL,NULL,'2024-12-13 15:00:00','2024-12-13 16:00:00',500.00,0.00,'2024-10-09 19:01:25',0.00),('a98aefaa-1812-44b4-bd69-a4c076e08cf1',1,5,NULL,NULL,'2025-02-17 16:00:00','2025-02-17 17:00:00',600.00,0.00,'2025-02-17 11:29:11',0.00),('b96648a8-29e2-4cb5-aea7-060b16c18508',1,1,NULL,NULL,'2024-12-11 15:00:00','2024-12-11 16:00:00',500.00,500.00,'2024-12-09 18:21:12',0.00),('ba6f4883-3ed2-4afe-8547-38c2ca751b21',1,1,NULL,NULL,'2024-12-17 17:00:00','2024-12-17 18:00:00',500.00,0.00,'2024-12-13 13:17:47',0.00),('bf89389e-335f-4a61-9fc3-8095549498e9',1,1,NULL,NULL,'2024-12-10 20:00:00','2024-12-10 21:00:00',700.00,700.00,'2024-12-08 01:10:08',0.00),('c90d607a-b59c-492d-a4e9-567e52fc69f8',1,2,NULL,NULL,'2025-02-06 20:00:00','2025-02-06 21:00:00',700.00,0.00,'2025-02-03 11:39:40',0.00),('d3583b4f-64e4-4f06-8e1d-6a48b4864126',1,4,NULL,NULL,'2025-02-17 12:00:00','2025-02-17 13:00:00',500.00,0.00,'2025-02-17 11:41:05',0.00),('d992ea12-99dc-4461-8b66-ad704d12e9ee',1,2,NULL,NULL,'2025-02-07 10:00:00','2025-02-07 11:00:00',300.00,0.00,'2025-02-04 13:06:09',0.00),('d9a39735-9d1b-464f-b894-a73fd5337113',1,1,NULL,NULL,'2024-12-12 17:00:00','2024-12-12 18:00:00',500.00,500.00,'2024-12-09 19:03:59',500.00),('dab37ee4-636b-4974-addc-589b512caa5c',1,2,NULL,NULL,'2025-02-08 11:00:00','2025-02-08 12:00:00',300.00,0.00,'2025-02-04 14:01:01',0.00),('e198cb04-e79e-43f2-8fd4-6060596698f0',1,6,NULL,NULL,'2025-02-18 19:00:00','2025-02-18 20:00:00',700.00,0.00,'2025-02-17 09:58:58',0.00),('e1b5f587-8d9e-4c95-873f-c05c7f800c48',1,2,NULL,NULL,'2025-02-09 15:00:00','2025-02-09 16:00:00',500.00,0.00,'2025-02-04 15:38:01',0.00),('e50ea9d6-6ca0-4258-bb04-97afaad82bd6',1,1,NULL,NULL,'2024-12-09 20:00:00','2024-12-09 21:00:00',700.00,700.00,'2024-12-08 01:03:35',0.00),('e584d824-ef4f-4a91-8c9d-846887509ab1',1,2,NULL,NULL,'2025-02-08 10:00:00','2025-02-08 11:00:00',300.00,0.00,'2025-02-04 13:55:58',0.00),('e8798936-1abd-42d0-af91-132463e86060',1,1,NULL,NULL,'2024-12-05 20:00:00','2024-12-05 22:00:00',700.00,700.00,'2024-12-05 17:25:34',0.00),('ee63a07b-fcc4-42c0-95bf-153a29eca529',1,2,NULL,NULL,'2025-02-08 17:00:00','2025-02-08 18:00:00',500.00,0.00,'2025-02-04 13:19:43',0.00),('f5fe5b31-304c-4736-8001-41fcc57dc72b',1,2,NULL,NULL,'2025-02-08 12:00:00','2025-02-08 13:00:00',300.00,0.00,'2025-02-04 14:04:53',0.00),('fdf5dc7f-e733-4d64-94a6-880931ff8e24',1,1,NULL,NULL,'2024-12-10 19:00:00','2024-12-10 20:00:00',700.00,700.00,'2024-12-10 12:41:10',0.00);
/*!40000 ALTER TABLE `termin` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `terminliga`
--

DROP TABLE IF EXISTS `terminliga`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `terminliga` (
  `id` varchar(36) NOT NULL,
  `Liga_id` varchar(36) NOT NULL,
  `Type` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_MachLiga_Liga1_idx` (`Liga_id`),
  CONSTRAINT `fk_MachLiga_Liga1` FOREIGN KEY (`Liga_id`) REFERENCES `liga` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `terminliga`
--

LOCK TABLES `terminliga` WRITE;
/*!40000 ALTER TABLE `terminliga` DISABLE KEYS */;
/*!40000 ALTER TABLE `terminliga` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `terminturnir`
--

DROP TABLE IF EXISTS `terminturnir`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `terminturnir` (
  `id` varchar(36) NOT NULL,
  `Turnir_id` varchar(36) NOT NULL,
  `Type` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_TerminTurnir_Turnir1_idx` (`Turnir_id`),
  CONSTRAINT `fk_TerminTurnir_Turnir1` FOREIGN KEY (`Turnir_id`) REFERENCES `turnir` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `terminturnir`
--

LOCK TABLES `terminturnir` WRITE;
/*!40000 ALTER TABLE `terminturnir` DISABLE KEYS */;
/*!40000 ALTER TABLE `terminturnir` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `turnir`
--

DROP TABLE IF EXISTS `turnir`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `turnir` (
  `id` varchar(36) NOT NULL,
  `Klub_id` int NOT NULL,
  `startDate` datetime NOT NULL,
  `endDate` datetime NOT NULL,
  `name` varchar(100) NOT NULL,
  `imagesFolderName` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_Turnir_Klub1_idx` (`Klub_id`),
  CONSTRAINT `fk_Turnir_Klub1` FOREIGN KEY (`Klub_id`) REFERENCES `klub` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `turnir`
--

LOCK TABLES `turnir` WRITE;
/*!40000 ALTER TABLE `turnir` DISABLE KEYS */;
/*!40000 ALTER TABLE `turnir` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ucesnikliga`
--

DROP TABLE IF EXISTS `ucesnikliga`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ucesnikliga` (
  `User_id` int NOT NULL,
  `Liga_id` varchar(36) NOT NULL,
  `datumPrijave` datetime NOT NULL,
  PRIMARY KEY (`User_id`,`Liga_id`),
  KEY `fk_UcesnikLiga_Liga1_idx` (`Liga_id`),
  CONSTRAINT `fk_UcesnikLiga_Liga1` FOREIGN KEY (`Liga_id`) REFERENCES `liga` (`id`),
  CONSTRAINT `fk_UcesnikLiga_User1` FOREIGN KEY (`User_id`) REFERENCES `user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ucesnikliga`
--

LOCK TABLES `ucesnikliga` WRITE;
/*!40000 ALTER TABLE `ucesnikliga` DISABLE KEYS */;
/*!40000 ALTER TABLE `ucesnikliga` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ucesnikturnir`
--

DROP TABLE IF EXISTS `ucesnikturnir`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ucesnikturnir` (
  `User_id` int NOT NULL,
  `Turnir_id` varchar(36) NOT NULL,
  `datumPrijave` datetime NOT NULL,
  PRIMARY KEY (`User_id`,`Turnir_id`),
  KEY `fk_UcesnikTurnir_User1_idx` (`User_id`),
  KEY `fk_UcesnikTurnir_Turnir1` (`Turnir_id`),
  CONSTRAINT `fk_UcesnikTurnir_Turnir1` FOREIGN KEY (`Turnir_id`) REFERENCES `turnir` (`id`),
  CONSTRAINT `fk_UcesnikTurnir_User1` FOREIGN KEY (`User_id`) REFERENCES `user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ucesnikturnir`
--

LOCK TABLES `ucesnikturnir` WRITE;
/*!40000 ALTER TABLE `ucesnikturnir` DISABLE KEYS */;
/*!40000 ALTER TABLE `ucesnikturnir` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `uplata`
--

DROP TABLE IF EXISTS `uplata`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `uplata` (
  `id` varchar(36) NOT NULL,
  `User_id` int NOT NULL,
  `totalAmount` decimal(15,2) NOT NULL,
  `date` datetime NOT NULL,
  `typeUplata` int NOT NULL,
  `razduzeno` decimal(15,2) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_Uplata_User1_idx` (`User_id`),
  CONSTRAINT `fk_Uplata_User1` FOREIGN KEY (`User_id`) REFERENCES `user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `uplata`
--

LOCK TABLES `uplata` WRITE;
/*!40000 ALTER TABLE `uplata` DISABLE KEYS */;
INSERT INTO `uplata` VALUES ('8e084716-bcc1-42fd-afcd-e8692df78f5f',1,1500.00,'2024-12-12 17:16:01',1,1500.00),('a99cd0ef-2f23-470d-821f-af027e267948',1,10000.00,'2024-12-12 13:50:36',0,10000.00);
/*!40000 ALTER TABLE `uplata` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user`
--

DROP TABLE IF EXISTS `user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user` (
  `id` int NOT NULL AUTO_INCREMENT,
  `Klub_id` int NOT NULL,
  `fullName` varchar(100) NOT NULL,
  `password` varchar(45) NOT NULL,
  `username` varchar(45) NOT NULL,
  `type` int NOT NULL,
  `birthday` datetime NOT NULL,
  `contact` varchar(45) NOT NULL,
  `email` varchar(60) NOT NULL,
  `freeTermin` int NOT NULL DEFAULT '0',
  `jmbg` varchar(13) NOT NULL,
  `profileImageUrl` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_User_Klub1_idx` (`Klub_id`),
  CONSTRAINT `fk_User_Klub1` FOREIGN KEY (`Klub_id`) REFERENCES `klub` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user`
--

LOCK TABLES `user` WRITE;
/*!40000 ALTER TABLE `user` DISABLE KEYS */;
INSERT INTO `user` VALUES (1,1,'Test123 Test123','test12','test123',0,'1995-07-18 00:00:00','06444202546','test123@gmail.com',0,'1234567890123','https://ccs.fra1.cdn.digitaloceanspaces.com/CcsSportScheduler/test123_document.png'),(2,1,'aaa aaa','aaa','aaa',1,'1995-07-18 00:00:00','06444202546','aaa@gmail.com',0,'3216549872365','https://ccs.fra1.cdn.digitaloceanspaces.com/CcsSportScheduler/aaa_e.png'),(3,1,'moderator pezime','mod1','mod',6,'1995-07-18 00:00:00','06444202546','aaa@gmail.com',0,'3216549872365','https://ccs.fra1.cdn.digitaloceanspaces.com/CcsSportScheduler/mod_coffee.png'),(4,1,'Fiksni Fiksni','fik','fik',0,'1995-07-18 00:00:00','06444202546','aaa@gmail.com',0,'3216549872365',NULL),(5,1,'Plivajuci Plivajuci','pli','pli',1,'1995-07-18 00:00:00','06444202546','aaa@gmail.com',0,'3216549872365',NULL),(6,1,'Trenerski Trenerski','tre','tre',2,'1995-07-18 00:00:00','06444202546','aaa@gmail.com',0,'3216549872365',NULL),(7,1,'Vanredni Vanredni','van','van',3,'1995-07-18 00:00:00','06444202546','aaa@gmail.com',0,'3216549872365',NULL),(8,1,'Neclanski Neclanski','nec','nec',4,'1995-07-18 00:00:00','06444202546','aaa@gmail.com',0,'3216549872365',NULL),(9,1,'Klupski Klupski','klu','klu',5,'1995-07-18 00:00:00','06444202546','aaa@gmail.com',0,'3216549872365',NULL);
/*!40000 ALTER TABLE `user` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `vestiliga`
--

DROP TABLE IF EXISTS `vestiliga`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `vestiliga` (
  `id` varchar(36) NOT NULL,
  `Liga_id` varchar(36) NOT NULL,
  `description` blob NOT NULL,
  PRIMARY KEY (`id`,`Liga_id`),
  KEY `fk_VestiLiga_Liga1_idx` (`Liga_id`),
  CONSTRAINT `fk_VestiLiga_Liga1` FOREIGN KEY (`Liga_id`) REFERENCES `liga` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `vestiliga`
--

LOCK TABLES `vestiliga` WRITE;
/*!40000 ALTER TABLE `vestiliga` DISABLE KEYS */;
/*!40000 ALTER TABLE `vestiliga` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `vestiturnir`
--

DROP TABLE IF EXISTS `vestiturnir`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `vestiturnir` (
  `id` varchar(36) NOT NULL,
  `Turnir_id` varchar(36) NOT NULL,
  `description` blob NOT NULL,
  PRIMARY KEY (`id`,`Turnir_id`),
  KEY `fk_VestiTurnir_Turnir1_idx` (`Turnir_id`),
  CONSTRAINT `fk_VestiTurnir_Turnir1` FOREIGN KEY (`Turnir_id`) REFERENCES `turnir` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `vestiturnir`
--

LOCK TABLES `vestiturnir` WRITE;
/*!40000 ALTER TABLE `vestiturnir` DISABLE KEYS */;
/*!40000 ALTER TABLE `vestiturnir` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-17 12:04:32
