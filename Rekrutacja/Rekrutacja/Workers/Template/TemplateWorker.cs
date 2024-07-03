using Soneta.Business;
using Soneta.Types;
using Rekrutacja.Workers.Template;
using System;
using Soneta.Kadry;
using Soneta.Tools;
using System.Linq;

//Rejetracja Workera - Pierwszy TypeOf określa jakiego typu ma być wyświetlany Worker, Drugi parametr wskazuje na jakim Typie obiektów będzie wyświetlany Worker
[assembly: Worker(typeof(TemplateWorker), typeof(Pracownicy))]

namespace Rekrutacja.Workers.Template
{
    public class TemplateWorker
    {


        #region deklaracja operatorów

        //////////////////////////////////////////////////////////////
        // Utworzenie operatorów geometrycznych do wykonywania
        // operacji matematycznych
        public enum Figury
        {

            // Zmienna geometryczna kwadrat
            [Caption("kwadrat")]
            kwadrat,

            // Zmienna geometryczna prostokąt
            [Caption("prostokąt")]
            prostokat,

            // Zmienna geometryczna trójkąt
            [Caption("trójkąt")]
            trojkat,

            // Zmienna geometryczna koło
            [Caption("koło")]
            kolo

        }

        #endregion

        #region konfiguracja kontrolek w oknie dialogowym

        //////////////////////////////////////////////////////////////
        // Aby parametry działały prawidłowo dziedziczymy
        // po klasie ContextBase, która zapewnia integrację
        // z kontekstem aplikacji Soneta.
        public class TemplateWorkerParametry : ContextBase
        {

            // Parametr 'A' używany w operacjach matematycznych.
            [Caption("A"), Priority(1)]
            public string ZmiennaA { get; set; }

            // Parametr 'B' używany w operacjach matematycznych.
            [Caption("B"), Priority(2)]
            public string ZmiennaB { get; set; }

            // Data, dla której wykonane będą obliczenia.
            [Caption("Data obliczeń"), Priority(3)]
            public Date DataObliczen { get; set; }

            // Rodzaj operacji matematycznej do wykonania (dodawanie, odejmowanie, itp.).
            [Caption("Figura"), Priority(4)]
            public Figury Figura { get; set; }


            // Konstruktor klasy inicjujący domyślne wartości parametrów.
            public TemplateWorkerParametry(Context context) : base(context)
            {

                // Domyślna wartość parametru 'A'.
                ZmiennaA = "0";

                // Domyślna wartość parametru 'B'.
                ZmiennaB = "0";

                // Domyślna wartość dla daty obliczeń (dzisiejsza data).
                DataObliczen = Date.Today;

                // Domyślna operacja matematyczna - dodawanie.
                Figura = Figury.kwadrat; 

            }
        }

        #endregion

        #region deklaracja zmiennych Context

        //////////////////////////////////////////////////////////////
        //Obiekt Context jest to pudełko które przechowuje Typy danych,
        //aktualnie załadowane w aplikacji. Atrybut Context pobiera
        //z "Contextu" obiekty które aktualnie widzimy na ekranie
        [Context]
        public Context Cx { get; set; }



        //////////////////////////////////////////////////////////////
        //Pobieramy z Contextu parametry, jeżeli nie ma w Context
        //Parametrów mechanizm sam utworzy nowy obiekt oraz wyświetli
        //jego formatkę
        [Context]
        public TemplateWorkerParametry Parametry { get; set; }

        #endregion

        #region obsługa okna dialogowego (logika)

        //////////////////////////////////////////////////////////////
        //Atrybut Action - Wywołuje nam metodę która znajduje się poniżej
        [Action("Kalkulator",
           Description = "Prosty kalkulator figur geometrycznych",
           Priority = 10,
           Mode = ActionMode.ReadOnlySession,
           Icon = ActionIcon.Accept,
           Target = ActionTarget.ToolbarWithText)]
        public void WykonajAkcje()
        {

            //Włączenie Debug, aby działał należy wygenerować DLL w trybie DEBUG
            DebuggerSession.MarkLineAsBreakPoint();


            // Pobieranie danych z Contextu
            // KOREKTA BŁEDU
            // z    ...of(Pracownik)))
            // na   ...of(Pracownik[])))
            if (Cx.Contains(typeof(Pracownik[]))) 
            {



                //////////////////////////////////////////////////////////////
                // Utworzenie listy parametrów pracowników
                // Pobieramy tablicę pracowników z kontekstu.
                // Jeśli kontekst zawiera tablicę pracowników (Pracownik[]),
                // przypisujemy ją do zmiennej 'pracownicy'.
                // Dzięki temu mamy dostęp do wszystkich pracowników obecnych w kontekście.
                var pracownicy = (Pracownik[])Cx[typeof(Pracownik[])];

                // Sprawdzenie, czy lista pracowników nie jest pusta
                if (pracownicy != null && pracownicy.Length > 0)
                {
                    // Tworzymy nową sesję, która pozwala na modyfikację obiektów
                    using (Session nowaSesja = Cx.Login.CreateSession(false, false, "ModyfikacjaPracownika"))
                    {
                        // Rozpoczynamy transakcję, aby można było dokonywać zmian w sesji
                        using (ITransaction trans = nowaSesja.Logout(true))
                        {
                            // Iterujemy przez wszystkich pracowników z kontekstu
                            foreach (var pracownik in pracownicy)
                            {
                                // Pobieramy obiekt pracownika z nowo utworzonej sesji
                                var pracownikZSesja = nowaSesja.Get(pracownik);

                                // Modyfikujemy pole 'DataObliczen' w obiekcie pracownika
                                pracownikZSesja.Features["DataObliczen"] = Parametry.DataObliczen;

                                // Modyfikujemy pole 'Wynik' w obiekcie pracownika, przypisując wynik obliczeń
                                pracownikZSesja.Features["Wynik"] = Obliczenie();
                            }
                            // Zatwierdzamy zmiany wykonane w transakcji
                            trans.CommitUI();
                        }
                        // Zapisujemy zmiany w sesji
                        nowaSesja.Save();
                    }
                }
            }
        }

        #endregion

        #region obsługa okna dialogowego (obliczenia)

        //////////////////////////////////////////////////////////////
        // Metoda wykonująca obliczenia matematyczne na podstawie
        // wybranych parametrów. Wykorzystuje enum Figury
        // do określenia rodzaju operacji matematycznej.
        public double Obliczenie()
        {

            // Dodanie zmiennych wewnętrznych parsowanych manualnie
            int intZmiennaA = Parsowanie(Parametry.ZmiennaA);
            int intZmiennaB = Parsowanie(Parametry.ZmiennaB);

            // Wybór operacji matematycznej na podstawie wartości enum Figury z parametrów
            switch (Parametry.Figura)
            {
                case Figury.kwadrat:

                    // Oblicz pole kwadratu
                    int poleKwadratu = intZmiennaA * intZmiennaA;

                    // Zwraca wynik
                    return poleKwadratu;

                case Figury.prostokat:

                    // Oblicz pole prostokąta
                    int poleProstokata = intZmiennaA * intZmiennaB;

                    // Zwraca wynik
                    return poleProstokata;

                case Figury.trojkat:

                    // Oblicz pole trójkąta
                    double poleTrojkata = 0.5 * intZmiennaA * intZmiennaB;

                    // Zwraca wynik
                    return Convert.ToInt32(poleTrojkata);

                case Figury.kolo:

                    // Oblicz pole koła
                    double poleKola = System.Math.PI * intZmiennaA * intZmiennaA;

                    // Zwraca wynik
                    return Convert.ToInt32(poleKola);

                default:

                    // Rzucenie wyjątku w przypadku nieznanej operacji
                    throw new ArgumentException("Nieznana fugura.");
            }
        }

        #endregion

        #region parsowanie stringów na integer

        //////////////////////////////////////////////////////////////
        // Parsuje podany ciąg znaków, filtrując i konwertując tylko cyfry na liczbę całkowitą.
        // Ciąg znaków, który ma zostać przetworzony. Powinien zawierać tylko cyfry, w przeciwnym razie metoda zwraca 0.
        // Wynik konwersji na liczbę całkowitą utworzoną z cyfr znajdujących się w podanym ciągu.
        // Jeśli w ciągu znajdą się znaki nie będące cyframi, metoda zwraca 0.
        // Metoda nie generuje wyjątków, ale jeśli w ciągu znajdują się znaki nie będące cyframi, metoda zwraca 0.
        public int Parsowanie(string zmienna)
        {

            // Tworzenie tablicy cyfr jako char
            char[] cyfry = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            // Inicjalizacja pustego stringa do przechowywania cyfr
            string nowyString = "";

            // Iterowanie przez każdy znak w zmiennej
            foreach (char znak in zmienna)
            {
                // Sprawdzanie, czy znak jest cyfrą
                if (cyfry.Contains(znak))
                {
                    // Dodanie cyfry do nowego stringa
                    nowyString += znak;
                }
                else
                {
                    // Rzucenie wyjątku, gdy napotkano znak, który nie jest cyfrą
                    return 0;
                }
            }

            // Konwersja nowego stringa do liczby całkowitej
            int wynik = Convert.ToInt32(nowyString);

            // Zwrócenie wyniku
            return wynik;

        }

        #endregion 



    }
}
