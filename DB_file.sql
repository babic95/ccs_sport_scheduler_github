-- MySQL Workbench Synchronization
-- Generated: 2024-12-06 13:16
-- Model: New Model
-- Version: 1.0
-- Project: Name of the project
-- Author: Babic

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

ALTER TABLE `sportscheduler`.`User` 
DROP FOREIGN KEY `fk_User_Klub1`;

ALTER TABLE `sportscheduler`.`Termin` 
DROP FOREIGN KEY `fk_Termin_Teren1`,
DROP FOREIGN KEY `fk_Termin_TerminLiga1`,
DROP FOREIGN KEY `fk_Termin_TerminTurnir1`;

ALTER TABLE `sportscheduler`.`Teren` 
DROP FOREIGN KEY `fk_Teren_Klub1`;

ALTER TABLE `sportscheduler`.`Turnir` 
DROP FOREIGN KEY `fk_Turnir_Klub1`;

ALTER TABLE `sportscheduler`.`VestiTurnir` 
DROP FOREIGN KEY `fk_VestiTurnir_Turnir1`;

ALTER TABLE `sportscheduler`.`TerminTurnir` 
DROP FOREIGN KEY `fk_TerminTurnir_Turnir1`;

ALTER TABLE `sportscheduler`.`Liga` 
DROP FOREIGN KEY `fk_Liga_Klub1`;

ALTER TABLE `sportscheduler`.`VestiLiga` 
DROP FOREIGN KEY `fk_VestiLiga_Liga1`;

ALTER TABLE `sportscheduler`.`TerminLiga` 
DROP FOREIGN KEY `fk_MachLiga_Liga1`;

ALTER TABLE `sportscheduler`.`Obavestenja` 
DROP FOREIGN KEY `fk_Obavestenja_Termin1`;

ALTER TABLE `sportscheduler`.`UcesnikLiga` 
DROP FOREIGN KEY `fk_UcesnikLiga_Liga1`;

ALTER TABLE `sportscheduler`.`UcesnikTurnir` 
DROP FOREIGN KEY `fk_UcesnikTurnir_Turnir1`,
DROP FOREIGN KEY `fk_UcesnikTurnir_User1`;

ALTER TABLE `sportscheduler`.`Items` 
DROP FOREIGN KEY `fk_Items_Klub1`;

ALTER TABLE `sportscheduler`.`Racun` 
DROP FOREIGN KEY `fk_Racun_User1`;

ALTER TABLE `sportscheduler`.`RacunItem` 
DROP FOREIGN KEY `fk_RacunItem_Items1`;

ALTER TABLE `sportscheduler`.`Uplata` 
DROP FOREIGN KEY `fk_Uplata_User1`;

ALTER TABLE `sportscheduler`.`NaplataTermina` 
DROP FOREIGN KEY `fk_NaplataTermina_Klub1`;

ALTER TABLE `sportscheduler`.`PopustiTermina` 
DROP FOREIGN KEY `fk_PopustiTermina_Klub1`;

ALTER TABLE `sportscheduler`.`User` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`Termin` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`Teren` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`Turnir` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`VestiTurnir` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`TerminTurnir` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`Liga` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`VestiLiga` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`TerminLiga` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`Obavestenja` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ,
ADD COLUMN `prvoSlanje` INT(11) NOT NULL AFTER `seen`;

ALTER TABLE `sportscheduler`.`UcesnikLiga` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`UcesnikTurnir` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ,
DROP INDEX `fk_UcesnikTurnir_Turnir1` ;
;

ALTER TABLE `sportscheduler`.`Items` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`Racun` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`RacunItem` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`Uplata` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`NaplataTermina` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`PopustiTermina` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`Klub` 
CHARACTER SET = utf8 , COLLATE = utf8_general_ci ;

ALTER TABLE `sportscheduler`.`User` 
ADD CONSTRAINT `fk_User_Klub1`
  FOREIGN KEY (`Klub_id`)
  REFERENCES `sportscheduler`.`Klub` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`Termin` 
DROP FOREIGN KEY `fk_Termin_User1`;

ALTER TABLE `sportscheduler`.`Termin` ADD CONSTRAINT `fk_Termin_Teren1`
  FOREIGN KEY (`Teren_id`)
  REFERENCES `sportscheduler`.`Teren` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_Termin_User1`
  FOREIGN KEY (`User_id`)
  REFERENCES `sportscheduler`.`User` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_Termin_TerminLiga1`
  FOREIGN KEY (`TerminLiga_id`)
  REFERENCES `sportscheduler`.`TerminLiga` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_Termin_TerminTurnir1`
  FOREIGN KEY (`TerminTurnir_id`)
  REFERENCES `sportscheduler`.`TerminTurnir` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`Teren` 
ADD CONSTRAINT `fk_Teren_Klub1`
  FOREIGN KEY (`Klub_id`)
  REFERENCES `sportscheduler`.`Klub` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`Turnir` 
ADD CONSTRAINT `fk_Turnir_Klub1`
  FOREIGN KEY (`Klub_id`)
  REFERENCES `sportscheduler`.`Klub` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`VestiTurnir` 
ADD CONSTRAINT `fk_VestiTurnir_Turnir1`
  FOREIGN KEY (`Turnir_id`)
  REFERENCES `sportscheduler`.`Turnir` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`TerminTurnir` 
ADD CONSTRAINT `fk_TerminTurnir_Turnir1`
  FOREIGN KEY (`Turnir_id`)
  REFERENCES `sportscheduler`.`Turnir` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`Liga` 
ADD CONSTRAINT `fk_Liga_Klub1`
  FOREIGN KEY (`Klub_id`)
  REFERENCES `sportscheduler`.`Klub` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`VestiLiga` 
ADD CONSTRAINT `fk_VestiLiga_Liga1`
  FOREIGN KEY (`Liga_id`)
  REFERENCES `sportscheduler`.`Liga` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`TerminLiga` 
ADD CONSTRAINT `fk_MachLiga_Liga1`
  FOREIGN KEY (`Liga_id`)
  REFERENCES `sportscheduler`.`Liga` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`Obavestenja` 
DROP FOREIGN KEY `fk_Obavestenja_User1`;

ALTER TABLE `sportscheduler`.`Obavestenja` ADD CONSTRAINT `fk_Obavestenja_User1`
  FOREIGN KEY (`User_id`)
  REFERENCES `sportscheduler`.`User` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_Obavestenja_Termin1`
  FOREIGN KEY (`Termin_id`)
  REFERENCES `sportscheduler`.`Termin` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`UcesnikLiga` 
DROP FOREIGN KEY `fk_UcesnikLiga_User1`;

ALTER TABLE `sportscheduler`.`UcesnikLiga` ADD CONSTRAINT `fk_UcesnikLiga_User1`
  FOREIGN KEY (`User_id`)
  REFERENCES `sportscheduler`.`User` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_UcesnikLiga_Liga1`
  FOREIGN KEY (`Liga_id`)
  REFERENCES `sportscheduler`.`Liga` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`UcesnikTurnir` 
ADD CONSTRAINT `fk_UcesnikTurnir_Turnir1`
  FOREIGN KEY (`Turnir_id`)
  REFERENCES `sportscheduler`.`Turnir` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_UcesnikTurnir_User1`
  FOREIGN KEY (`User_id`)
  REFERENCES `sportscheduler`.`User` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`Items` 
ADD CONSTRAINT `fk_Items_Klub1`
  FOREIGN KEY (`Klub_id`)
  REFERENCES `sportscheduler`.`Klub` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`Racun` 
ADD CONSTRAINT `fk_Racun_User1`
  FOREIGN KEY (`User_id`)
  REFERENCES `sportscheduler`.`User` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`RacunItem` 
DROP FOREIGN KEY `fk_RacunItem_Racun1`;

ALTER TABLE `sportscheduler`.`RacunItem` ADD CONSTRAINT `fk_RacunItem_Racun1`
  FOREIGN KEY (`Racun_id`)
  REFERENCES `sportscheduler`.`Racun` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_RacunItem_Items1`
  FOREIGN KEY (`Items_id`)
  REFERENCES `sportscheduler`.`Items` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`Uplata` 
ADD CONSTRAINT `fk_Uplata_User1`
  FOREIGN KEY (`User_id`)
  REFERENCES `sportscheduler`.`User` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`NaplataTermina` 
ADD CONSTRAINT `fk_NaplataTermina_Klub1`
  FOREIGN KEY (`Klub_id`)
  REFERENCES `sportscheduler`.`Klub` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `sportscheduler`.`PopustiTermina` 
ADD CONSTRAINT `fk_PopustiTermina_Klub1`
  FOREIGN KEY (`Klub_id`)
  REFERENCES `sportscheduler`.`Klub` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
