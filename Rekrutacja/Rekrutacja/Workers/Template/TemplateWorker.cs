using Soneta.Business;
using Soneta.Types;
using Rekrutacja.Workers.Template;
using System;
using Soneta.Kadry;
using Soneta.Tools;

//Rejetracja Workera - Pierwszy TypeOf określa jakiego typu ma być wyświetlany Worker, Drugi parametr wskazuje na jakim Typie obiektów będzie wyświetlany Worker
[assembly: Worker(typeof(TemplateWorker), typeof(Pracownicy))]

namespace Rekrutacja.Workers.Template
{
    public class TemplateWorker
    {


        #region deklaracja operatorów

        //////////////////////////////////////////////////////////////
        // Utworzenie operatorów arytmetycznych do wykonywania
        // podstawowych operacji matematycznych
        public enum Operacje
        {

            // Operacja dodawania
            [Caption("+")]
            Dodawanie,

            // Operacja odejmowania
            [Caption("-")]
            Odejmowanie,

            // Operacja mnożenia
            [Caption("*")]
            Mnożenie,

            // Operacja dzielenia
            [Caption("/")]
            Dzielenie

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
            public double ZmiennaA { get; set; }

            // Parametr 'B' używany w operacjach matematycznych.
            [Caption("B"), Priority(2)]
            public double ZmiennaB { get; set; }

            // Data, dla której wykonane będą obliczenia.
            [Caption("Data obliczeń"), Priority(3)]
            public Date DataObliczen { get; set; }

            // Rodzaj operacji matematycznej do wykonania (dodawanie, odejmowanie, itp.).
            [Caption("Operacja"), Priority(4)]
            public Operacje Operacja { get; set; }

            // Konstruktor klasy inicjujący domyślne wartości parametrów.
            public TemplateWorkerParametry(Context context) : base(context)
            {

                // Domyślna wartość parametru 'A'.
                ZmiennaA = 0;

                // Domyślna wartość parametru 'B'.
                ZmiennaB = 0;

                // Domyślna wartość dla daty obliczeń (dzisiejsza data).
                DataObliczen = Date.Today;

                // Domyślna operacja matematyczna - dodawanie.
                Operacja = Operacje.Dodawanie; 

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
           Description = "Prosty kalkulator",
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
        // wybranych parametrów. Wykorzystuje enum Operacje
        // do określenia rodzaju operacji matematycznej.
        public double Obliczenie()
        {

            // Wybór operacji matematycznej na podstawie wartości enum Operacje z parametrów
            switch (Parametry.Operacja)
            {
                case Operacje.Dodawanie:

                    // Zwraca sumę ZmiennaA i ZmiennaB
                    return Parametry.ZmiennaA + Parametry.ZmiennaB;

                case Operacje.Odejmowanie:

                    // Zwraca różnicę ZmiennaA i ZmiennaB
                    return Parametry.ZmiennaA - Parametry.ZmiennaB;

                case Operacje.Mnożenie:

                    // Zwraca iloczyn ZmiennaA i ZmiennaB
                    return Parametry.ZmiennaA * Parametry.ZmiennaB;

                case Operacje.Dzielenie:

                    // Sprawdza, czy dzielnik (ZmiennaB) jest różny od zera
                    if (Parametry.ZmiennaB != 0)
                    {

                        // Zwraca iloraz ZmiennaA przez ZmiennaB
                        return Parametry.ZmiennaA / Parametry.ZmiennaB;
                    }
                    else
                    {

                        // Rzucenie wyjątku, gdy próba dzielenia przez zero
                        throw new InvalidOperationException("Nie można dzielić przez zero.");
                    }

                default:

                    // Rzucenie wyjątku w przypadku nieznanej operacji
                    throw new ArgumentException("Nieznana operacja.");
            }
        }

        #endregion

    }
}
