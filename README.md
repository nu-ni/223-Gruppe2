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
- **Optimistische Sperrung**: Versionierung verwenden, um sicherzustellen, dass keine Daten überschrieben werden, die sich seit dem letzten Lesen geändert haben.
- **Wiederholte Versuche**: Bei einem Fehler die gesamte Operation in einer Schleife neu starten.
- Die genaue Implementation ist in der Methode Book im Bookingrepository zu finden.

### Transaktionssicherheit bei neuen Features

- **Löschen eines Kontos**: Beim Löschen eines Kontos wird eine Transaktion verwendet, um sicherzustellen, dass der Löschvorgang atomar ist und bei einem Fehler zurückgerollt werden kann. Dies verhindert inkonsistente Zustände in der Datenbank. So kann ein Konto nicht gelöscht werden, wenn beispielsweise noch eine Überweisung aussteht.
- **Erstellen eines Kontos**: Hier ist keine Transaktion nötig.