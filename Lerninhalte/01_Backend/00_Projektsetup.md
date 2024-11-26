# Projekt Setup

Zum start mit dem Modul 223, müssen wir unsere Geräte bereit machen für die Entwicklung von Fullstack applikationen. Fullstack, heisst von der DB bis zum Frontend. Und mit der Datenbank fangen wir an.

### Datenbank Setup

----
#### Für Windows
Teilaufgabe 1: Installation SQL Server Express 2022
Installieren Sie den «SQL Server Express 2022» . Gehen Sie dabei professionell vor und informieren Sie sich zuerst über das Produkt und die Installation. Sie können dabei das Web oder auch YouTube  verwenden. Gehen Sie bei der Installation sorgfältig vor und kontrollieren Sie Ihre Einstellungen jeweils. [sql express](https://go.microsoft.com/fwlink/p/?linkid=2216019&clcid=0x409&culture=en-us&country=us)

Optional: Wenn Sie nicht möchten, dass der SQL-Server mit dem Start von Windows automatisch läuft und Ressourcen verbraucht, folgen Sie der Anleitung , um den Start auf Manuell umzustellen. Sie dürfen dann aber nicht vergessen, den Server für dieses Modul jeweils manuell zu starten.

Teilaufgabe 2: Microsoft SQL Server Management Studio
Installieren Sie das «Microsoft SQL Server Management Studio» . Gehen Sie dabei professionell vor und informieren Sie sich zuerst über das Produkt und die Installation. [ssms](https://learn.microsoft.com/de-de/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16)

----

#### Für Mac / Linux



Teilaufgabe 1: Installation MariaDB

Installieren Sie MariaDB. Gehen Sie dabei professionell vor und informieren Sie sich zuerst über das Produkt und die Installation. Sie können dabei das Web oder auch YouTube verwenden. Gehen Sie bei der Installation sorgfältig vor und kontrollieren Sie Ihre Einstellungen jeweils. [mariadb](https://mariadb.org/download/)

Teilaufgabe 2: Installation DataGrip

Installieren Sie DataGrip. Gehen Sie dabei professionell vor und informieren Sie sich zuerst über das Produkt und die Installation. Sie können dabei das Web oder auch YouTube verwenden. Gehen Sie bei der Installation sorgfältig vor und kontrollieren Sie Ihre Einstellungen jeweils. [datagrip](https://www.jetbrains.com/datagrip/download/)

ℹ️ℹ️ℹ️ *Alle weiteren Schritte wurden basierend auf SQLExpress beschrieben, funktionieren aber auch für MariaDB, beachten Sie aber die unterschiedlichen Konfigurationseinstellungen.* ℹ️ℹ️ℹ️

----

Teilaufgabe 3: Überprüfen der Installation
Starten Sie das SQL Server Management Studio und loggen Sie sich ein.	 
Erstellen Sie mittels Rechtsklick eine neue Datenbank.	 
Erstellen Sie eine Test-Datenbank.	 
Öffnen Sie eine Query-Konsole.	 
Erstellen Sie eine Tabelle, füllen Sie Daten ein und fragen Sie diese ab.	 

 
Teilaufgabe 4: Verbindungen erlauben
Damit unser C# Programm eine Verbindung aufnehmen kann, muss diese erlaubt werden.
Öffnen Sie den «SQL Server 2022 Configuration Manager» (das ist NICHT das Server Management Studio!)	
Klicken Sie auf «SQL Server Networkl Configuration» und dann auf «Protocols for SQLEXPRESS»

Doppelklicken Sie auf alle Protokolle, die auf «Disabled» stehen und enablen Sie diese.	 
Restarten Sir den Datenbankserver, indem Sie auf «SQL Server Services» gehen und mit der rechten Maustaste Ihren Server anklicken.	 


Gütekriterien
Der Lern- und Arbeitsauftrag ist erfüllt, wenn …
•	Sie mit dem «Microsoft SQL Server Management Studio» auf die lokale Datenbank «SQL Server Express 2022» zugreifen und Tabellen erstellen, Daten abfüllen und abfragen können.
Zusätzliche Angaben zum Auftrag
Zu Teilaufgabe 1:
 
Mögliche Erweiterungsaufträge
•	Sie können Tabellen auch mit einem GUI erstellen, versuchen Sie dies:
 
•	Informieren Sie sich über den MS SQL Server und die Management Console.

### Dev Setup

Holen sie sich das Repo runter, mit dem FE und BE Code.
Dieses finden sie [hier](https://github.com/GF3R/223-ma-app)

Falls noch nicht installiert, laden Sie sich folgende Tools herunter:

Für die Frontendentwicklung:
- [Visual Studio Code](https://code.visualstudio.com/)
- [Node.js](https://nodejs.org/en/)
- [NPM](https://www.npmjs.com/)
Optional können Sie sich auch [NVM](https://www.freecodecamp.org/news/node-version-manager-nvm-install-guide/) installieren, um zwischen verschiedenen Node Versionen zu wechseln.


Für die Backendentwicklung:
- [Visual Studio](https://visualstudio.microsoft.com/de/vs/)

Sonstiges:

- [Git](https://git-scm.com/)

Wenn all dies installiert ist, sollten sie das Frontend und Backend starten können.
Dieses finden sie im Repo unter `frontend` und `backend`.