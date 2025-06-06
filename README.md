# BudgetTracker: System Zarządzania Finansami Osobistymi

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## 📝 Opis Projektu

BudgetTracker to osobista aplikacja webowa do zarządzania finansami, zbudowana w technologii ASP.NET Core MVC. Umożliwia użytkownikom efektywne śledzenie swoich wydatków i przychodów, kategoryzowanie transakcji, definiowanie metod płatności oraz ustalanie miesięcznych limitów wydatków. Aplikacja oferuje intuicyjny dashboard z wizualizacjami danych (wykresy, paski postępu limitów) oraz pełnoprawne REST API do programistycznego zarządzania danymi.

Celem projektu jest zapewnienie użytkownikowi pełnej kontroli nad jego finansami, oferując jednocześnie zaawansowane funkcjonalności w prosty i przejrzysty sposób.

## ✨ Kluczowe Funkcjonalności

### Aplikacja Webowa (ASP.NET Core MVC)
*   **Uwierzytelnianie i Autoryzacja:**
    *   System logowania i rejestracji użytkowników.
    *   Sesja do zapamiętania faktu zalogowania.
    *   Hasła przechowywane w bazie danych w postaci bezpiecznego skrótu (hashu).
    *   Automatyczne tworzenie pierwszego konta administratora przy pierwszym uruchomieniu aplikacji.
    *   Administrator ma możliwość dodawania i przeglądania innych użytkowników systemu.
*   **Zarządzanie Transakcjami:**
    *   Rejestrowanie, przeglądanie, edytowanie i usuwanie **wydatków**.
    *   Rejestrowanie, przeglądanie, edytowanie i usuwanie **przychodów**.
    *   Wszystkie operacje CRUD (Create, Read, Update, Delete) realizowane przez interfejs webowy.
*   **Personalizacja Finansów:**
    *   Definiowanie własnych, spersonalizowanych **kategorii** transakcji (z rozróżnieniem na typ "wydatek" lub "przychód", unikalne dla każdego użytkownika).
    *   Definiowanie własnych **metod płatności**.
*   **Budżetowanie:**
    *   Ustalanie **miesięcznych limitów wydatków** na poszczególne kategorie.
    *   Monitorowanie wykorzystania limitów na dashboardzie (paski postępu z kolorami: zielony/żółty/czerwony).
*   **Dashboard Finansowy:**
    *   Podsumowanie łącznych przychodów, wydatków i bilansu za wybrany okres.
    *   Interaktywne wykresy (Chart.js) wizualizujące:
        *   Przychody vs. wydatki.
        *   Wydatki według kategorii.
        *   Przychody według kategorii.
        *   Wydatki według metod płatności.
    *   Pasek postępu pokazujący wykorzystanie limitów dla każdej kategorii wydatków.
*   **Responsywny Interfejs Użytkownika:** Przyjazny dla użytkownika wygląd, dostosowujący się do różnych rozmiarów ekranów.

### REST API
*   **Pełny CRUD:** Możliwość dodawania, usuwania, modyfikacji oraz wyświetlania danych (wydatków, przychodów, kategorii itp.) za pomocą żądań HTTP.
*   **Uwierzytelnianie:**
    *   Autentykacja żądań API odbywa się w oparciu o nazwę użytkownika (przesyłaną jako parametr URL) oraz unikalny **klucz/token API** (przesyłany w nagłówku `X-Api-Key`).
    *   Każdy użytkownik ma przypisany do swojego konta taki klucz, generowany podczas rejestracji lub resetowany na żądanie.
*   **Program Konsolowy:** Program konsolowy znajdujący się w katalogu **BudgetTracker.ApiClient** demonstrujący prawidłowość działania REST API.

## 🛠️ Technologie Użyte w Projekcie

*   **ASP.NET Core MVC:** Główny framework webowy (C#).
*   **Entity Framework Core:** ORM do interakcji z bazą danych (Code-First Migrations).
*   **SQLite:** Lekka, plikowa baza danych.
*   **Chart.js:** Biblioteka JavaScript do generowania interaktywnych wykresów.
*   **JavaScript (Vanilla JS):** Do dynamicznych elementów UI (np. toggle box).
*   **HTML5 / CSS3 (z Bootstrap 5):** Do struktury i stylizacji interfejsu.
*   **C# 12 / .NET 9.0 SDK:** Język programowania i platforma.
*   **Narzędzia CLI:** `dotnet-aspnet-codegenerator`, `dotnet-ef`.

## 🚀 Jak Uruchomić Projekt

Poniższe instrukcje przeprowadzą Cię przez proces konfiguracji i uruchomienia projektu na Twoim lokalnym środowisku deweloperskim.

### Wymagania Wstępne
*   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
*   [Git](https://git-scm.com/downloads)
*   Edytor kodu (np. [Visual Studio Code](https://code.visualstudio.com/), [Visual Studio](https://visualstudio.microsoft.com/vs/), [JetBrains Rider](https://www.jetbrains.com/rider/))
*   (Opcjonalnie) [DB Browser for SQLite](https://sqlitebrowser.org/) do przeglądania bazy danych.

### Klonowanie Repozytorium
1.  Otwórz terminal (np. Git Bash, PowerShell, CMD).
2.  Sklonuj repozytorium na swój komputer:
    ```bash
    git clone https://github.com/YourGitHubUsername/BudgetTracker.git
    ```
3.  Przejdź do głównego katalogu projektu:
    ```bash
    cd BudgetTracker
    ```

### Instalacja Narzędzi i Pakietów
W głównym katalogu projektu (`BudgetTracker/`), uruchom następujące polecenia, aby zainstalować globalne narzędzia .NET i pakiety NuGet:
```bash
# Odinstaluj i zainstaluj/zaktualizuj globalne narzędzia
dotnet tool uninstall --global dotnet-aspnet-codegenerator --verbosity quiet
dotnet tool install --global dotnet-aspnet-codegenerator
dotnet tool uninstall --global dotnet-ef --verbosity quiet
dotnet tool install --global dotnet-ef

# Dodaj pakiety NuGet do projektu (jeśli już nie są dodane w .csproj)
# Standardowo te pakiety powinny być już w pliku .csproj i zostaną zainstalowane podczas buildowania.
# Jeśli brakuje ich po buildzie, możesz spróbować ręcznie:
# dotnet add package Microsoft.EntityFrameworkCore.Tools
# dotnet add package Microsoft.EntityFrameworkCore.Design
# dotnet add package Microsoft.EntityFrameworkCore.SQLite
# dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
# dotnet add package Microsoft.EntityFrameworkCore.SqlServer # Jeśli używasz też SQL Server
```

### Konfiguracja Bazy Danych (SQLite)
Aplikacja używa SQLite, a jej schemat jest tworzony za pomocą migracji Entity Framework Core.
1.  **Utwórz lub zaktualizuj bazę danych:**
    Z głównego katalogu projektu (`BudgetTracker/`), uruchom:
    ```bash
    dotnet ef database update
    ```
    *   To polecenie utworzy plik bazy danych `BudgetTracker.db` (lub inną nazwę zdefiniowaną w `appsettings.json`) w głównym katalogu projektu i zastosuje wszystkie migracje.
    *   **Uwaga:** Plik `BudgetTracker.db` jest ignorowany przez Git (`.gitignore`) i nie znajduje się w repozytorium.
2.  **Inicjalizacja Pierwszego Użytkownika (Administratora):**
    *   Przy pierwszym uruchomieniu aplikacji po utworzeniu bazy danych, system automatycznie sprawdzi, czy istnieje jakikolwiek użytkownik.
    *   Jeśli baza danych użytkowników jest pusta, zostanie utworzone konto administratora z następującymi danymi:
        *   **Username:** `admin`
        *   **Email:** `admin@mail.com`
        *   **Password:** `admin` (Dla celów demonstracyjnych. W rzeczywistym projekcie użyj silnego hasła!)
    *   **Token API:** Dla tego użytkownika zostanie wygenerowany unikalny token API, który będzie potrzebny do korzystania z REST API. Jego wartość zostanie wyświetlona w konsoli serwera podczas uruchamiania aplikacji, gdy konto admina będzie tworzone. **Zanotuj ten token!**

### Uruchamianie Aplikacji Webowej
1.  Z głównego katalogu projektu (`BudgetTracker/`), uruchom:
    ```bash
    dotnet run
    ```
2.  Aplikacja uruchomi się, domyślnie na porcie `https://localhost:7045` (port może się różnić w zależności od konfiguracji `launchSettings.json`).
3.  Otwórz przeglądarkę i przejdź pod wskazany adres.
4.  Zaloguj się jako administrator, używając danych podanych powyżej.

### Uruchamianie Klienta REST API (Program Konsolowy)
Projekt zawiera oddzielną aplikację konsolową (`ApiClientDemo`), która demonstruje interakcję z REST API.

1.  **Przejdź do katalogu klienta API:**
    ```bash
    cd ApiClientDemo
    ```
2.  **Skonfiguruj dane uwierzytelniające:**
    Otwórz plik `ApiClientDemo/Program.cs` w swoim edytorze.
    *   Zaktualizuj `BaseUrl` o prawidłowy adres i port działającej aplikacji webowej (np. `https://localhost:7045/api/`).
    *   Zaktualizuj `ApiKey` o token API, który zanotowałeś podczas inicjalizacji konta administratora.
    ```csharp
    private const string BaseUrl = "https://localhost:7045/api/"; // Zmień na swój port
    private const string Username = "admin";
    private const string string ApiKey = "TWÓJ_WYGENEROWANY_TOKEN_API"; // Wklej tutaj token
    ```
3.  **Uruchom klienta API:**
    Z katalogu `ApiClientDemo/`, uruchom:
    ```bash
    dotnet run
    ```
4.  Program konsolowy wykona szereg żądań do API (GET, POST, PUT, DELETE) i wyświetli wyniki w konsoli. Upewnij się, że Twoja aplikacja webowa (`dotnet run` z `BudgetTracker/`) jest uruchomiona i dostępna, zanim uruchomisz klienta API.

## 📂 Struktura Projektu

*   **`BudgetTracker/`** (Główny projekt aplikacji webowej)
    *   **`Controllers/`**: Kontrolery MVC (np. `ExpenseController`, `HomeController`).
    *   **`Controllers/Api/`**: Kontrolery REST API (np. `ExpensesApiController`).
    *   **`Data/`**: Kontekst bazy danych (`ApplicationDbContext`), pliki migracji (`Migrations/`), inicjalizator danych (`DbInitializer.cs`).
    *   **`Models/`**: Klasy encji bazodanowych (np. `User`, `Expense`, `Category`).
    *   **`Utils/`**: Pomocnicze klasy (np. filtry autoryzacji).
    *   **`ViewModels/`**: Modele widoku dla formularzy i dashboardu (np. `DashboardViewModel`, `RegisterViewModel`).
    *   **`Views/`**: Widoki Razor (.cshtml) dla interfejsu webowego.
    *   **`wwwroot/`**: Statyczne zasoby (CSS, JavaScript, ikona aplikacji `favicon.ico`).
    *   **`appsettings.json`**: Główny plik konfiguracyjny aplikacji.
    *   **`Program.cs`**: Punkt wejścia aplikacji, konfiguracja usług i potoku żądań HTTP.
*   **`ApiClientDemo/`** (Projekt aplikacji konsolowej)
    *   **`Program.cs`**: Logika klienta API.

## 🌐 Endpointy REST API (Przykładowe)

*   **Uwierzytelnianie:** Nagłówek `X-Api-Key: [TWÓJ_TOKEN_API]` oraz parametr URL `username=[NAZWA_UŻYTKOWNIKA]` w każdym żądaniu.
*   **Adres bazowy:** `https://localhost:PORT/api/`

| Metoda | Endpoint                 | Opis                                    | Dane Wejściowe (Body)                      | Dane Wyjściowe (JSON)                  |
| :----- | :----------------------- | :-------------------------------------- | :----------------------------------------- | :------------------------------------- |
| `GET`  | `ExpensesApi`            | Pobiera wszystkie wydatki użytkownika.  | Brak                                       | `List<Expense>`                        |
| `GET`  | `ExpensesApi/{id}`       | Pobiera wydatek po ID.                  | Brak                                       | `Expense`                              |
| `POST` | `ExpensesApi`            | Tworzy nowy wydatek.                    | `ExpenseCreateApiModel`                    | `Expense` (utworzony rekord)           |
| `PUT`  | `ExpensesApi/{id}`       | Aktualizuje wydatek po ID.              | `ExpenseUpdateApiModel`                    | Brak (Status 204 No Content)           |
| `DELETE` | `ExpensesApi/{id}`     | Usuwa wydatek po ID.                    | Brak                                       | Brak (Status 204 No Content)           |

*(Analogiczne endpointy dla `IncomesApi`, `CategoriesApi`, `PaymentMethodsApi` i `MonthlyCategoryBudgetsApi` mogą być zaimplementowane.)*

---

## 🤝 Autorzy

*   Patrick Bajorski

---

## 📄 Licencja

Ten projekt jest udostępniony na licencji MIT. Więcej informacji w pliku [LICENSE](LICENSE).

---