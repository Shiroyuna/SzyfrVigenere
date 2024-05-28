using System;
using System.Text;

class Program
{
    static void Main()
    {
        //inicjalizacja zmiennej wyboru użytkownika
        int choice = 0;
        //wczytanie treści plików tekst i klucz do odpowiednich zmiennych
        string tekst = File.ReadAllText("tekst.txt");
        string klucz = File.ReadAllText("klucz.txt");

        
        do
        {
            //menu wyboru, wyświetlające się przy starcie programu
            Console.WriteLine("Chuncia Julia, nr 109284\nSzyfr Vigenere’a:\n");
            Console.WriteLine(" Zaszyfruj tekst (1)");
            Console.WriteLine(" Odszyfruj tekst z kluczem (2)");
            Console.WriteLine(" Odszyfruj te bez klucza/Kryptoanaliza (3)");
            Console.WriteLine(" Zakończ program (4)");
            Console.Write("\nWpisz opcję: ");

            //wpisanie w choice wyboru użytkownika
            try
            {
                choice = Convert.ToInt32(Console.ReadLine());
            }
            //wyświetlenie komunikatu przy błędzie
            catch (Exception)
            {
                Console.WriteLine("Wpisz numer od 1 do 4");
                continue;
            }

            switch (choice)
            {
                case 1:
                    //szyfrowanie tekstu i wpisywanie go do pliku tekst.txt
                    string zaszyfrowanyTekst = SzyfrVigenere(tekst, klucz, true);
                    File.WriteAllText("tekst.txt", zaszyfrowanyTekst);
                    //czyszczenie konsoli, wyświetlanie komunikatu i wyniku
                    Console.Clear();
                    Console.WriteLine("Zaszyfrowany tekst został zapisany w pliku 'tekst.txt'.");
                    Console.WriteLine("Wynik szyfrowania: " + zaszyfrowanyTekst + "\n");
                    break;
                case 2:
                    //wczytywanie zaszyfrowanego tekstu z tekst.txt
                    tekst = File.ReadAllText("tekst.txt");
                    //odszyfrowywanie tekstu i wpisywanie go do pliku odszyfrowany.txt
                    string odszyfrowanyTekst = SzyfrVigenere(tekst, klucz, false);
                    File.WriteAllText("odszyfrowany.txt", odszyfrowanyTekst);
                    //czyszczenie konsoli, wyświetlanie komunikatu i wyniku
                    Console.Clear();
                    Console.WriteLine("Odszyfrowany tekst został zapisany w pliku 'odszyfrowany.txt'.");
                    Console.WriteLine("Wynik odszyfrowania z kluczem: " + odszyfrowanyTekst + "\n");
                    break;
                case 3:
                    //tabela częstotliwości liter w języku angielskim
                    Dictionary<char, double> tabelaCzestotliwosci = new Dictionary<char, double>
                    {
                        { 'A', 0.082 },{ 'B', 0.014 },{ 'C', 0.038 },{ 'D', 0.032 },{ 'E', 0.122 },
                        { 'F', 0.003 },{ 'G', 0.007 },{ 'H', 0.014 },{ 'I', 0.082 },{ 'J', 0.021 },
                        { 'K', 0.034 },{ 'L', 0.041 },{ 'M', 0.022 },{ 'N', 0.071 },{ 'O', 0.079 },
                        { 'P', 0.031 },{ 'R', 0.066 },{ 'S', 0.061 },{ 'T', 0.091 },{ 'U', 0.037 },
                        { 'W', 0.036 },{ 'Y', 0.065 },{ 'Z', 0.036 }
                    };

                    //zliczanie wystąpień liter w tekście
                    var licznikLiter = new Dictionary<char, int>();
                    foreach (var znak in tekst.ToUpper().Where(char.IsLetter))
{
                        if (!licznikLiter.ContainsKey(znak))
                        {
                            licznikLiter[znak] = 0;
                        }
                        licznikLiter[znak]++;
                    }
                    //suma wystąpień wszystkich liter
                    int liczbaLiter = licznikLiter.Values.Sum();

                    //normalizacja częstości liter w tekście
                    var czestotliwoscLiter = licznikLiter.ToDictionary(pair => pair.Key, pair => (double)pair.Value / liczbaLiter);

                    //znalezienie najlepszego przesunięcia klucza
                    var najlepszePrzesuniecie = Enumerable.Range(0, 26).Select(przesuniecie => new
                        {
                            Przesuniecie = przesuniecie,
                            Wynik = tabelaCzestotliwosci.Sum(pair =>
                            {
                                var przesunietaLitera = (char)((pair.Key - 'A' + przesuniecie) % 26 + 'A');
                                var obserwowanaCzestotliwosc = czestotliwoscLiter.ContainsKey(przesunietaLitera) ? czestotliwoscLiter[przesunietaLitera] : 0.0;
                                return Math.Pow(obserwowanaCzestotliwosc - pair.Value, 2);
                            })
                        })
                        .OrderBy(result => result.Wynik)
                        .First()
                        .Przesuniecie;

                    //odszyfrowanie tekstu za pomocą najlepszego przesunięcia
                    var tekstOdszyfrowany = new string(tekst.Select(znak =>
                    {
                        if (!char.IsLetter(znak)) return znak;
                        var offset = char.IsUpper(znak) ? 'A' : 'a';
                        return (char)((((znak - offset - najlepszePrzesuniecie + 26) % 26) + offset));
                    }).ToArray());

                    //zapisanie odszyfrowanego tekstu do pliku kryptoanaliza.txt
                    File.WriteAllText("kryptoanaliza.txt", tekstOdszyfrowany.ToString());
                    //czyszczenie konsoli, wyświetlanie komunikatu i wyniku
                    Console.Clear();
                    Console.WriteLine("Rozszyfrowany tekst został zapisany w pliku 'kryptoanaliza.txt'.");
                    Console.WriteLine("Wynik szyfrowania: " + tekstOdszyfrowany.ToString() + "\n");
                    break;
                case 4:
                    Console.WriteLine("Zamykanie programu..");
                    break;
                default:
                    Console.WriteLine("Wpisz numer od 1 do 4");
                    break;
            }
        } while (choice != 4);
    }

    //metoda do szyfrowania lub odszyfrowywania tekstu szyfrem Vigenere'a
    static string SzyfrVigenere(string tekst, string klucz, bool szyfruj)
    {
        //stringBuilder do przechowywania wyniku
        StringBuilder wynik = new StringBuilder();
        //zamiana klucza na małe litery
        klucz = klucz.ToLower();
        //inicjalizacja indeksu klucza
        int indexKlucza = 0;

        //loop wykonujący działania dla wszystkich znaków w tekście
        foreach (char c in tekst)
        {
            //wykonywanie kodu po sprawdzeniu czy znak jest literą
            if (char.IsLetter(c))
            {
                //określenie offsetu dla dużych i małych liter
                char offset = char.IsUpper(c) ? 'A' : 'a';
                //obliczenie przesunięcia klucza
                int keyOffset = klucz[indexKlucza % klucz.Length] - 'a';
                //jeśli bool szyfruj jest false, następuje odwrócenie przesuniecią dla odszyfrowania
                if (!szyfruj)
                {
                    keyOffset = -keyOffset;
                }
                //obliczenie nowego znaku
                char encryptedChar = (char)((((c - offset) + keyOffset + 26) % 26) + offset);
                //dodanie znaku do wyniku
                wynik.Append(encryptedChar);
                //przesunięcie indeksu klucza
                indexKlucza++;
            }
            //jeśli znak nie jest literą, następuje dodanie go do wyniku
            else
            {
                wynik.Append(c);
            }
        }
        //zwrócenie wyniku zamienionego w string
        return wynik.ToString();
    }
}