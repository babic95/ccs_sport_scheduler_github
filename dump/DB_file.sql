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
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

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
-- Table structure for table `naplatatermina`
--

DROP TABLE IF EXISTS `naplatatermina`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `naplatatermina` (
  `id` int NOT NULL AUTO_INCREMENT,
  `Klub_id` int NOT NULL,
  `name` varchar(45) NOT NULL,
  `startTime` time NOT NULL,
  `endTime` time NOT NULL,
  `price` decimal(10,2) NOT NULL,
  `vikend` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `fk_NaplataTermina_Klub1_idx` (`Klub_id`),
  CONSTRAINT `fk_NaplataTermina_Klub1` FOREIGN KEY (`Klub_id`) REFERENCES `klub` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

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
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

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
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-16 20:08:46
