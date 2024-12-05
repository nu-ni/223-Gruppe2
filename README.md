# 223-ma-app

## Autoren

Maurus Brunnschweiler, Nino Nussbaumer, Mois Diabate

## Teilaufgabe 1: Analyse

Wo kann es bei diesem Ablauf bei Multiuserapplikationen zu Problemen kommen? Finden Sie mindestens 3 Probleme.

Mögliche Probleme bei einer Multiuserapplikation wären:

- Concurrent Updates (Gleichzeitige Aktualisierungen): Wenn zwei Transaktionen gleichzeitig ausgeführt werden und gleichzeitig dasselbe Konto aktualisieren, könnte eine Race Condition entstehen, bei der eine der Aktionen nicht korrekt widergespiegelt wird.

- Deadlocks: Wenn verschiedene Transaktionen gleichzeitig verschiedene Konten blockieren und dann versuchen, auf das jeweils andere Konto zuzugreifen, könnte es zu einem Deadlock kommen.

- Nicht persistentes Lesen (Dirty Reads): Eine Transaktion könnte den Wert eines Kontos lesen, während eine andere Transaktion es ändert, was zu inkonsistenten Daten führen könnte.

### Massnahmen:

- **Transaktionen verwenden**: Alle Operationen innerhalb einer Transaktion ausführen, um sicherzustellen, dass sie atomar sind.
- **Wiederholte Versuche**: Bei einem Fehler die gesamte Operation in einer Schleife neu starten.
- Die genaue Implementation ist in der Methode Book im BookingRepository zu finden.

### Transaktionssicherheit bei neuen Features

- **Löschen eines Kontos**: Beim Löschen eines Kontos wird eine Transaktion verwendet, um sicherzustellen, dass der Löschvorgang atomar ist und bei einem Fehler zurückgerollt werden kann. Dies verhindert inkonsistente Zustände in der Datenbank. So kann ein Konto nicht gelöscht werden, wenn beispielsweise noch eine Überweisung aussteht.
- **Erstellen eines Kontos**: Hier ist keine Transaktion nötig.

### Voraussetzungen für den Lasttest

Um den Lasttest mit dem bereitgestellten `booking_scenario` in der `Program.cs` durchzuführen, müssen folgende Voraussetzungen erfüllt sein:

1. **Vorhandene Ledger:** Der SourceLedger und der DestinationLedger müssen bereits in der Datenbank existieren.
2. **Ausreichendes Guthaben:** Der SourceLedger muss über ausreichend Guthaben verfügen, um den Transaktionsbetrag (z. B. `Amount = 1`) abzudecken.


### Sicherstellung Transaktionssicherheit die via Unittests und Lasttests
Wir haben Lasttests hierfür verwendet. Diese Tests simulieren eine hohe Anzahl gleichzeitiger Transaktionen, um sicherzustellen, dass das System unter Last korrekt funktioniert. Wir haben die Summe aller Kontostände zu Beginn geloggt und am Ende. Wenn die Beträge nach den Tests gleich hoch waren, war die Transaktionssicherheit gewährleistet. Das haben wir erreicht.

### Unsere weiteren Features im Backend und Frontend
Wir haben implementiert: Erstellen eines Kontos und Löschen eines Kontos.


### Sequenzdiagramm eines Features
<img width="938" alt="image" src="https://github.com/user-attachments/assets/a2d63d77-4ef8-4499-a964-af49eac08195">
